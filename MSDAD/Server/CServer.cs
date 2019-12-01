using System;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Collections.Generic;
using ClassLibrary;
using System.Threading;
using System.Linq;
using System.Collections.Concurrent;

namespace Server
{
    public delegate void SendAllInvitationsDelegate(MeetingProposal proposal);
    public delegate void InvitationDelegate(IClient user, MeetingProposal proposal, string userName);

    public delegate string BroadcastNewMeetingDelegate(IServer server, string url, MeetingProposal proposal);
    public delegate string BroadcastJoinDelegate(IServer server, string url, string username, MeetingProposal proposal, MeetingRecord record);
    public delegate void BroadcastCloseDelegate(IServer server, MeetingProposal proposal);
    public delegate void BroadcastUpdateLocationDelegate(IServer server, Location location);

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

        /// <summary>
        /// If client wants to switch server, he can ask the server to provide him with a list of servers' urls
        /// </summary>
        private ConcurrentDictionary<string, IServer> _servers = new ConcurrentDictionary<string, IServer>();

        /// <summary>
        /// string->username IClient->client remote object
        /// </summary>
        private ConcurrentDictionary<string, IClient> _clients = new ConcurrentDictionary<string, IClient>();

        /// <summary>
        /// Max number of faults tolerated by the system ?????
        /// </summary>
        private int _maxFaults;

        /// <summary>
        /// Interval to attribute a random delay to each incoming message in millisseconds
        /// </summary>
        private int _minDelay;
        private int _maxDelay;

        // to send messages to clients asynchronously, otherwise the loop would deadlock
        private SendAllInvitationsDelegate _sendAllInvitationsDelegate;
        private InvitationDelegate _sendInvitationsDelegate;

        private BroadcastNewMeetingDelegate _broadcastNewMeetingDelegate;
        private BroadcastJoinDelegate _broadcastJoinDelegate;
        private BroadcastCloseDelegate _broadcastCloseDelegate;
        private BroadcastUpdateLocationDelegate _broadcastUpdateLocationDelegate;

        //int n_acks = 0;


        /// <summary>
        /// Decides a random number for the delay of an incoming message in millisseconds
        /// according to the server's delay interval
        /// </summary>
        /// <returns> The delay in millisseconds </returns>
        public int RandomIncomingMessageDelay()
        {
            Random random = new Random();
            return random.Next(_minDelay, _maxDelay);
        }

        public CServer(string serverId, string url, int maxFaults, int minDelay, int maxDelay, string roomsFile, List<string> serverUrls = null)
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
            }

            _maxFaults = maxDelay;

            _minDelay = minDelay;
            _maxDelay = maxDelay;

            _sendAllInvitationsDelegate = new SendAllInvitationsDelegate(SendAllInvitations);
            _sendInvitationsDelegate = new InvitationDelegate(SendInvitationToClient);

            _broadcastNewMeetingDelegate = new BroadcastNewMeetingDelegate(BroadcastNewMeetingToServer);
            _broadcastJoinDelegate = new BroadcastJoinDelegate(BroadcastJoinToServer);
            _broadcastCloseDelegate = new BroadcastCloseDelegate(BroadcastCloseToServer);
            _broadcastUpdateLocationDelegate = new BroadcastUpdateLocationDelegate(BroadcastUpdateLocationToServer);
        }

        // ------------------- COMMANDS SENT BY CLIENTS -------------------

        public void RegisterUser(string username, string clientUrl) 
        {
            Thread.Sleep(RandomIncomingMessageDelay());

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
            Thread.Sleep(RandomIncomingMessageDelay());

            if (_currentMeetingProposals.TryAdd(proposal.Topic, proposal))
            {
                Console.WriteLine("Created new meeting proposal for " + proposal.Topic + ".");
                //_sendAllInvitationsDelegate.BeginInvoke(proposal, SendAllInvitationsCallback, null);
                BroadcastNewMeeting(proposal);
            }
            else
            {
                Console.WriteLine("Not possible to create meeting with topic {0}", proposal.Topic);
            }
        }

        public void List(string name, Dictionary<string,MeetingProposal> knownProposals)
        {
            Thread.Sleep(RandomIncomingMessageDelay());

            Dictionary<string,MeetingProposal> proposals = new Dictionary<string, MeetingProposal>();

            foreach (KeyValuePair<string, MeetingProposal> proposal in _currentMeetingProposals)
            {
                
                if (knownProposals.ContainsKey(proposal.Value.Topic))
                {
                    proposals.Add(proposal.Value.Topic, proposal.Value);
                }
            }
            _clients[name].UpdateList(proposals);
        }

        public void Join(string username, string topic, MeetingRecord record)
        {
            Thread.Sleep(RandomIncomingMessageDelay());

            MeetingProposal proposal;
            if (!_currentMeetingProposals.TryGetValue(topic, out proposal))
            {
                // if the server does not have a meeting, he tells the client to switch to a different server
                AttributeNewServer(username);
            }
            else
            {
                // Checks if the join arrived after the meeting is closed, in that 
                // case it maintains a record with a special status, FAILED
                if (proposal.MeetingStatus.Equals(MeetingStatus.CLOSED) ||
                        proposal.MeetingStatus.Equals(MeetingStatus.CANCELLED))
                {
                    if (!proposal.FailedRecords.Contains(record))
                    {
                        proposal.AddFailedRecord(record);
                        BroadcastJoin(username, proposal, record);
                    }
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
                    BroadcastJoin(username, proposal, record);
                }
                Console.WriteLine(record.Name + " joined meeting proposal " + proposal.Topic + ".");
            }
        }

        public void Close(string topic)
        {
            Thread.Sleep(RandomIncomingMessageDelay());

            MeetingProposal proposal = _currentMeetingProposals[topic];

            DateLocation finalDateLocation = new DateLocation();
            foreach (DateLocation dateLocation in proposal.DateLocationSlots)
            {
                if (dateLocation.Invitees > finalDateLocation.Invitees)
                {
                    
                    finalDateLocation = dateLocation;
                }
            }
            Console.WriteLine(finalDateLocation.LocationName);
            Location location = _locations[finalDateLocation.LocationName];
            SortedDictionary<int, Room> possibleRooms = new SortedDictionary<int, Room>();
            int maxCapacity = 0;
            foreach (KeyValuePair<string,Room> room in location.Rooms)
            {
                Console.WriteLine(room.Value.RoomAvailability);
                if (room.Value.RoomAvailability == Room.RoomStatus.NONBOOKED)
                {
                    possibleRooms.Add(room.Value.Capacity, room.Value);

                    if (maxCapacity < room.Value.Capacity) maxCapacity = room.Value.Capacity;
                }
            }
            if (maxCapacity < finalDateLocation.Invitees && possibleRooms.Count != 0)
            {
                proposal.FinalRoom = possibleRooms[maxCapacity];
            }

            else
            {
                foreach (KeyValuePair<int, Room> room in possibleRooms)
                {
                    if (room.Key >= finalDateLocation.Invitees)
                    {
                        proposal.FinalRoom = room.Value;
                        break;
                    }
                }
            }

            if (finalDateLocation.Invitees < proposal.MinAttendees || possibleRooms.Count == 0)
            {
                proposal.MeetingStatus = MeetingStatus.CANCELLED;
            }
            else
            {
                int countInvitees = 0;
                proposal.MeetingStatus = MeetingStatus.CLOSED;
                _locations[finalDateLocation.LocationName].Rooms[proposal.FinalRoom.Name].RoomAvailability = Room.RoomStatus.BOOKED;

                BroadcastUpdateLocation(location);

                proposal.FinalDateLocation = finalDateLocation;
                foreach (KeyValuePair<string, MeetingRecord> record in proposal.Records)
                {

                    if (record.Value.DateLocationSlots.Contains(finalDateLocation))
                    {
                        countInvitees++;

                        Console.WriteLine("record " + record.Value);

                        if (countInvitees > maxCapacity)
                        {
                            proposal.AddFullRecord(record.Value);
                        }
                        else
                        {
                            proposal.Participants.Add(record.Value.Name);
                        }
                    }
                }
            }
            Console.WriteLine(proposal.Coordinator + " closed meeting proposal " + proposal.Topic + ".");

            BroadcastClose(proposal);
        }


        // ------------------- COMMUNICATION WITH OTHER SERVERS -------------------
        // servers should send updates to other servers so that they maintain the state distributed

        // TODO all invitations now should be sent by client
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

                        Console.WriteLine("Sent invitation to user {0}", username);
                        // CHECK IF BROADCAST CLIENTS CONTAIS ALL INVITEES
                        // IF NOT, CHANGE USER SERVER
                    }
                }
            }
        }

        // TODO invitations now must be sent by a client to other clients
        public void SendInvitationToClient(IClient user, MeetingProposal proposal, string username)
        {
            Console.WriteLine("going to send invitation to {0}", username);
            user.ReceiveInvitation(proposal);
        }

        // TODO
        public void SendAllInvitationsCallback(IAsyncResult res)
        {
            // _sendAllInvitationsDelegate.EndInvoke(res);
            Console.WriteLine("finished sending all invitations");
        }

        // TODO
        public void SendInvitationCallback(IAsyncResult res)
        {
            //_sendInvitationsDelegate.EndInvoke(res);
            Console.WriteLine("finished sending invitation");
        }

        public void BroadcastNewMeeting(MeetingProposal proposal)
        {
            int n_acks = 0;
            List<IAsyncResult> res = new List<IAsyncResult>();
            List<bool> res_bool = new List<bool>();
            foreach (KeyValuePair<string, IServer> server in _servers)
            {
                res.Add(_broadcastNewMeetingDelegate.BeginInvoke(server.Value, server.Key, proposal, BroadcastNewMeetingCallback, null));
                res_bool.Add(false);
           
                
            }

            /*while (n_acks < 1)
            {
                Console.WriteLine(".");
                for (int i = 0; i < res.Count(); i++)// result in res)
                    if (res[i].IsCompleted && !res_bool[i]) 
                    {
                        n_acks++;
                        res_bool[i] = true;
                    }
            }*/

            
            //Console.WriteLine("acks "+n_acks);
        }

        public string BroadcastNewMeetingToServer(IServer server, string url, MeetingProposal proposal)
        {
            Console.WriteLine("going to send new meeting {0}", proposal.Topic);
            server.ReceiveNewMeeting(proposal);
            return url;
        }

        public void BroadcastNewMeetingCallback(IAsyncResult res)
        {
            /*int acks = 0;*/
            try
            {
                string returnValue = _broadcastNewMeetingDelegate.EndInvoke(res);
                Console.WriteLine("finished sending new meeting to " + returnValue);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public void ReceiveNewMeeting(MeetingProposal meeting)
        {
            Thread.Sleep(RandomIncomingMessageDelay());
            //Thread.Sleep(10000);
            if (_currentMeetingProposals.TryAdd(meeting.Topic, meeting))
            {
                Console.WriteLine("received new meeting {0}", meeting.Topic);
                BroadcastNewMeeting(meeting);
            }
            /*else
            {
                Console.WriteLine("not possible to receive new meeting {0}", meeting.Topic);
            }*/
        }

        public void BroadcastJoin(string username, MeetingProposal proposal, MeetingRecord record)
        {
            List<IAsyncResult> res = new List<IAsyncResult>();
            List<bool> res_bool = new List<bool>();

            foreach (KeyValuePair<string, IServer> server in _servers)
            {
                res.Add(_broadcastJoinDelegate.BeginInvoke(server.Value, server.Key, username, proposal, record, BroadcastJoinCallback, null));
                res_bool.Add(false);
            }
        }

        public string BroadcastJoinToServer(IServer server, string url, string username, MeetingProposal proposal, MeetingRecord record)
        {
            Console.WriteLine("going to send join {0}", proposal.Topic);
            server.ReceiveJoin(username, proposal, record);
            return url;
        }

        public void BroadcastJoinCallback(IAsyncResult res)
        {

            try
            {
                string returnValue = _broadcastJoinDelegate.EndInvoke(res);
                Console.WriteLine("finished sending join to " + returnValue);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public void ReceiveJoin(string username, MeetingProposal proposal, MeetingRecord record)
        {
            Thread.Sleep(RandomIncomingMessageDelay());

            Console.WriteLine("received join {0}", proposal.Topic);

            MeetingProposal previousProposal;
            if (_currentMeetingProposals.TryGetValue(proposal.Topic, out previousProposal))
            {
                if (previousProposal.MeetingStatus == MeetingStatus.CLOSED)
                {
                    Join(username, proposal.Topic, record);
                } 
            }
            else
            {
                _currentMeetingProposals[proposal.Topic] = proposal;
            }
        }

        public void BroadcastClose(MeetingProposal proposal)
        {
            foreach (KeyValuePair<string, IServer> server in _servers)
            {
                _broadcastCloseDelegate.BeginInvoke(server.Value, proposal, BroadcastCloseCallback, null);
            }
        }

        public void BroadcastCloseToServer(IServer server, MeetingProposal proposal)
        {
            Console.WriteLine("going to send close {0}", proposal.Topic);
            server.ReceiveClose(proposal);
        }

        public void BroadcastCloseCallback(IAsyncResult res)
        {
            _broadcastCloseDelegate.EndInvoke(res);
            Console.WriteLine("finished sending close");
        }

        public void ReceiveClose(MeetingProposal proposal)
        {
            Thread.Sleep(RandomIncomingMessageDelay());

            Console.WriteLine("received close {0}", proposal.Topic);
            _currentMeetingProposals[proposal.Topic] = proposal;
        }

        public void BroadcastUpdateLocation(Location location)
        {
            foreach (KeyValuePair<string, IServer> server in _servers)
            {
                _broadcastUpdateLocationDelegate.BeginInvoke(server.Value, location, BroadcastUpdateLocationCallback, null);
            }
        }

        public void BroadcastUpdateLocationToServer(IServer server, Location location)
        {
            Console.WriteLine("going to send updated location {0}", location.Name);
            server.ReceiveUpdateLocation(location);
        }

        public void BroadcastUpdateLocationCallback(IAsyncResult res)
        {
            _broadcastUpdateLocationDelegate.EndInvoke(res);
            Console.WriteLine("finished sending update");
        }

        public void ReceiveUpdateLocation(Location location)
        {
            Thread.Sleep(RandomIncomingMessageDelay());

            Console.WriteLine("received updated location {0}", location.Name);
            _locations[location.Name] = location;
        }


        // ------------------- PUPPET MASTER COMMANDS -------------------

        public void Status()
        {
            Console.WriteLine("Server is active. URL: {0}", SERVER_URL);
        }

        public void ShutDown()
        {

        }

        public void Crash()
        {

        }

        public void Freeze()
        {

        }

        public void Unfreeze()
        {

        }


        // ------------------- METHODS FOR SERVER TO INITIATE CORRECTLY -------------------

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

                if (!_locations.ContainsKey(locationName))
                {
                    if (_locations.TryAdd(locationName, new Location(locationName)))
                    {
                        Console.WriteLine("added new location {0}", locationName);
                    }
                    else
                    {
                        Console.WriteLine("Not possible to add location {0}", locationName);
                    }
                }

                _locations[locationName].AddRoom(new Room(roomName, capacity, Room.RoomStatus.NONBOOKED));

            }
        }

        public void UpdateServers(List<string> serverUrls)
        {
            foreach (string url in serverUrls)
            {
                UpdateServer(url);
            }
        }

        public void UpdateServer(string serverUrl)
        {
            Console.WriteLine("Updating server {0}", serverUrl);

            IServer server = RegisterServer(serverUrl);

            server.RegisterServer(SERVER_URL);
        }

        public IServer RegisterServer(string serverUrl)
        {
            Console.WriteLine("Registering server {0}", serverUrl);

            IServer server = (IServer)Activator.GetObject(typeof(IServer), serverUrl);
            if(!_servers.ContainsKey(serverUrl)) _servers.TryAdd(serverUrl, server);

            return server;
        }


        // ------------------- METHODS TO SUPPORT SERVER FAILURES -------------------

        public void AttributeNewServer(string username)
        {
            // TODO change method of server selection, for example server with least clients
            Random randomizer = new Random();
            int random = randomizer.Next(_servers.Count);

            List<string> urls = _servers.Keys.ToList();
            _clients[username].SwitchServer(urls[random]);
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
        ///     args[7]->serversUrls
        /// </param>
        static void Main(string[] args) {

            CServer server;

            if (args.Length > 6)
            {
                Console.WriteLine("nServers " + args[6]);
                int nServers = Int32.Parse(args[6]);

                List<string> serversUrl = new List<string>();
                int i = 7;
                for (; i < 7 + nServers; i++)
                {
                    Console.WriteLine("new server url added " + args[i]);
                    serversUrl.Add(args[i]);
                }
                server = new CServer(args[0], args[1], Int32.Parse(args[2]), Int32.Parse(args[3]), Int32.Parse(args[4]), args[5], serversUrl);    
            }
            // with PuppetMaster
            else
            {
                server = new CServer(args[0], args[1], Int32.Parse(args[2]), Int32.Parse(args[3]), Int32.Parse(args[4]), args[5]);
            }
            
            System.Console.WriteLine("<enter> para sair...");
			System.Console.ReadLine();
		}
	}
}