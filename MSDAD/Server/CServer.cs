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

            Console.WriteLine("Server created at url: {0}", SERVER_URL);
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

        public void Create(MeetingProposal proposal)
        {
            _currentMeetingProposals.Add(proposal.Topic, proposal);
            Console.WriteLine("Created new meeting proposal for " + proposal.Topic + ".");
        }

        public void List(string name, Hashtable knownProposals)
        {
            Hashtable proposals = new Hashtable();

            foreach(DictionaryEntry meetingProposal in _currentMeetingProposals)
            {
                MeetingProposal proposal = (MeetingProposal)meetingProposal.Value;
                if (proposal.Invitees.Contains(name) && knownProposals.Contains(proposal.Topic))
                {
                    proposals.Add(proposal.Topic, proposal);
                }
            }
            _users.Find(u => u.Nickname == name).RemoteClient.UpdateList(proposals);
        }

        public void Join(string topic, MeetingRecord record)
        {
            MeetingProposal proposal = (MeetingProposal)_currentMeetingProposals[topic];

            foreach (DateLocation date1 in proposal.DateLocationSlots)
            {
                foreach(DateLocation date2 in record.DateLocationSlots)
                {
                    if (date1.Equals(date2))
                    {
                        date1.Invitees++;
                    }
                }
            }

            proposal.Records.Add(record);
            Console.WriteLine(record.Name + " joined meeting proposal " + proposal.Topic + ".");
        }

        public void Close(string topic)
        {
            MeetingProposal proposal = (MeetingProposal)_currentMeetingProposals[topic];

            DateLocation finalDateLocation = new DateLocation();
            foreach(DateLocation dateLocation in proposal.DateLocationSlots)
            {
                
                if (dateLocation.Invitees > finalDateLocation.Invitees)
                {
                    finalDateLocation = dateLocation;
                }
            }

            if (finalDateLocation.Invitees < proposal.MinAttendees)
            {
                Console.WriteLine("finalDateLocation.Invitees" + finalDateLocation.Invitees);
                Console.WriteLine("proposal.MinAttendees" + proposal.MinAttendees);
                proposal.Status = Status.Cancelled;
            }
            else
            {
                proposal.Status = Status.Closed;

                proposal.FinalDateLocation = finalDateLocation;
                foreach(MeetingRecord record in proposal.Records)
                {
                    if (record.DateLocationSlots.Contains(finalDateLocation))
                    {
                        Console.WriteLine("adding participant" + record.Name);
                        proposal.Participants.Add(record.Name);
                    }
                }
            }
            Console.WriteLine(proposal.Coordinator + " closed meeting proposal " + proposal.Topic + ".");
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