
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
    public delegate string BroadcastCloseDelegate(IServer server, string url, MeetingProposal proposal);
    public delegate void BroadcastUpdateLocationDelegate(IServer server, Location location);

    public class CServer : MarshalByRefObject, IServer
    {
        private readonly string SERVER_ID;
        private readonly string SERVER_URL;

        private ConcurrentDictionary<string, MeetingProposal> _currentMeetingProposals = new ConcurrentDictionary<string, MeetingProposal>();

        /// <summary>
        /// string->locationaName Locations->contains Rooms
        /// </summary> 
        private ConcurrentDictionary<string, Location> _locations = new ConcurrentDictionary<string, Location>();

        /// <summary>
        /// If client wants to switch server, he can ask the server to provide him with a list of servers' urls
        /// </summary>
        private ConcurrentDictionary<string, IServer> _servers = new ConcurrentDictionary<string, IServer>();

        /// <summary>
        /// Keeps the status of the servers
        /// key: String corresponding to server url
        /// Value: if dead true
        /// </summary>
        private ConcurrentDictionary<string, bool> _serversStatus = new ConcurrentDictionary<string, bool>();

        /// <summary>
        /// string->username IClient->client remote object
        /// </summary>
        private ConcurrentDictionary<string, IClient> _clients = new ConcurrentDictionary<string, IClient>();

        private List<string> _clientUrls = new List<string>();

        /// <summary>
        /// Simulates a collection of vector clocks
        /// key: String corresponding to meeting topic
        /// Value: Vector clock
        /// </summary>
        private ConcurrentDictionary<String, VectorClock> _meetingsClocks = new ConcurrentDictionary<String, VectorClock>();

        /// <summary>
        /// Simulates the operations log
        /// key: String corresponding to meeting topic
        /// Value: List of Operations
        /// </summary>
        private ConcurrentDictionary<String, List<Operation>> _operationsLog = new ConcurrentDictionary<string, List<Operation>>();

        /// <summary>
        /// Max number of faults tolerated by the system ?????
        /// </summary>
        private int _maxFaults;

        /// <summary>
        /// Interval to attribute a random delay to each incoming message in millisseconds
        /// </summary>
        private int _minDelay;
        private int _maxDelay;

        private BroadcastNewMeetingDelegate _broadcastNewMeetingDelegate;
        private BroadcastJoinDelegate _broadcastJoinDelegate;
        private BroadcastCloseDelegate _broadcastCloseDelegate;
        private BroadcastUpdateLocationDelegate _broadcastUpdateLocationDelegate;

        //int n_acks = 0;

        /// <summary>
        /// Mutual exclusion mechanism to freeze threads on
        /// freeze command from PuppetMaster
        /// </summary>
        private static Mutex _mutex = new Mutex();
        /// <summary>
        /// Bool variable to know when to unfreeze the server
        /// </summary>
        private bool _isFrozen = false;

        /// <summary>
        /// true -> threads running 
        /// false -> threads not running
        /// </summary>
        private ManualResetEvent _manualResetEvent = new ManualResetEvent(true);

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

            _broadcastNewMeetingDelegate = new BroadcastNewMeetingDelegate(BroadcastNewMeetingToServer);
            _broadcastJoinDelegate = new BroadcastJoinDelegate(BroadcastJoinToServer);
            _broadcastCloseDelegate = new BroadcastCloseDelegate(BroadcastCloseToServer);
            _broadcastUpdateLocationDelegate = new BroadcastUpdateLocationDelegate(BroadcastUpdateLocationToServer);
        }

        // ------------------- COMMANDS SENT BY CLIENTS -------------------


        public void RegisterUser(string username, string clientUrl, string urlFailed = null)
        {
            Console.WriteLine("REGISTER USER");
            while (_isFrozen) { }
            Thread.Sleep(RandomIncomingMessageDelay());

            // obtain client remote object
            if (_clients.TryAdd(username, (IClient)Activator.GetObject(typeof(IClient), clientUrl)))
            {
                Console.WriteLine("BEFORE LOCK");
                lock (_clientUrls)
                {
                    Console.WriteLine("BEFORE ADD");
                    _clientUrls.Add(clientUrl);
                    Console.WriteLine("New user {0} with url {0} registered.", username, clientUrl);
                }
                
            }
            else
            {
                Console.WriteLine("not possible to register user {0} with URL: {1}. Try again", username, clientUrl);
            }
            BroadcastNewClient(clientUrl);
        }

        public List<string> AskForUpdateClients(string urlFailed = null)
        {
            Console.WriteLine("askforupdate");
            foreach (string user in _clientUrls) {
                Console.WriteLine("client url {0}", user );
            }
            return _clientUrls;

        }

        public void Create(MeetingProposal proposal, string urlFailed = null)
        {
            while (_isFrozen) { }
            Thread.Sleep(RandomIncomingMessageDelay());

            if (_currentMeetingProposals.TryAdd(proposal.Topic, proposal))
            {
                // we create a new vector clock for the new meeting
                _meetingsClocks[proposal.Topic] = new VectorClock(SERVER_URL, _servers.Keys);
                _meetingsClocks[proposal.Topic].printVectorClock(proposal.Topic);

                // we create a new operations log for the new meeting
                _operationsLog[proposal.Topic] = new List<Operation>();
                VectorClock copy = new VectorClock(_meetingsClocks[proposal.Topic]._currentVectorClock);
                _operationsLog[proposal.Topic].Add(new CreateOperation(copy, proposal));

                Console.WriteLine("Created new meeting proposal for " + proposal.Topic + ".");

                BroadcastNewMeeting(proposal);
            }
            else
            {
                Console.WriteLine("Not possible to create meeting with topic {0}", proposal.Topic);
            }
        }


        public void List(string name, Dictionary<string,MeetingProposal> knownProposals, string urlFailed = null)
        {
            while (_isFrozen) { }
            Thread.Sleep(RandomIncomingMessageDelay());

            Dictionary<string, MeetingProposal> proposals = new Dictionary<string, MeetingProposal>();

            foreach (KeyValuePair<string, MeetingProposal> proposal in _currentMeetingProposals)
            {

                if (knownProposals.ContainsKey(proposal.Value.Topic))
                {
                    proposals.Add(proposal.Value.Topic, proposal.Value);
                }
            }
            _clients[name].UpdateList(proposals);


        }


        public void Join(string username, string topic, MeetingRecord record, string urlFailed = null, bool local = true)
        {
            while (_isFrozen) { }
            Thread.Sleep(RandomIncomingMessageDelay());
            MeetingProposal proposal;
            if (!_currentMeetingProposals.TryGetValue(topic, out proposal))
            {
                // if the server does not have a meeting, he tells the client to switch to a different server
                _clients[username].SwitchServer();
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

                        // we update the respective vector clock
                        if (local) incrementVectorClock(topic);

                        // we update the respective log
                        updateLog(topic, record, username);

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

                    // we update the respective vector clock
                    incrementVectorClock(topic);

                    // we update the respective log
                    updateLog(topic, record, username);

                    BroadcastJoin(username, proposal, record);
                }

                Console.WriteLine(record.Name + " joined meeting proposal " + proposal.Topic + ".");
            }
        }

        public void Close(string topic, string urlFailed = null)
        {
            while (_isFrozen) { }
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
            foreach (KeyValuePair<string, Room> room in location.Rooms)
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

                // sort records by VectorClock
                proposal.Records.Sort();

                
                foreach (MeetingRecord record in proposal.Records)
                {
                    Console.WriteLine(record.ToString());

                    if (record.DateLocationSlots.Contains(finalDateLocation))
                    {
                        countInvitees++;

                        Console.WriteLine("record " + record);

                        if (countInvitees > maxCapacity)
                        {
                            proposal.AddFullRecord(record);
                        }
                        else
                        {
                            proposal.Participants.Add(record.Name);
                        }
                    }
                }
            }
            Console.WriteLine(proposal.Coordinator + " closed meeting proposal " + proposal.Topic + ".");

            // we update the respective vector clock
            incrementVectorClock(topic);

            // we update the respective log
            updateLog(topic);

            BroadcastClose(proposal);
        }


        // ------------------- COMMUNICATION WITH OTHER SERVERS -------------------
        // servers should send updates to other servers so that they maintain the state distributed


        public void BroadcastNewClient(string url)
        {
            foreach (KeyValuePair<string, IServer> server in _servers)
            {
                server.Value.ReceiveNewClient(url);
            }
        }

        public void ReceiveNewClient(string url)
        {
            lock (_clientUrls)
            {
                _clientUrls.Add(url);
                Console.WriteLine("Receive new user with url {0}", url);
            }
        }

        public void BroadcastNewMeeting(MeetingProposal proposal)
        {
            int n_acks = 0;
            List<IAsyncResult> res = new List<IAsyncResult>();
            List<bool> res_bool = new List<bool>();
            foreach (KeyValuePair<string, IServer> server in _servers)
            {
                if (_serversStatus[server.Key] == false)
                {
                    res.Add(_broadcastNewMeetingDelegate.BeginInvoke(server.Value, server.Key, proposal, BroadcastNewMeetingCallback, server.Key));
                    res_bool.Add(false);
                }

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
            Console.WriteLine("going to inform server of new meeting {0}", proposal.Topic);
            server.ReceiveNewMeeting(proposal, _meetingsClocks[proposal.Topic]);


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
                Console.WriteLine("Server {0} is dead", res.AsyncState);
                _serversStatus[(string)res.AsyncState] = true;
            }
        }

        public void ReceiveNewMeeting(MeetingProposal meeting, VectorClock vector)
        {
            while (_isFrozen) { }
            //if (SERVER_ID == "1")
            //{
            //    Thread.Sleep(600000000);
            //}
            Thread.Sleep(RandomIncomingMessageDelay());

            if (_currentMeetingProposals.TryAdd(meeting.Topic, meeting))
            {
                _meetingsClocks[meeting.Topic] = vector;
                _meetingsClocks[meeting.Topic].printVectorClock(meeting.Topic);

                // we create a new operations log for the new meeting
                _operationsLog[meeting.Topic] = new List<Operation>();
                _operationsLog[meeting.Topic].Add(new CreateOperation(_meetingsClocks[meeting.Topic], meeting));

                Console.WriteLine("received new meeting {0}.", meeting.Topic);
                //Console.WriteLine("ALL TIME CLOCKS:");
                //foreach (KeyValuePair<String, VectorClock> pair in _meetingsClocks)
                //    pair.Value.printVectorClock(pair.Key);
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
                if (_serversStatus[server.Key] == false)
                {
                    res.Add(_broadcastJoinDelegate.BeginInvoke(server.Value, server.Key, username, proposal, record, BroadcastJoinCallback, null));
                    res_bool.Add(false);
                }
            }
        }

        public string BroadcastJoinToServer(IServer server, string url, string username, MeetingProposal proposal, MeetingRecord record)
        {
            Console.WriteLine("going to send join {0}", proposal.Topic);
            server.ReceiveJoin(username, proposal, record, _meetingsClocks[proposal.Topic]);
            return url;
        }

        public void BroadcastJoinCallback(IAsyncResult res)
        {

            //try
            //{
                string returnValue = _broadcastJoinDelegate.EndInvoke(res);
                Console.WriteLine("finished sending join to " + returnValue);
            //}
            //catch (Exception ex)
            //{
            //    Console.WriteLine("Server {0} is dead", res.AsyncState);
            //    _serversStatus[(string)res.AsyncState] = true;
            //}
        }

        public void ReceiveJoin(string username, MeetingProposal proposal, MeetingRecord record, VectorClock newVector)
        {
            while (_isFrozen) { }
            //if (SERVER_ID == "1")
            //{
            //    Thread.Sleep(600000000);
            //}
            Thread.Sleep(RandomIncomingMessageDelay());

            // UPDATE VECTOR CLOCK
            updateVectorClock(proposal, newVector);

            Console.WriteLine("received join {0}", proposal.Topic);

            MeetingProposal previousProposal;

            if (_currentMeetingProposals.TryGetValue(proposal.Topic, out previousProposal))
            {
                if (!previousProposal.Records.Contains(record)) //stop condition request already received
                {
                    _currentMeetingProposals[proposal.Topic] = proposal;
                    BroadcastJoin(username, proposal, record);
                    //Join(username, proposal.Topic, record);
                }
            }
            else
            {
                _currentMeetingProposals.TryAdd(proposal.Topic, proposal);
            }

        }

        public void BroadcastClose(MeetingProposal proposal)
        {
            List<IAsyncResult> res = new List<IAsyncResult>();
            List<bool> res_bool = new List<bool>();

            foreach (KeyValuePair<string, IServer> server in _servers)
            {
                if (_serversStatus[server.Key] == false)
                {
                    res.Add(_broadcastCloseDelegate.BeginInvoke(server.Value, server.Key, proposal, BroadcastCloseCallback, null));
                    res_bool.Add(false);
                }
            }
        }

        public string BroadcastCloseToServer(IServer server, string url, MeetingProposal proposal)
        {
            Console.WriteLine("going to send close {0}", proposal.Topic);
            server.ReceiveClose(proposal, _meetingsClocks[proposal.Topic]);
            //Console.WriteLine("Finished sending close!");
            Thread.Sleep(3000);
            return url;
        }

        public void BroadcastCloseCallback(IAsyncResult res)
        {
            //try
            //{
            string returnValue = _broadcastCloseDelegate.EndInvoke(res);
            Console.WriteLine("finished sending close to " + returnValue);
            //}
            //catch (Exception ex)
            //{
            //Console.WriteLine("Server {0} is dead", res.AsyncState);
            //_serversStatus[(string)res.AsyncState] = true;
            //}
        }

        public void ReceiveClose(MeetingProposal proposal, VectorClock newVector)
        {
            while (_isFrozen) { }
            Thread.Sleep(RandomIncomingMessageDelay());

            Console.WriteLine("received close {0}", proposal.Topic);

            // UPDATE VECTOR CLOCK
            updateVectorClock(proposal, newVector);

            MeetingProposal previousProposal;

            if (_currentMeetingProposals.TryGetValue(proposal.Topic, out previousProposal))
            {
                if (previousProposal.MeetingStatus != MeetingStatus.CLOSED)
                {
                    _currentMeetingProposals[proposal.Topic] = proposal;
                    BroadcastClose(proposal);
                }
            }
            else _currentMeetingProposals.TryAdd(proposal.Topic, proposal);

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
            while (_isFrozen) { }
            Thread.Sleep(RandomIncomingMessageDelay());

            Console.WriteLine("received updated location {0}", location.Name);
            _locations[location.Name] = location;
        }

        // ------------------- VECTOR CLOCK -------------------

        public void incrementVectorClock(String meeting)
        {

            _meetingsClocks[meeting].incrementVectorClock(SERVER_URL);
            _meetingsClocks[meeting].printVectorClock(meeting);
        }

        public void updateVectorClock(MeetingProposal proposal, VectorClock newVector)
        {
            //Console.WriteLine("UPDATE VECTOR CLOCK");
            // VECTOR CLOCK UPDATE
            VectorClock updatedMeetingVector; // after receiving the new operation, this will be the resulting vector clock
            if (_meetingsClocks.TryGetValue(proposal.Topic, out updatedMeetingVector))
            {
                //Console.WriteLine("MEETING CLOCK DA MEETING {0} EXISTE.", proposal.Topic);
                // this server knows this meeting
                foreach (KeyValuePair<String, int> clock in _meetingsClocks[proposal.Topic]._currentVectorClock)
                {
                    //Console.WriteLine("GONNA COMPARE CLOCK OF SERVER {0} ", clock.Key);
                    // if the received clock is only one step ahead we only need to do a simple update
                    if (newVector._currentVectorClock[clock.Key] - clock.Value == 1)
                    {
                        //Console.WriteLine("RECEIVED CLOCK IS ONE STEP AHEAD");
                        updatedMeetingVector._currentVectorClock[clock.Key] = newVector._currentVectorClock[clock.Key];
                    }
                    // if the received clock is more than one step ahead we need to request the missing information
                    else if (newVector._currentVectorClock[clock.Key] - clock.Value > 1)
                    {
                        //Console.WriteLine("RECEIVED CLOCK IS MORE THAN 1 STEP AHEAD");
                        Thread.Sleep(3000);
                        // TO DO
                        // REQUEST MISSING INFORMATION FROM THE SERVER THAT IS A FEW STEPS AHEAD, NOT FROM ALL SERVERS
                        getMissingInformation(clock.Key, proposal.Topic, clock.Value, newVector._currentVectorClock[clock.Key]);

                        // now that we have all the missing information we update the vector clock
                        updatedMeetingVector._currentVectorClock[clock.Key] = newVector._currentVectorClock[clock.Key];
                    }

                    // if the received clock is behind the known we don't care

                }
            }
            else
            {
                // the server doesn't know about this meeting so we need to request the missing information
                // TO DO CHECK FOR REPEATED OPERATIONS
                // REQUEST MISSING INFORMATION FROM ALL SERVERS
                foreach (KeyValuePair<String, IServer> server in _servers)
                    getMissingInformation(server.Key, proposal.Topic, -1, newVector._currentVectorClock[server.Key]);

                // now that we have all the missing information we update the vector clock
                foreach (KeyValuePair<String, int> clock in _meetingsClocks[proposal.Topic]._currentVectorClock)
                {
                    // if the received clock is only one step ahead we only need to do a simple update
                    if (newVector._currentVectorClock[clock.Key] - clock.Value == 1)
                    {
                        updatedMeetingVector._currentVectorClock[clock.Key] = newVector._currentVectorClock[clock.Key];
                    }
                }
            }

            _meetingsClocks[proposal.Topic] = updatedMeetingVector;
                
        }

        public void updateLog(String meetingTopic, MeetingRecord record = null, String username = null)
        {
            VectorClock copy = new VectorClock(_meetingsClocks[meetingTopic]._currentVectorClock);

            if (record == null)
            {
                // we register a new close operation
                _operationsLog[meetingTopic].Add(new CloseOperation(copy, _currentMeetingProposals[meetingTopic]));
            }
            else
            {
                // meeting record will now have a VectorClock associated for posterior sorting
                record._vector = copy;
                // we register a new join operation
                _operationsLog[meetingTopic].Add(new JoinOperation(copy, record, username));
            }

            //Console.WriteLine("OPERATIONS LOG OF MEETING {0} AFTER UPDATE LOG", meetingTopic);
            //foreach (Operation op in _operationsLog[meetingTopic])
            //{
            //    op.printOperation();
            //}
        }

        public void getMissingInformation(String serverURL, String topic, int currentClock, int maxClock)
        {
            //Console.WriteLine("GONNA GET MISSING INFORMATION FROM {0} {1} {2} {3}", serverURL, topic, currentClock, maxClock);
            //Thread.Sleep(3000);
            // retrieve missing information starting from t = currentClock up to t = maxClock
            List<Operation> missingOperations = _servers[serverURL].retrieveOperations(topic, currentClock, maxClock);

            //Console.WriteLine("RETRIEVED LIST OF OPERATIONS WITH SIZE: {0}", missingOperations.Count);
            //Thread.Sleep(3000);
            // gonna execute all the missing operations
            foreach (Operation op in missingOperations)
                op.executeOperation(this, topic);
        }

        public List<Operation> retrieveOperations(string meetingTopic, int minClock, int maxClock)
        {
            List<Operation> toSendOperations = new List<Operation>();
            foreach (Operation op in _operationsLog[meetingTopic])
            {
                if (op.GetVectorClock()._currentVectorClock[SERVER_URL] > minClock && op.GetVectorClock()._currentVectorClock[SERVER_URL] < maxClock)
                    toSendOperations.Add(op);
            }

            return toSendOperations;
        }


        // ------------------- PUPPET MASTER COMMANDS -------------------

        public void Status()
        {
            Console.WriteLine("Server is active. URL: {0}", SERVER_URL);
        }

        public void Freeze()
        {
            Console.WriteLine("FREEZE");
            _isFrozen = true;
        }

        public void Unfreeze()
        {
            Console.WriteLine("UNFREEZE");
            _isFrozen = false;
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
            if (!_servers.ContainsKey(serverUrl))
            {
                _servers.TryAdd(serverUrl, server);
                _serversStatus.TryAdd(serverUrl, false);
            }
            return server;
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
        static void Main(string[] args)
        {

            // caso em que nao há ordem
            ConcurrentDictionary<string, int> dic1 = new ConcurrentDictionary<string, int> { ["1"] = 1, ["2"] = 1, ["3"] = 1 };
            ConcurrentDictionary<string, int> dic2 = new ConcurrentDictionary<string, int> { ["1"] = 3, ["2"] = 0, ["3"] = 0 };
            ConcurrentDictionary<string, int> dic3 = new ConcurrentDictionary<string, int> { ["1"] = 1, ["2"] = 1, ["3"] = 0 };
            ConcurrentDictionary<string, int> dic4 = new ConcurrentDictionary<string, int> { ["1"] = 1, ["2"] = 0, ["3"] = 0 };
            ConcurrentDictionary<string, int> dic5 = new ConcurrentDictionary<string, int> { ["1"] = 5, ["2"] = 4, ["3"] = 3 };

            VectorClock vec1 = new VectorClock(dic1);
            VectorClock vec2 = new VectorClock(dic2);
            VectorClock vec3 = new VectorClock(dic3);
            VectorClock vec4 = new VectorClock(dic4);
            VectorClock vec5 = new VectorClock(dic5);

            MeetingRecord rec1 = new MeetingRecord
            {
                Name = "Adriana",
                _vector = vec1
            };

            MeetingRecord rec2 = new MeetingRecord
            {
                Name = "Bárbara",
                _vector = vec2
            };
            MeetingRecord rec5 = new MeetingRecord
            {
                Name = "Cátia",
                _vector = vec5
            };
            MeetingRecord rec3 = new MeetingRecord
            {
                Name = "Diogo",
                _vector = vec3
            };
            MeetingRecord rec4 = new MeetingRecord
            {
                Name = "Eva",
                _vector = vec4
            };

            List<MeetingRecord> allRecords = new List<MeetingRecord> { rec5, rec2, rec3, rec4, rec1 };
            allRecords.Sort();
            Console.WriteLine(" ");
            Console.WriteLine("FINAL ORDER");
            foreach (MeetingRecord rec in allRecords)
            {
                Console.WriteLine(rec.Name);
                rec._vector.printVectorClock("");
            }

            List<MeetingRecord> allRecords1 = new List<MeetingRecord> { rec5, rec1, rec2, rec3, rec4 };
            allRecords1.Sort();
            Console.WriteLine(" ");
            Console.WriteLine("FINAL ORDER");
            foreach (MeetingRecord rec in allRecords1)
            {
                Console.WriteLine(rec.Name);
                rec._vector.printVectorClock("");
            }

            List<MeetingRecord> allRecords2 = new List<MeetingRecord> { rec5, rec1, rec3, rec2, rec4 };
            allRecords2.Sort();
            Console.WriteLine(" ");
            Console.WriteLine("FINAL ORDER");
            foreach (MeetingRecord rec in allRecords2)
            {
                Console.WriteLine(rec.Name);
                rec._vector.printVectorClock("");
            }


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