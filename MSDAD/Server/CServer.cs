using System;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Collections.Generic;
using System.Linq;
using ClassLibrary;
using System.Collections;

namespace Server
{
    //public delegate void UpdateMessagesDelegate(IClient remoteClient, string nickname, string message);

    public class CServer : MarshalByRefObject, IServer
    {     
        private List<User> _users;

        private Hashtable _currentMeetingProposals;

        // to send messages to clients asynchhronously, otherwise the loop would deadlock
        //public UpdateMessagesDelegate _updateMessagesDelegate;
        //public AsyncCallback _updateMessagesCallback;

        private readonly string SERVER_ID;
        private readonly string SERVER_URL;

        public CServer(string serverId, string url, int maxFaults, int minDelay, int maxDelay)
        {
            SERVER_ID = serverId;
            SERVER_URL = url;
   
            TcpChannel serverChannel = new TcpChannel(PortExtractor.Extract(SERVER_URL));
            ChannelServices.RegisterChannel(serverChannel, false);

            // creates the server's remote object
            RemotingServices.Marshal(this, SERVER_ID, typeof(CServer));

            _users = new List<User>();
            _currentMeetingProposals = new Hashtable();

            //_updateMessagesDelegate = new UpdateMessagesDelegate(UpdateMessages);
            //_updateMessagesCallback = new AsyncCallback(UpdateMessagesCallback);
        }

        public void RegisterUser(string username, string clientUrl)
        {
            // obtain client remote object
            IClient remoteClient = (IClient)Activator.GetObject(typeof(IClient), clientUrl);
            _users.Add(new User(remoteClient, username));

            Console.WriteLine("New user " + username  + " with url " + clientUrl + " registered.");
        }

        public List<MeetingProposal> List(string username)
        {
            List<MeetingProposal> userProposals = new List<MeetingProposal>();
            foreach(DictionaryEntry proposal in _currentMeetingProposals)
            {
                MeetingProposal meetingProposal = (MeetingProposal)proposal.Value;
                if (meetingProposal.Coordinator.Equals(username) || meetingProposal.Invitees.Contains(username))
                {
                    userProposals.Add(meetingProposal);
                }   
            }

            return userProposals;
        }

        public void Create(MeetingProposal proposal)
        {
            _currentMeetingProposals.Add(proposal.Topic, proposal);
            Console.WriteLine("Created new meeting proposal for " + proposal.Topic + ".");
        }

        public void Join(string topic, MeetingRecord record)
        {
            MeetingProposal proposal = (MeetingProposal) _currentMeetingProposals[topic];
            proposal.Records.Add(record);
            Console.WriteLine(record.Name + " joined meeting proposal " + proposal.Topic + ".");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="args">
        ///     args[0]->serverId
        ///     args[1]->serverUrl
        ///     args[2]->maxFaults
        ///     args[3]->minDelay
        ///     args[4]->maxDelay
        /// </param>
        static void Main(string[] args) {

            new CServer(args[0], args[1], Int32.Parse(args[2]), Int32.Parse(args[3]), Int32.Parse(args[4]));
            
            System.Console.WriteLine("<enter> para sair...");
			System.Console.ReadLine();
		}
	}
}