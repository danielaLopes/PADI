using System;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Collections.Generic;
using ClassLibrary;
using System.Collections;
using System.Threading;
using System.Linq;
using System.Collections.Concurrent;

namespace Server
{
    public delegate void SendAllInvitationsDelegate(MeetingProposal proposal);
    public delegate void InvitationDelegate(IClient user, MeetingProposal proposal, string userName);

    public delegate void BroadcastNewMeetingDelegate(IServer user, MeetingProposal proposal);
    public delegate void BroadcastJoinDelegate(IServer user, string topic, MeetingRecord record);
    public delegate void BroadcastCloseDelegate(IServer user, string topic, MeetingRecord record);

    public class CServer : MarshalByRefObject, IServer
    {
        private readonly string SERVER_ID;
        private readonly string SERVER_URL;

        private ConcurrentDictionary<string, MeetingProposal> _currentMeetingProposals = new ConcurrentDictionary<string, MeetingProposal>();

        // TODO THIS IS JUST TEMPORARY UNTIL PEER TO PEER CLIENT COMMUNICATION
        private ConcurrentDictionary<string, IClient> _broadcastClients = new ConcurrentDictionary<string, IClient>();

        /// <summary>
        /// string->locationaName Locations->contains Rooms
        /// </summary> 
        private ConcurrentDictionary<string, Location> _locations = new ConcurrentDictionary<string, Location>();

        private List<IServer> _servers = new List<IServer>();

        /// <summary>
        /// If client wants to switch server, he can ask the server to provide him with a list of servers' urls
        /// </summary>
        private List<string> _serverUrls = new List<string>();

        /// <summary>
        /// string->username IClient->client remote object
        /// </summary>
        private ConcurrentDictionary<string, IClient> _clients = new ConcurrentDictionary<string, IClient>();

        // to send messages to clients asynchronously, otherwise the loop would deadlock
        private SendAllInvitationsDelegate _sendAllInvitationsDelegate;
        private InvitationDelegate _sendInvitationsDelegate;
        private BroadcastNewMeetingDelegate _broadcastNewMeetingDelegate;

        private BroadcastJoinDelegate _broadcastJoinDelegate;

        private BroadcastCloseDelegate _broadcastCloseDelegate;

        public CServer(string serverId, string url, int maxFaults, int minDelay, int maxDelay, string roomsFile, List<string> serverUrls = null, List<string> clientUrls = null)
        //public CServer(string serverId, string url, int maxFaults, int minDelay, int maxDelay, List<string> locations = null, List<string> serverUrls = null, List<string> clientUrls = null)
        {
            SERVER_ID = serverId;
            SERVER_URL = url;
   
            TcpChannel serverChannel = new TcpChannel(PortExtractor.Extract(SERVER_URL));
            ChannelServices.RegisterChannel(serverChannel, false);

            // creates the server's remote object
            RemotingServices.Marshal(this, SERVER_ID, typeof(CServer));
            Console.WriteLine("Server created at url: {0}", SERVER_URL);

            RegisterRooms(roomsFile);
            
            if (serverUrls != null)
            {
                // gets other server's remote objects and saves them
                UpdateServers(serverUrls);
            } //else : the puppet master invokes the correspondent update methods
            if (clientUrls != null)
            {  
                // gets clients's remote objects and saves them
                UpdateClients(clientUrls);
            } //else : the puppet master invokes the correspondent update methods

            _sendAllInvitationsDelegate = new SendAllInvitationsDelegate(SendAllInvitations);

            _sendInvitationsDelegate = new InvitationDelegate(SendInvitationToClient);

            _broadcastNewMeetingDelegate = new BroadcastNewMeetingDelegate(BroadcastNewMeetingToServer);

            _broadcastJoinDelegate = new BroadcastJoinDelegate(BroadcastJoinToServer);

            _broadcastCloseDelegate = new BroadcastCloseDelegate(BroadcastCloseToServer);
        }

        public void RegisterUser(string username, string clientUrl) 
        {
            // obtain client remote object
            if (_clients.TryAdd(username, (IClient)Activator.GetObject(typeof(IClient), clientUrl)))
            {
                Console.WriteLine("New user {0} with url {0} registered.", username, clientUrl);
            }
            else
            {
                Console.WriteLine("not possible to register user {0} with URL: {1}. Try again", username, clientUrl);
            }
        }

        public void Create(MeetingProposal proposal)
        {
            if (_currentMeetingProposals.TryAdd(proposal.Topic, proposal))
            {
                Console.WriteLine("Created new meeting proposal for " + proposal.Topic + ".");
                _sendAllInvitationsDelegate.BeginInvoke(proposal, SendAllInvitationsCallback, null);
                BroadcastNewMeeting(proposal);
            }
            else
            {
                Console.WriteLine("Not possible to create meeting with topic {0}", proposal.Topic);
            }
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

            // Checks if the join arrived after the meeting is closed, in that 
            // case it maintains a record with a special status, FAILED
            if (proposal.MeetingStatus.Equals(MeetingStatus.CLOSED) || 
                    proposal.MeetingStatus.Equals(MeetingStatus.CANCELLED))
            {
                record.RecordStatus = RecordStatus.FAILED;
                proposal.AddFailedRecord(record);
            }
            else
            {
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
                proposal.AddMeetingRecord(record);
            }
            Console.WriteLine(record.Name + " joined meeting proposal " + proposal.Topic + ".");
            BroadcastJoin(topic, record);
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
                proposal.MeetingStatus = MeetingStatus.CANCELLED;
            }
            else
            {
                proposal.MeetingStatus = MeetingStatus.CLOSED;

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
                foreach (KeyValuePair<string, IClient> client in _broadcastClients)
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
                        while (!_broadcastClients.ContainsKey(username))
                        {
                            Console.WriteLine("user {0} not present. going to wait", username);
                            Thread.Sleep(500);
                        }

                        IClient invitee = _broadcastClients[username];
                        _sendInvitationsDelegate.BeginInvoke(invitee, proposal, username, SendInvitationCallback, null);

                        // CHECK IF BROADCAST CLIENTS CONTAIS ALL INVITEES
                        // IF NOT, CHANGE USER SERVER
                    }
                }
            }
        }

        public void SendInvitationToClient(IClient user, MeetingProposal proposal, string username)
        {
            Console.WriteLine("going to send invitation to {0}", username);
            user.ReceiveInvitation(proposal);
        }

        public void SendAllInvitationsCallback(IAsyncResult res)
        {
            _sendAllInvitationsDelegate.EndInvoke(res);
            Console.WriteLine("finished sending all invitations");
        }

        public void SendInvitationCallback(IAsyncResult res)
        {
            _sendInvitationsDelegate.EndInvoke(res);
            Console.WriteLine("finished sending invitation");
        }

        public void BroadcastNewMeeting(MeetingProposal proposal)
        {
            foreach (IServer server in _servers)
            {
                _broadcastNewMeetingDelegate.BeginInvoke(server, proposal, BroadcastNewMeetingCallback, null);
            }
        }

        public void BroadcastNewMeetingToServer(IServer server, MeetingProposal proposal)
        {
            Console.WriteLine("going to send new meeting {0}", proposal.Topic);
            server.ReceiveNewMeeting(proposal);
        }

        public void BroadcastNewMeetingCallback(IAsyncResult res)
        {
            _broadcastNewMeetingDelegate.EndInvoke(res);
            Console.WriteLine("finished sending new meeting");
        }

        public void ReceiveNewMeeting(MeetingProposal meeting)
        {
            if (_currentMeetingProposals.TryAdd(meeting.Topic, meeting))
            {
                Console.WriteLine("received new meeting {0}", meeting.Topic);
            }
            else
            {
                Console.WriteLine("not possible to receive new meeting {0}", meeting.Topic);
            }
        }

        public void BroadcastJoin(string topic, MeetingRecord record)
        {
            foreach (IServer server in _servers)
            {
                //_broadcastJoinDelegate.BeginInvoke(server, topic, record, BroadcastJoinCallback, null);
                _broadcastJoinDelegate.BeginInvoke(server, topic, record, null, null);
            }
        }

        public void BroadcastJoinToServer(IServer server, string topic, MeetingRecord record)
        {
            Console.WriteLine("going to send join {0}", topic);
            server.ReceiveJoin(topic, record);
        }

        /*public void BroadcastJoinCallback(IAsyncResult res)
        {
            _broadcastJoinDelegate.EndInvoke(res);
            Console.WriteLine("finished sending join");
        }*/

        public void ReceiveJoin(string topic, MeetingRecord record)
        {
            Console.WriteLine("received join {0}", topic);
            _currentMeetingProposals[topic].AddMeetingRecord(record);
        }

        public void BroadcastClose(string topic, MeetingRecord record)
        {
            foreach (IServer server in _servers)
            {
                //_broadcastJoinDelegate.BeginInvoke(server, topic, record, BroadcastJoinCallback, null);
                _broadcastCloseDelegate.BeginInvoke(server, topic, record, null, null);
            }
        }

        public void BroadcastCloseToServer(IServer server, string topic, MeetingRecord record)
        {
            Console.WriteLine("going to send close {0}", topic);
            server.ReceiveJoin(topic, record);
        }

        /*public void BroadcastCloseCallback(IAsyncResult res)
        {
            _broadcastJoinDelegate.EndInvoke(res);
            Console.WriteLine("finished sending close");
        }*/

        public void ReceiveClose(string topic, MeetingRecord record)
        {
            Console.WriteLine("received close {0}", topic);
            _currentMeetingProposals[topic].AddMeetingRecord(record);
        }

        public void RegisterRooms(string fileName)
        {
            string[] lines = System.IO.File.ReadAllLines(@fileName);

            // each line has the location and room arguments: locationName roomName capacity
            foreach (string line in lines)
            {
                List<string> args = line.Split().ToList();
                string locationName = args[0];
                int capacity = Int32.Parse(args[1]);
                string roomName = args[2];               

                if (!_locations.ContainsKey(locationName)) {
                    if (_locations.TryAdd(locationName, new Location(locationName)))
                    {
                        Console.WriteLine("added new location {0}", locationName);
                    }
                    else
                    {
                        Console.WriteLine("Not possible to add location {0}", locationName);
                    }
                }

                _locations[locationName].AddRoom(new Room(roomName, capacity, Room.RoomStatus.NonBooked));

            }
        }

        public void UpdateServers(List<string> serverUrls)
        {
            foreach (string url in _serverUrls)
            {
                UpdateServer(url);
            }
        }

        public void UpdateServerAndSpread(string serverUrl)
        {
            Console.WriteLine("Updating server and spread {0}", serverUrl);

            IServer server = UpdateServer(serverUrl);
            server.UpdateServer(SERVER_URL);
        }

        public IServer UpdateServer(string serverUrl)
        {
            Console.WriteLine("Updating server {0}", serverUrl);

            _serverUrls.Add(serverUrl);
            IServer server = (IServer)Activator.GetObject(typeof(IServer), serverUrl);
            _servers.Add(server);

            return server;
        }

        public void UpdateClients(List<string> clientUrls)
        {
            foreach (string url in clientUrls)
            {
                UpdateClient(url);
            }
        }

        public void UpdateClient(string clientUrl)
        {
            string clientName = clientUrl.Split('/').ToList()[3];
            if (_broadcastClients.TryAdd(clientName, (IClient)Activator.GetObject(typeof(IClient), clientUrl)))
            {
                Console.WriteLine("added client {0} to broadcastClients", clientName);
            }
            else
            {
                Console.WriteLine("Not possible to add client {0} to broadcasteClient", clientName);
            }
        }

        public void AttributeNewServer(string username)
        {
            // TODO change method of server selection, for example server with least clients
            Random randomizer = new Random();
            int random = randomizer.Next(_serverUrls.Count);

            _clients[username].SwitchServer(_serverUrls[random]);
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
        ///     args[5]->roomsFile
        ///     (optional)
        ///     args[6]->numServers
        ///     args[7]->numClients
        ///     args[8]->serversUrls
        ///     args[9]->clientUrls
        /// </param>
        static void Main(string[] args) {

            CServer server;

            // without PuppetMaster
            if (args.Length > 6)
            {
                int nServers = Int32.Parse(args[6]);
                int nClients = Int32.Parse(args[7]);

                List<string> serversUrl = new List<string>();
                int i = 6;
                for (; i < 6 + nServers; i++)
                {
                    serversUrl.Add(args[i]);
                }

                if (nClients > 0)
                {
                    List<string> clientsUrl = new List<string>();
                    for (; i < nClients; i++)
                    {
                        clientsUrl.Add(args[i]);
                    }
                    server = new CServer(args[0], args[1], Int32.Parse(args[2]), Int32.Parse(args[3]), Int32.Parse(args[4]), args[5], serversUrl, clientsUrl);
                }
                else
                {
                    server = new CServer(args[0], args[1], Int32.Parse(args[2]), Int32.Parse(args[3]), Int32.Parse(args[4]), args[5], serversUrl, null);
                }
                
            }
            // with PuppetMaster
            else
            {
                server = new CServer(args[0], args[1], Int32.Parse(args[2]), Int32.Parse(args[3]), Int32.Parse(args[4]), args[5]);
            }

            //Thread.Sleep(1000);

            //server.GetRooms();

            
            System.Console.WriteLine("<enter> para sair...");
			System.Console.ReadLine();
		}
	}
}