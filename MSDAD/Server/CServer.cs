using System;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Collections.Generic;
using ClassLibrary;
using System.Collections;
using System.Threading;

namespace Server
{
    public delegate void InvitationDelegate(IClient user, MeetingProposal proposal, string userName);
    public delegate void InvitationCallbackDelegate();

    public class CServer : MarshalByRefObject, IServer
    {     
        private Hashtable _currentMeetingProposals;

        private List<IServer> _servers;

        // TODO THIS IS JUST TEMPORARY UNTIL PEER TO PEER CLIENT COMMUNICATION
        private List<IClient> _broadcastClients;

        private Dictionary<string, Location> _locations;

        private Dictionary<string, IClient> _clients;

        // to send messages to clients asynchronously, otherwise the loop would deadlock
        private InvitationDelegate _sendInvitationsDelegate;
        private AsyncCallback _sendInvitationsCallbackDelegate;


        private readonly string SERVER_ID;
        private readonly string SERVER_URL;

        public CServer(string serverId, string url, int maxFaults, int minDelay, int maxDelay, List<string> serverUrls = null, List<string> clientUrls = null)
        {
            SERVER_ID = serverId;
            SERVER_URL = url;
   
            TcpChannel serverChannel = new TcpChannel(PortExtractor.Extract(SERVER_URL));
            ChannelServices.RegisterChannel(serverChannel, false);

            // creates the server's remote object
            RemotingServices.Marshal(this, SERVER_ID, typeof(CServer));

            _currentMeetingProposals = new Hashtable();

            _clients = new Dictionary<string, IClient>();

            _servers = new List<IServer>();
            // gets other server's remote objects and saves them
            if (serverUrls != null)
            {
                GetMasterUpdateServers(serverUrls);
            }
            // else : the puppet master invokes GetMasterUpdateServers method

            _broadcastClients = new List<IClient>();
            // gets clients's remote objects and saves them
            if (clientUrls != null)
            {
                GetMasterUpdateClients(clientUrls);
            }
            // else : the puppet master invokes GetMasterUpdateClients method

            Console.WriteLine("Server created at url: {0}", SERVER_URL);

            _sendInvitationsDelegate = new InvitationDelegate(SendInvitationToClient);
            _sendInvitationsCallbackDelegate = new AsyncCallback(SendInvitationCallback);
        }

        public void RegisterUser(string username, string clientUrl) 
        {
            // obtain client remote object
            _clients.Add(username, (IClient)Activator.GetObject(typeof(IClient), clientUrl));

            Console.WriteLine("New user {0} with url {0} registered.", username, clientUrl);
        }

        public void Create(MeetingProposal proposal)
        {
            _currentMeetingProposals.Add(proposal.Topic, proposal);

            Console.WriteLine("Created new meeting proposal for " + proposal.Topic + ".");
            SendAllInvitations(proposal);
        }

        public void Join(string topic, MeetingRecord record)
        {
            MeetingProposal proposal = (MeetingProposal) _currentMeetingProposals[topic];
            proposal.Records.Add(record);
            Console.WriteLine(record.Name + " joined meeting proposal " + proposal.Topic + ".");
        }
                    
        public void SendAllInvitations(MeetingProposal proposal)
        {

            if (proposal.Invitees == null)
            {
                foreach (KeyValuePair<string, IClient> client in _clients)
                {
                    _sendInvitationsDelegate.BeginInvoke(client.Value, proposal, client.Key, SendInvitationCallback, null); 
                }

            }
            else
            {
                foreach (string username in proposal.Invitees)
                {
                    if (username != proposal.Coordinator)
                    {
                        IClient invitee = _clients[username];
                        _sendInvitationsDelegate.BeginInvoke(invitee, proposal, username, SendInvitationCallback, null);
                    }
                }


            }
        }

        public void SendInvitationToClient(IClient user, MeetingProposal proposal, string username)
        {
            Console.WriteLine("going to send invitation to {0}", username);
            user.ReceiveInvitation(proposal);
        }

        public void SendInvitationCallback(IAsyncResult res)
        {
            _sendInvitationsDelegate.EndInvoke(res);
            Console.WriteLine("finished sending invitation");
        }

        public void GetMasterUpdateServers(List<string> serverUrls)
        {
            Console.WriteLine(" GetMasterUpdateServers");
            foreach (string url in serverUrls)
            {
                _servers.Add((IServer)Activator.GetObject(typeof(IServer), url));
            }
        }

        public void GetMasterUpdateClients(List<string> clientUrls)
        {
            Console.WriteLine(" GetMasterUpdateClients");
            foreach (string url in clientUrls)
            {
                _broadcastClients.Add((IClient)Activator.GetObject(typeof(IClient), url));
            }
        }

        public void GetMasterUpdateLocations(Dictionary<string, Location> locations)
        {
            Console.WriteLine(" GetMasterUpdateLocations");
            Console.WriteLine("Got locations from puppetmaster {0} {0}", _locations["Porto"].Rooms.ToString(), _locations["Lisboa"].Rooms.ToString());
            _locations = locations;
            Console.WriteLine(" GetMasterUpdateLocations after");
        }

        public void Status()
        {

        }

        public void ShutDown()
        {

        }

        public void GetRooms()
        {
            Console.WriteLine("GetRooms()");
            Console.WriteLine("How many rooms: {0}", _locations.Count);
            Console.WriteLine("How many rooms in Lisboa: {0}", _locations["Lisboa"].Rooms.Count);
            Console.WriteLine(_locations["Lisboa"].Rooms.ToString());
            Console.WriteLine(_locations["Porto"].Rooms.ToString());
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

            CServer server = new CServer(args[0], args[1], Int32.Parse(args[2]), Int32.Parse(args[3]), Int32.Parse(args[4]));


            //Thread.Sleep(1000);

            //server.GetRooms();

            
            System.Console.WriteLine("<enter> para sair...");
			System.Console.ReadLine();
		}
	}
}