using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Net.Sockets;
using System.Runtime.Remoting.Channels.Tcp;
using System.Windows.Forms;

namespace CommonLibrary
{
    public class CClientService : MarshalByRefObject, ICClientService
    {
        private Form formApp;
        private List<String> messages;
        private Delegate updateMessages;

        public CClientService(Form form, Delegate updateMessages)
        {
            this.formApp = form;
            this.messages = new List<string>();
            this.updateMessages = updateMessages;
        }

        public String Messages
        {
            get
            {
                var allMessages = "";
                foreach (var message in this.messages)
                {
                    allMessages += message;
                    allMessages += "\r\t";
                }

                return allMessages;
            }
        }

        public int messagesAmount()
        {
            return this.messages.Count;
        }

        public void sendClient(List<string> messages)
        {
            Console.WriteLine("Received message: ");
            foreach (string message in messages)
                Console.WriteLine(message);

            this.messages = this.messages.Concat(messages).ToList();

            Console.WriteLine("TotalMessageList: ");
            foreach (string message in this.messages)
                Console.WriteLine(message);

            this.formApp.BeginInvoke(updateMessages, new object[]{Messages});
        }

    }
}
