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
    public class CClient : MarshalByRefObject, IClient
    {
        //TODO make this for several replicated servers
        private const string SERVER_URL = "tcp://localhost:8086/server-1";
        // obtain server remote object
        private IServer _remoteServer;

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
            _remoteServer = (IServer)Activator.GetObject(typeof(IServer), SERVER_URL);
            // register new user in remote server
            _remoteServer.RegisterUser(username, CLIENT_URL);
        }

        /*public void ListCommands()
        {
            Console.WriteLine("");
        }*/

        public void List()
        {
            List<MeetingProposal> proposals = _remoteServer.List(USERNAME);

            foreach(MeetingProposal proposal in proposals)
            {
                Console.WriteLine(proposal);
            }
            
            //TODO update textbox with multithreads!
        }

        public void Create(string meetingTopic, int minAttendees, string slots, string invitees = null)
        {
            List<DateLocation> parsedSlots = ParseSlots(slots);
            List<string> parsedInvitees = ParseInvitees(invitees);
            MeetingProposal proposal = new MeetingProposal {
                Coordinator = USERNAME,
                Topic = meetingTopic,
                MinAttendees = minAttendees,
                DateLocationSlots = parsedSlots,
                Invitees = parsedInvitees,
                Records = new List<MeetingRecord>()
            };
            _remoteServer.Create(proposal);

            // TODO
            if (invitees != null)
            {
                // send to all invitees
            }
            else
            {
                // send to every client
            }

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

        // slots -> Lisboa,2019-11-14 Porto,2020-02-03
        public List<DateLocation> ParseSlots(string slots)
        {
            List<DateLocation> parsedSlots = new List<DateLocation>();
            // each slot is separated by a space
            List<string> splitSlots = slots.Split(' ').ToList();
            foreach (string slot in splitSlots)
            {
                // local and date are separated by a comma
                List<string> slotDetail = slot.Split(',').ToList();
                parsedSlots.Add(new DateLocation(slotDetail[0], slotDetail[1]));
            }

            return parsedSlots;
        }

        // invitees -> Maria, Miguel
        public List<string> ParseInvitees(string invitees)
        {
            return invitees.Split(',').ToList();
        }

        [STAThread]
        static void Main(string[] args)
        {
            CClient client= new CClient("Maria", 8090);

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new SchedulingForm(client));

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
