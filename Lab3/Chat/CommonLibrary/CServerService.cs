using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace CommonLibrary
{
    delegate void ThreadWork();

    public class ThreadTask
    {
        private string user;
        private string url;
        private ICClientService client;

        public ThreadTask(string user, string url)
        {
            this.user = user;
            this.url = url;
            this.client = (CClientService)Activator.GetObject(typeof(ICClientService), this.url);
        }

        public int NMessages { get; set; }

        public void updateMessage()
        {
            Console.WriteLine(CServerService.MESSAGES.Count);
            Console.WriteLine(this.NMessages);
            // need lock here?
            List<string> messagesToSend = new List<string>();
            foreach (var message in CServerService.MESSAGES.Skip(NMessages))
            {
                Console.WriteLine("gonna send message: {0}", message);
                messagesToSend.Add(message);
            }
            
            // if there is anything to send
            if (messagesToSend.Count > 0) this.client.sendClient(messagesToSend);
        }

        public void getNumberMessages()
        {
            NMessages = this.client.messagesAmount();
        }
    }

    public class CServerService : MarshalByRefObject, ICServerService
    {
        private Dictionary<string, string> users = new Dictionary<string, string>();
        public static List<string> MESSAGES = new List<string>();
        private Thread[] threadPool = new Thread[0];
        private ThreadTask[] tasks = new ThreadTask[0];
        private ThreadWork[] tasksDelegate = new ThreadWork[0];

        public void addUser(string nickname, string URL)
        {
            try
            {
                lock (this.users)
                {
                    // not sure if we need lock in the threadPool, threadTask and taskDelegates
                    // a user is added so there will be one more thread and one more task
                    lock (this.threadPool)
                    {
                        lock (this.tasks)
                        {
                            lock (this.tasksDelegate)
                            {
                                Array.Resize<ThreadWork>(ref this.tasksDelegate, this.tasksDelegate.Length + 1);
                            }
                            Array.Resize<ThreadTask>(ref this.tasks, this.tasks.Length + 1);
                        }
                        Array.Resize<Thread>(ref this.threadPool, this.threadPool.Length + 1);
                    }
                    this.users.Add(nickname, URL);
                }

                Console.WriteLine("Added user with nickname: {0} and URL: {1}.", nickname, URL);
            } catch (ArgumentException)
            {
                Console.WriteLine("An element with Key = {0} already exists.", nickname);
            }
        }

        public void sendMessage(string nickname, string message)
        {
            lock (MESSAGES)
            {
                string fullMessage = nickname + ":" + message;
                MESSAGES.Add(fullMessage);
                Console.WriteLine(fullMessage);

                // not sure if this should be in or out of the lock
                updateUsers();
            }
        }

        public void updateUsers()
        {
            string[] userNames = this.users.Keys.ToArray();
            int nThreads = userNames.Length;

            for (int i = 0; i < nThreads; i++)
            {
                this.tasks[i] = new ThreadTask(userNames[i], this.users[userNames[i]]);
                
                // we want the thread to first get the number of missing messages and then
                // to send the missing messages when finished
                this.tasksDelegate[i] = new ThreadWork(this.tasks[i].getNumberMessages);
                this.tasksDelegate[i] += tasks[i].updateMessage;

                threadPool[i] = new Thread(new ThreadStart(this.tasksDelegate[i]));
                threadPool[i].Start();
            }
            
        }
    }
}
