using System;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Collections.Generic;
using System.Linq;
using ClassLibrary;

namespace Server
{
    //public delegate void UpdateMessagesDelegate(IClient remoteClient, string nickname, string message);

    public class CServer : MarshalByRefObject, IServer
    {     
        private List<User> users;
        private List<MeetingProposal> currentMeetingProposals;

        // to send messages to clients asynchhronously, otherwise the loop would deadlock
        //public UpdateMessagesDelegate _updateMessagesDelegate;
        //public AsyncCallback _updateMessagesCallback;

        private readonly string SERVER_URL;
        private readonly string SERVER_NAME;

        public CServer(string serverName, int portNumber)
        {
            SERVER_NAME = serverName;
            SERVER_URL = "tcp://localhost:" + portNumber + "/";

            // creates the server's remote object
            RemotingServices.Marshal(this, SERVER_NAME, typeof(CServer));

            users = new List<User>();

            //_updateMessagesDelegate = new UpdateMessagesDelegate(UpdateMessages);
            //_updateMessagesCallback = new AsyncCallback(UpdateMessagesCallback);
        }

        public void RegisterUser(string username, string clientUrl)
        {
            // obtain client remote object
            IClient remoteClient = (IClient)Activator.GetObject(typeof(IClient), clientUrl);
            users.Add(new User(remoteClient, username));

            Console.WriteLine("New user " + username  + " with url " + clientUrl + " registered.");
        }

        public List<MeetingProposal> List()
        {
            Console.WriteLine("Listed meeting proposals.");
            return currentMeetingProposals;
        }

        public void Create(string coordinator, string meetingTopic, int minAttendees, string slots, string invitees = null)
        {
            currentMeetingProposals.Add(new MeetingProposal(coordinator, meetingTopic, minAttendees, ParseSlots(slots), ParseInvitees(invitees)));
            Console.WriteLine("Created new meeting proposal for " + meetingTopic + ".");
        }

        // slots -> “Lisboa,2019-11-14, Porto,2020-02-03” ??
        public List<DateLocation> ParseSlots(string slots)
        {
            List<DataLocation> parsedSlots = new List<DataLocation>();
            slots.Split(',').ToList();
            for
        }

        // invitees -> "Maria", "Miguel"
        public List<string> ParseInvitees(string invitees)
        {
            return invitees.Split(',').ToList();
        }

        static void Main(string[] args) {

            const int SERVER_PORT = 8086;

            // A server has to open a channel only once when the server application launches
            TcpChannel serverChannel = new TcpChannel(SERVER_PORT);
            ChannelServices.RegisterChannel(serverChannel, false);

            // this will create the server's remote object
            CServer server = new CServer("server-1", SERVER_PORT);
            
            System.Console.WriteLine("<enter> para sair...");
			System.Console.ReadLine();
		}
	}
}