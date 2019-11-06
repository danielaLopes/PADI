using System;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Collections.Generic;
using ClassLibrary;
using System.Collections;

namespace Server
{
    public delegate void InvitationDelegate(IClient user, MeetingProposal proposal, string userName);

    public class CServer : MarshalByRefObject, IServer
    {     
        private Dictionary<string,MeetingProposal> _currentMeetingProposals;

        private List<IServer> _servers;

        // TODO THIS IS JUST TEMPORARY UNTIL PEER TO PEER CLIENT COMMUNICATION
        private List<IClient> _broadcastClients;

        private Dictionary<string, Location> _locations = new Dictionary<string, Location>();

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

            _currentMeetingProposals = new Dictionary<string, MeetingProposal>();

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

        public void List(string name, Dictionary<string,MeetingProposal> knownProposals)
        {
            Dictionary<string,MeetingProposal> proposals = new Dictionary<string, MeetingProposal>();

            foreach (KeyValuePair<string, MeetingProposal> proposal in _currentMeetingProposals)
            {
                
                if (proposal.Value.Invitees.Contains(name) && knownProposals.ContainsKey(proposal.Value.Topic))
                {
                    proposals.Add(proposal.Value.Topic, proposal.Value);
                }
            }
            _clients[name].UpdateList(proposals);
        }

        public void Join(string topic, MeetingRecord record)
        {
            MeetingProposal proposal = _currentMeetingProposals[topic];

            foreach (DateLocation date1 in proposal.DateLocationSlots)
            {
                foreach (DateLocation date2 in record.DateLocationSlots)
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
            MeetingProposal proposal = _currentMeetingProposals[topic];

            DateLocation finalDateLocation = new DateLocation();
            foreach (DateLocation dateLocation in proposal.DateLocationSlots)
            {

                if (dateLocation.Invitees > finalDateLocation.Invitees)
                {
                    finalDateLocation = dateLocation;
                }
            }

            /*Location location = _locations[finalDateLocation.LocationName];
            //<Room> possibleRooms = new List<Room>();
            SortedDictionary<int, Room> possibleRooms = new SortedDictionary<int, Room>();

            foreach (Room room in location.Rooms)
            {

                if (room.RoomAvailability == Room.RoomStatus.NonBooked )//&& room.Capacity >= finalDateLocation.Invitees)
                {
                    possibleRooms.Add(room.Capacity, room);
                }
            }*/

            if (finalDateLocation.Invitees < proposal.MinAttendees)// || possibleRooms.Count == 0)
            {
                proposal.MeetingStatus = MeetingStatus.Cancelled;
            }
            else
            {
                proposal.MeetingStatus = MeetingStatus.Closed;

                proposal.FinalDateLocation = finalDateLocation;
                foreach (MeetingRecord record in proposal.Records)
                {
                    if (record.DateLocationSlots.Contains(finalDateLocation))
                    {
                        proposal.Participants.Add(record.Name);
                    }
                }
            }
            Console.WriteLine(proposal.Coordinator + " closed meeting proposal " + proposal.Topic + ".");
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
            foreach(string url in serverUrls)
            {
                _servers.Add((IServer)Activator.GetObject(typeof(IServer), url));
            }
        }

        public void GetMasterUpdateClients(List<string> clientUrls)
        {
            foreach (string url in clientUrls)
            {
                _broadcastClients.Add((IClient)Activator.GetObject(typeof(IClient), url));
            }
        }

        public void GetMasterUpdateLocations(Dictionary<string, Location> locations)
        {
            _locations = locations;
            Console.WriteLine("adding location");
        }

        public void Status()
        {
            Console.WriteLine("Server is active. URL: {0}", SERVER_URL);
        }

        public void ShutDown()
        {

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