using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Text;
using System.Windows.Forms;
using ClassLibrary;

namespace Client
{
    class CClient : MarshalByRefObject, ClientAPI
    {
        //TODO make this for several replicated servers
        private const string SERVER_URL = "tcp://localhost:8086/server-1";
        // obtain server remote object
        private IServer remoteServer;

        private readonly string USERNAME;
        private readonly string CLIENT_URL;

        public CClient(string username, int port)
        {
            TcpChannel clientChannel = new TcpChannel(port);
            ChannelServices.RegisterChannel(clientChannel, false);

            USERNAME = username;
            CLIENT_URL = "tcp://localhost:" + port + "/" + username;

            // create the server's remote object
            RemotingServices.Marshal(this, username, typeof(CClient));
            // retrieve server's proxy
            remoteServer = (IServer)Activator.GetObject(typeof(IServer), SERVER_URL);
            // register new user in remote server
            remoteServer.RegisterUser(username, CLIENT_URL);
        }

        /*public void ListCommands()
        {
            Console.WriteLine("");
        }*/

        public void List()
        {
            List<MeetingProposal> proposals = remoteServer.List();

            //TODO update textbox
        }

        public void Create(string meetingTopic, int minAttendees, string slots, string invitees = null)
        {
            remoteServer.Create(USERNAME, meetingTopic, minAttendees, slots, invitees);
        }

        public void Join(string meetingTopic)
        {

        }

        public void Close(string meetingTopic)
        {

        }

        public void Wait(int milliseconds)
        {

        }

        [STAThread]
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new SchedulingForm());

            /*while()
            {
                int caseSwitch = 1;

                switch (caseSwitch)
                {
                    case 1:
                        Console.WriteLine("Case 1");
                        break;
                    case 2:
                        Console.WriteLine("Case 2");
                        break;
                    default:
                        Console.WriteLine("Default case");
                        break;
                }
            }*/

            System.Console.WriteLine("<enter> para sair...");
            System.Console.ReadLine();
        }
    }
}
