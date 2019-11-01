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
        private Hashtable _users;

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

            _users = new Hashtable();
            _currentMeetingProposals = new Hashtable();

            Console.WriteLine("Server created at url: {0}", SERVER_URL);
            //_updateMessagesDelegate = new UpdateMessagesDelegate(UpdateMessages);
            //_updateMessagesCallback = new AsyncCallback(UpdateMessagesCallback);
        }

        public void RegisterUser(string username, string clientUrl) 
        {
            // obtain client remote object
            IClient remoteClient = (IClient)Activator.GetObject(typeof(IClient), clientUrl);
            _users.Add(username, remoteClient);

            Console.WriteLine("New user " + username  + " with url " + clientUrl + " registered.");
        }

        public void Create(MeetingProposal proposal)
        {
            Console.WriteLine("create");
            _currentMeetingProposals.Add(proposal.Topic, proposal);
            Console.WriteLine("Created new meeting proposal for " + proposal.Topic + ".");
            SendInvitations(proposal);
        }

        public void Join(string topic, MeetingRecord record)
        {
            MeetingProposal proposal = (MeetingProposal) _currentMeetingProposals[topic];
            proposal.Records.Add(record);
            Console.WriteLine(record.Name + " joined meeting proposal " + proposal.Topic + ".");
        }
                    
        public void SendInvitations(MeetingProposal proposal)
        {
            Console.WriteLine(proposal.Invitees);
            Console.WriteLine(proposal.Invitees.Count);
            if (proposal.Invitees == null)
            {
                foreach (IClient user in _users.Values)
                {
                    user.ReceiveInvitation(proposal);
                }
            }
            else
            {
                foreach (string user in proposal.Invitees)
                {
                    Console.WriteLine("list of invitees: {0}", user);

                    foreach (string key in _users.Keys)
                        Console.WriteLine("key: {0}", key);

                    IClient invitee = (IClient)_users[user];
                    Console.WriteLine("remote object: {0}", invitee.ToString());
                    invitee.ReceiveInvitation(proposal);
                }
            }
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