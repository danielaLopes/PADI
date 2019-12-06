
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

    public delegate string BroadcastNewMeetingDelegate(AckController ackController, IServer server, string url, MeetingProposal proposal);
    public delegate string BroadcastJoinDelegate(AckController ackController, IServer server, string url, string username, MeetingProposal proposal, MeetingRecord record);
    public delegate string BroadcastCloseDelegate(AckController ackController, IServer server, string url, MeetingProposal proposal);
    public delegate string BroadcastDeadServersDelegate(AckController ackController, IServer server, string url, string deadServer);
    public delegate string BroadcastUpdateLocationDelegate(AckController ackController, IServer server, string url, Location location);

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
        /// server responsible for the slots
        /// </summary>
        private string _serverLocation = null;

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
        private BroadcastDeadServersDelegate _broadcastDeadServersDelegate;
        private BroadcastUpdateLocationDelegate _broadcastUpdateLocationDelegate;


        /// <summary>
        /// Bool variable to know when to unfreeze the server
        /// </summary>
        private bool _isFrozen = false;


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

             //every server will know the first on the list will be responsible

            _maxFaults = maxFaults;

            _minDelay = minDelay;
            _maxDelay = maxDelay;

            _broadcastNewMeetingDelegate = new BroadcastNewMeetingDelegate(BroadcastNewMeetingToServer);
            _broadcastJoinDelegate = new BroadcastJoinDelegate(BroadcastJoinToServer);
            _broadcastCloseDelegate = new BroadcastCloseDelegate(BroadcastCloseToServer);
            _broadcastDeadServersDelegate = new BroadcastDeadServersDelegate(BroadcastDeadServersToServer);
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
                {   //increments the number of invitees that can go to that slot
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
                    if(local) incrementVectorClock(topic);

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

            if (_serverLocation == null)
            {
                var list = _servers.Keys.ToList();
                list.Sort();
                _serverLocation = list[0];
                Console.WriteLine("server location" + _serverLocation);
            }

            MeetingProposal proposal = _currentMeetingProposals[topic];

            DateLocation finalDateLocation = new DateLocation();

            // each dateLocation has the number of invitees that can go to that slot
            foreach (DateLocation dateLocation in proposal.DateLocationSlots) 
            {
                if (dateLocation.Invitees > finalDateLocation.Invitees) //chooses the dateLocation with more invitees
                {

                    finalDateLocation = dateLocation;
                }
            }

            Room finalRoom = _servers[_serverLocation].getAvailableRoom(finalDateLocation, proposal);

            
            if (finalDateLocation.Invitees < proposal.MinAttendees || finalRoom == null)
            {
                proposal.MeetingStatus = MeetingStatus.CANCELLED;
                //Console.WriteLine("finalRoom " + finalRoom );
            }

            else
            {
                int countInvitees = 0;
                proposal.MeetingStatus = MeetingStatus.CLOSED;

                proposal.FinalRoom = finalRoom;
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

                        //if there's more invitees than the room capacity they go to a special list
                        if (countInvitees > proposal.FinalRoom.Capacity)//maxCapacity)
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

        public Room getAvailableRoom(DateLocation finalDateLocation, MeetingProposal proposal)
        {
            
            if (finalDateLocation.Invitees < proposal.MinAttendees) return null;

            Location location = _locations[finalDateLocation.LocationName];//the final location
            SortedDictionary<int, Room> possibleRooms = new SortedDictionary<int, Room>(); //rooms not booked
            int maxCapacity = 0;
            foreach (KeyValuePair<string, Room> room in location.Rooms)
            {
               // if (room.Value.RoomAvailability == Room.RoomStatus.NONBOOKED)
               //checks if the room is available for that day
                if (!room.Value.BookedDays.Contains(finalDateLocation))
                {
                    possibleRooms.Add(room.Value.Capacity, room.Value); 

                    if (maxCapacity < room.Value.Capacity) maxCapacity = room.Value.Capacity; // room with the biggest capacity
                }
            }

            Room finalRoom = new Room();

            //if there's no room for everybody the final room is the one with the biggest capacity
            if (maxCapacity < finalDateLocation.Invitees && possibleRooms.Count != 0)
            {
                finalRoom = possibleRooms[maxCapacity];
            }

            //else choose the the first room that fits everyone
            else
            {
                foreach (KeyValuePair<int, Room> room in possibleRooms)
                {
                    if (room.Key >= finalDateLocation.Invitees)
                    {
                        finalRoom = room.Value;
                        break;
                    }
                }
            }

            if (possibleRooms.Count > 0)
            {
                //changes the room availability to booked if it doesn't get cancelled
                //_locations[finalDateLocation.LocationName].Rooms[finalRoom.Name].RoomAvailability = Room.RoomStatus.BOOKED;
                _locations[finalDateLocation.LocationName].Rooms[finalRoom.Name].AddBookedDay(finalDateLocation);
                BroadcastUpdateLocation(location);
                return finalRoom;
                
            }
            return null;
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

        public void WaitForMaxFault(AckController ackController)
        {
            //int n_acks = 0;
            while (true)
            {
                if (ackController.N_acks >= _maxFaults) break;
                //Console.WriteLine("acks " + ackController.N_acks);
                //Console.WriteLine("maxFaults " + _maxFaults);
                //int index = WaitHandle.WaitAny(ackController.Handles);
                //n_acks++;
                /*Console.WriteLine("index" + index);

                var list_handle = new List<WaitHandle>(ackController.Handles);
                list_handle.RemoveAt(index);
                ackController.Handles = list_handle.ToArray();*/

            }
            Console.WriteLine("acks" + ackController.N_acks);

        }

        //BROADCAST NEW MEETING

        public void BroadcastNewMeeting(MeetingProposal proposal)
        {

            WaitHandle[] handles = new WaitHandle[_servers.Count];
            AckController ackController = new AckController();


            int i = 0;
            foreach (KeyValuePair<string, IServer> server in _servers)
            {
                if (_serversStatus[server.Key] == false)
                {
                    handles[i] = _broadcastNewMeetingDelegate.BeginInvoke(ackController, server.Value, server.Key, proposal, BroadcastNewMeetingCallback, server.Key).AsyncWaitHandle;
                    i++;
                }
            }

            WaitForMaxFault(ackController);
            
        }

        public string BroadcastNewMeetingToServer(AckController ackController, IServer server, string url, MeetingProposal proposal)
        {
            Console.WriteLine("going to inform server of new meeting {0}", proposal.Topic);
            server.ReceiveNewMeeting(proposal, _meetingsClocks[proposal.Topic]);
            ackController.N_acks++;

            return url;
        }

        public void BroadcastNewMeetingCallback(IAsyncResult res)
        {
            try
            {
                string returnValue = _broadcastNewMeetingDelegate.EndInvoke(res);
                Console.WriteLine("finished sending new meeting to " + returnValue);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Server {0} is dead", res.AsyncState);
                if (_serversStatus[(string)res.AsyncState] != true)
                {
                    _maxFaults--;
                    _serversStatus[(string)res.AsyncState] = true;
                }
                BroadcastDeadServers((string)res.AsyncState);
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

        //BROADCAST JOIN

        public void BroadcastJoin(string username, MeetingProposal proposal, MeetingRecord record)
        {
            WaitHandle[] handles = new WaitHandle[_servers.Count];
            AckController ackController = new AckController();
            List<bool> res_bool = new List<bool>();

            int i = 0;
            foreach (KeyValuePair<string, IServer> server in _servers)
            {

                if (_serversStatus[server.Key] == false)
                {
                    handles[i] = _broadcastJoinDelegate.BeginInvoke(ackController, server.Value, server.Key, username, proposal, record, BroadcastJoinCallback, server.Key).AsyncWaitHandle;
                    i++;
                    res_bool.Add(false);
                }
            }

            WaitForMaxFault(ackController);
        }

        public string BroadcastJoinToServer(AckController ackController, IServer server, string url, string username, MeetingProposal proposal, MeetingRecord record)
        {
            Console.WriteLine("going to send join {0}", proposal.Topic);
            server.ReceiveJoin(username, proposal, record, _meetingsClocks[proposal.Topic]);
            ackController.N_acks++;

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
                Console.WriteLine("Server {0} is dead", res.AsyncState);
                if (_serversStatus[(string)res.AsyncState] != true)
                {
                    _maxFaults--;
                    _serversStatus[(string)res.AsyncState] = true;
                }
                BroadcastDeadServers((string)res.AsyncState);
            }
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
                    //_currentMeetingProposals[proposal.Topic] = proposal;
                    //BroadcastJoin(username, proposal, record);
                    Join(username, proposal.Topic, record, local:false);
                }
            }
            else
            {
                _currentMeetingProposals.TryAdd(proposal.Topic, proposal);
            }

        }

        //BROADCAST CLOSE

        public void BroadcastClose(MeetingProposal proposal)
        {
            WaitHandle[] handles = new WaitHandle[_servers.Count];
            AckController ackController = new AckController();
            List<bool> res_bool = new List<bool>();

            int i = 0;
            foreach (KeyValuePair<string, IServer> server in _servers)
            {
                if (_serversStatus[server.Key] == false)
                {
                    handles[i] = _broadcastCloseDelegate.BeginInvoke(ackController, server.Value, server.Key, proposal, BroadcastCloseCallback, server.Key).AsyncWaitHandle;
                    i++;
                    res_bool.Add(false);
                }
            }

            WaitForMaxFault(ackController);
        }

        public string BroadcastCloseToServer(AckController ackController, IServer server, string url, MeetingProposal proposal)
        {
            Console.WriteLine("going to send close {0}", proposal.Topic);
            server.ReceiveClose(proposal, _meetingsClocks[proposal.Topic]);
            //Console.WriteLine("Finished sending close!");
            //Thread.Sleep(3000);
            ackController.N_acks++;
            return url;
        }

        public void BroadcastCloseCallback(IAsyncResult res)
        {
            try
            {
                string returnValue = _broadcastCloseDelegate.EndInvoke(res);
                Console.WriteLine("finished sending close to " + returnValue);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Server {0} is dead", res.AsyncState);
                if (_serversStatus[(string)res.AsyncState] != true)
                {
                    _maxFaults--;
                    _serversStatus[(string)res.AsyncState] = true;
                }
                BroadcastDeadServers((string)res.AsyncState);
            }
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
                if (previousProposal.MeetingStatus == MeetingStatus.OPEN )
                {
                    _currentMeetingProposals[proposal.Topic] = proposal;
                    BroadcastClose(proposal);
                }
            }
            else _currentMeetingProposals.TryAdd(proposal.Topic, proposal);

        }

        //BROADCAST LOCATION

        public void BroadcastUpdateLocation(Location location)
        {

            WaitHandle[] handles = new WaitHandle[_servers.Count];
            AckController ackController = new AckController();
            int i = 0;
            foreach (KeyValuePair<string, IServer> server in _servers)
            {
                handles[i] = _broadcastUpdateLocationDelegate.BeginInvoke(ackController, server.Value, server.Key, location, BroadcastUpdateLocationCallback, server.Key).AsyncWaitHandle;
                i++;
            }

            WaitForMaxFault(ackController);
        }

        public string BroadcastUpdateLocationToServer(AckController ackController, IServer server, string url, Location location)
        {
            Console.WriteLine("going to send updated location {0}", location.Name);
            server.ReceiveUpdateLocation(location);
            ackController.N_acks++;
            return url;
        }

        public void BroadcastUpdateLocationCallback(IAsyncResult res)
        {
            try
            {
                string returnValue = _broadcastUpdateLocationDelegate.EndInvoke(res);
                Console.WriteLine("finished sending update location to " + returnValue);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Server {0} is dead", res.AsyncState);
                if (_serversStatus[(string)res.AsyncState] != true)
                {
                    _maxFaults--;
                    _serversStatus[(string)res.AsyncState] = true;
                }
                BroadcastDeadServers((string)res.AsyncState);
            }
        }

        public void ReceiveUpdateLocation(Location location)
        {
            while (_isFrozen) { }
            Thread.Sleep(RandomIncomingMessageDelay());

            Console.WriteLine("received updated location {0}", location.Name);

            //if(_locations[location.Name] != location)
                _locations[location.Name] = location;
        }

        //BROADCAST DEAD SERVERS
        public void BroadcastDeadServers(string deadServer)
        {
            WaitHandle[] handles = new WaitHandle[_servers.Count];
            AckController ackController = new AckController();

            int i = 0;
            foreach (KeyValuePair<string, IServer> server in _servers)
            {
                if (_serversStatus[server.Key] == false)
                {
                    handles[i] = _broadcastDeadServersDelegate.BeginInvoke(ackController, server.Value, server.Key, deadServer, BroadcastDeadServersCallback, server.Key).AsyncWaitHandle;
                    i++;
                }
            }

            WaitForMaxFault(ackController);
        }

        public string BroadcastDeadServersToServer(AckController ackController, IServer server, string url, string deadServer)
        {
            Console.WriteLine("going to send dead server {0}", deadServer);
            server.ReceiveDeadServers(deadServer);
            ackController.N_acks++;
            return url;
        }

        public void BroadcastDeadServersCallback(IAsyncResult res)
        {
            try
            {
                string returnValue = _broadcastDeadServersDelegate.EndInvoke(res);
                Console.WriteLine("finished sending dead server to " + returnValue);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Server {0} is dead", res.AsyncState);
                if (_serversStatus[(string)res.AsyncState] != true)
                {
                    _maxFaults--;
                    _serversStatus[(string)res.AsyncState] = true;
                }
                BroadcastDeadServers((string)res.AsyncState);
            }
        }

        public void ReceiveDeadServers(string deadServer)
        {
            while (_isFrozen) { }
            Thread.Sleep(RandomIncomingMessageDelay());

            Console.WriteLine("received dead server {0}",deadServer);
            if (_serversStatus[deadServer] != true)
            {
                _serversStatus[deadServer] = true;
                _maxFaults--;
                BroadcastDeadServers(deadServer);
            }
        }

        // ------------------- VECTOR CLOCK -------------------

        public void incrementVectorClock(String meeting)
        {

            _meetingsClocks[meeting].incrementVectorClock(SERVER_URL);
            _meetingsClocks[meeting].printVectorClock(meeting);
        }

        public void updateVectorClock(MeetingProposal proposal, VectorClock newVector)
        {
            Console.WriteLine("UPDATE VECTOR CLOCK");
            // VECTOR CLOCK UPDATE
            VectorClock updatedMeetingVector; // after receiving the new operation, this will be the resulting vector clock
            if (_meetingsClocks.TryGetValue(proposal.Topic, out updatedMeetingVector))
            {
                Console.WriteLine("MEETING CLOCK DA MEETING {0} EXISTE.", proposal.Topic);
                // this server knows this meeting
                foreach (KeyValuePair<String, int> clock in _meetingsClocks[proposal.Topic]._currentVectorClock)
                {
                    Console.WriteLine("GONNA COMPARE CLOCK OF SERVER {0} ", clock.Key);
                    // if the received clock is only one step ahead we only need to do a simple update
                    if (newVector._currentVectorClock[clock.Key] - clock.Value == 1)
                    {
                        Console.WriteLine("RECEIVED CLOCK IS ONE STEP AHEAD");
                        updatedMeetingVector._currentVectorClock[clock.Key] = newVector._currentVectorClock[clock.Key];
                    }
                    // if the received clock is more than one step ahead we need to request the missing information
                    else if (newVector._currentVectorClock[clock.Key] - clock.Value > 1)
                    {

                        //Console.WriteLine("RECEIVED CLOCK IS MORE THAN 1 STEP AHEAD");
                        //Thread.Sleep(3000);
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