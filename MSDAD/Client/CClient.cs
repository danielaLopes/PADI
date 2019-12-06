using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Threading;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using ClassLibrary;

namespace Client
{
    public class CClient : MarshalByRefObject, IClient, ILocalClient
    {
        private readonly string USERNAME;
        private readonly string CLIENT_URL;

        private ConcurrentDictionary<string, IClient> _clients;
        private List<string> _knownClientUrls;
        
        // saves the meeting proposal the client knows about (created or received invitation)
        public Dictionary<string, MeetingProposal> _knownMeetingProposals;

        // obtain server remote object
        private IServer _remoteServer;
        // keeps track of the current server's url
        private string _remoteServerUrl;

        /// <summary>
        /// keeps track of the dead servers (true if dead, false if alive 
        /// key -> serverUrl, value -> boolean to say if server is dead
        /// </summary>
        private Dictionary<string, bool> _serverStatus;

        private int TIMEOUT = 10000;

        /// <summary>
        /// Creates TCP channel, saves relevant information for remoting, registers itself as
        /// remote object and gets preferred server's remote object.
        /// </summary>
        /// <param name="username"></param>
        /// <param name="clientUrl"></param>
        /// <param name="serverUrl"></param>
        public CClient(string username, string clientUrl, string serverUrl, List<string> backupServers, List<string> clientsUrls = null)
        {
            USERNAME = username;
            CLIENT_URL = clientUrl;

            /*IDictionary properties = new System.Collections.Hashtable();
            //properties["name"] = Ipc_Channel_Name;
            properties["port"] = PortExtractor.Extract(CLIENT_URL);
            properties["connectionTimeout"] = 5000;
            BinaryClientFormatterSinkProvider clientProvider = new BinaryClientFormatterSinkProvider();
            BinaryServerFormatterSinkProvider serverProvider = new BinaryServerFormatterSinkProvider();

            // creates TCP channel
            TcpChannel clientChannel = new TcpChannel(properties, clientProvider, serverProvider);*/
            // creates TCP channel
            TcpChannel clientChannel = new TcpChannel(PortExtractor.Extract(CLIENT_URL));
            ChannelServices.RegisterChannel(clientChannel, false);
            // create the client's remote object
            RemotingServices.Marshal(this, username, typeof(CClient));

            Console.WriteLine("Client registered with username {0} with url {1}.", username, clientUrl);

            RegisterNewServer(serverUrl);

            _serverStatus = new Dictionary<string, bool>();
            foreach (string backup in backupServers)
            {
                _serverStatus.Add(backup, false);
                Console.WriteLine("Backup server assigned {0}", backup);
            }     

            _clients = new ConcurrentDictionary<string, IClient>();
            _knownClientUrls = new List<string>();           

            _knownMeetingProposals = new Dictionary<string, MeetingProposal>();

            // gets other client's remote objects and saves them
            if (clientsUrls != null)
            {
                UpdateClients(clientsUrls);  
            }
        }

        /// <summary>
        /// a separate task waits for an established timeout time and
        /// throws a timeout exception at the end
        /// </summary>
        public void Timeout()
        {
            Thread.Sleep(TIMEOUT);
            throw new TimeoutException("Server is dead due to timeout");
        }

        /// <summary>
        /// Determines whether the remote call to the server
        /// got completed or not
        /// </summary>
        /// <param name="contactedServer"></param>
        public string DetectServerDeath(bool contactedServer)
        {
            if (!contactedServer)
            {
                string previousUrl = _remoteServerUrl;
                SwitchServer();
                return previousUrl;    
            }
            return null;
        }

        public void RegisterNewServer(string serverUrl)
        {
            bool contactedServer = false;
            try
            {
                Parallel.Invoke(() => Timeout());
                // retrieve server's proxy
                _remoteServer = (IServer)Activator.GetObject(typeof(IServer), serverUrl);
                _remoteServerUrl = serverUrl;
                // register new user in remote server
                _remoteServer.RegisterUser(USERNAME, CLIENT_URL);
                contactedServer = true;
                Console.WriteLine("Registered with server {0}", serverUrl);
            }
            catch(Exception e)
            {
                DetectServerDeath(contactedServer);
            }

            Console.WriteLine("Registered with server {0}", serverUrl);
        }

        

        public void List()
        {
            bool contactedServer = false;
            try
            {
                Parallel.Invoke(() => Timeout());
                _remoteServer.List(USERNAME, _knownMeetingProposals);
                contactedServer = true;
            }
            catch (Exception e)
            {
                string previousUrl = DetectServerDeath(contactedServer);
                _remoteServer.List(USERNAME, _knownMeetingProposals, previousUrl);
            } 
            
            foreach (KeyValuePair<string, MeetingProposal> meetingProposal in _knownMeetingProposals)
            {
                MeetingProposal proposal = meetingProposal.Value;
                Console.WriteLine(proposal);
            }
        }

        public void Create(string meetingTopic, string minAttendees, List<string> slots, List<string> invitees = null)
        {
            bool contactedServer = false;
            try
            {
                Parallel.Invoke(() => Timeout());
                _knownClientUrls = _remoteServer.AskForUpdateClients();
                contactedServer = true;
                UpdateClients(_knownClientUrls);
            }
            catch (Exception e)
            {
                string previousUrl = DetectServerDeath(contactedServer);
                _knownClientUrls = _remoteServer.AskForUpdateClients(previousUrl);
                UpdateClients(_knownClientUrls);
            }

            List<DateLocation> parsedSlots = ParseSlots(slots);
            List<string> parsedInvitees = invitees;
            
            MeetingProposal proposal = new MeetingProposal
            {
                Coordinator = USERNAME,
                Topic = meetingTopic,
                MinAttendees = Int32.Parse(minAttendees),
                DateLocationSlots = parsedSlots,
                Invitees = parsedInvitees,
                Records = new SortedDictionary<string, MeetingRecord>(),
                FailedRecords = new List<MeetingRecord>(),
                FullRecords = new List<MeetingRecord>(),
                Participants = new List<string>(),
                MeetingStatus = MeetingStatus.OPEN
            };
            contactedServer = false;
            try
            {
                Parallel.Invoke(() => Timeout());
                _remoteServer.Create(proposal);
                contactedServer = true;
            }
            catch (Exception e)
            {
                string previousUrl = DetectServerDeath(contactedServer);
                _remoteServer.Create(proposal, previousUrl);
            }            

            _knownMeetingProposals.Add(proposal.Topic, proposal);

            if (invitees == null)
            {
                SendInvitations(new List<string>(_clients.Keys), proposal);
            }
            else
            {
                // if coordinator is in the invitees, he takes it away because he does
                // not need to receive an invitation
                if (invitees.Contains(USERNAME)) {
                    List<string> inviteesWithoutCoordinator = new List<string>();
                    foreach (string invitee in invitees)
                    {
                        if (!invitee.Equals(USERNAME))
                            inviteesWithoutCoordinator.Add(invitee);
                    }
                    SendInvitations(inviteesWithoutCoordinator, proposal);
                }
                else
                {
                    SendInvitations(invitees, proposal);
                }   
            }
        }

        public void Join(string meetingTopic, List<string> slots)
        {
            if (_knownMeetingProposals.ContainsKey(meetingTopic))
            {
                
                List<DateLocation> parsedSlots = ParseSlots(slots);
                MeetingRecord record = new MeetingRecord
                {
                    Name = USERNAME,
                    DateLocationSlots = parsedSlots,
                };
                bool contactedServer = false;
                try
                {
                    Parallel.Invoke(() => Timeout());
                    _remoteServer.Join(USERNAME, meetingTopic, record);
                    contactedServer = true;
                }
                catch(Exception e)
                {
                    string previousUrl = DetectServerDeath(contactedServer);
                    _remoteServer.Join(USERNAME, meetingTopic, record, urlFailed:previousUrl);
                }
            }
        }

        public void Close(string meetingTopic)
        {
            bool contactedServer = false;
            try
            {
                Parallel.Invoke(() => Timeout());
                _remoteServer.Close(meetingTopic);
                contactedServer = true;
            }
            catch(Exception e)
            {
                string previousUrl = DetectServerDeath(contactedServer);
                _remoteServer.Close(meetingTopic, previousUrl);
            }    
        }

        public void Wait(string milliseconds)
        {
            Thread.Sleep(Int32.Parse(milliseconds));
            Console.WriteLine("waited {0}", milliseconds);
        }

        public void UpdateList(Dictionary<string, MeetingProposal> proposals)
        {
            _knownMeetingProposals = proposals;
        }

        /// <summary>
        /// In case the server attributed to a client is slow,
        /// not updated or dead, the client gets initialized with
        /// a list of f server urls and chooses one of those
        /// those urls to contact a new server, by order of the
        /// urls received. This garantees the client is always going to
        /// know the url of a server to contact, even in case of f failures
        /// </summary>
        public void SwitchServer()
        {
            List<string> aliveUrls = new List<string>();
            foreach (KeyValuePair<string, bool> url in _serverStatus)
            {
                if (url.Value == false) aliveUrls.Add(url.Key);
            }

            Random randomizer = new Random();
            int random = randomizer.Next(aliveUrls.Count);

            RegisterNewServer(aliveUrls[random]);
        }

        /// <summary>
        /// When client receives a list with other clients' urls 
        /// he gets the respective remote objects
        /// </summary>
        /// <param name="clientsUrls"></param>
        public void UpdateClients(List<string> clientsUrls)
        {
            foreach (string url in clientsUrls)
            {
                string name = url.Split('/')[3];
                if (!name.Equals(USERNAME))
                    RegisterClient(name, url);
            }
        }

        /// <summary>
        /// Obtain the client's remote object and saves it
        /// along with the respective client name for easy search
        /// </summary>
        /// <param name="name"></param>
        /// <param name="clientUrl"></param>
        /// <returns></returns>
        public IClient RegisterClient(string name, string clientUrl)
        {
            Console.WriteLine("new client {0}", name);
            IClient client = (IClient)Activator.GetObject(typeof(IClient), clientUrl);
            _clients.TryAdd(name, client);

            return client;
        }

        public void Status()
        {
            Console.WriteLine("Client is active. URL: {0}", CLIENT_URL);
        }

        public void SendInvitations(List<string> invitees, MeetingProposal proposal)
        {
            // assumes every client knows every other client
            int threshold = 2;

            foreach (string invitee in invitees)
            {
                Console.WriteLine("invitee: {0}", invitee);
            }          

            // easy case: there are few invitees so we can 
            // send the invitations directly
            if (invitees.Count <= threshold)
            {
                foreach (string invitee in invitees)
                {   
                    // no need to send inviteesLeft because the invitation
                    // is not going to need to be propapagated anymore
                    _clients[invitee].ReceiveInvitation(proposal, _knownClientUrls.Count);
                }
            }
            // case with lots of invitees: send first directly and then
            // those to deliver the rest of the messages
            else
            {
                List<string> inviteesLeft = new List<string>(
                        invitees.GetRange(threshold, invitees.Count - threshold));

                // calculates how many clients must each client spread forward
                int nInviteesForEach = (inviteesLeft.Count) / threshold;
                int remainder = inviteesLeft.Count % threshold;

                int i = 0;
                foreach (string invitee in invitees.GetRange(0, threshold))
                {
                    if (i == 0)
                    {
                        _clients[invitee].ReceiveInvitation(proposal, _knownClientUrls.Count,
                            inviteesLeft.GetRange(0, nInviteesForEach + remainder));
                        /*foreach (string name in inviteesLeft.GetRange(0, nInviteesForEach + remainder))
                        {
                            Console.WriteLine("first case {0}", name);
                        } */                  
                    }
                    else
                    {
                        _clients[invitee].ReceiveInvitation(proposal, _knownClientUrls.Count,
                                inviteesLeft.GetRange(nInviteesForEach * i + remainder, nInviteesForEach));
                        /*foreach (string name in inviteesLeft.GetRange(nInviteesForEach * i + remainder, nInviteesForEach))
                        {
                            Console.WriteLine("second case {0}", name);
                        }*/
                    } 
                    i++;
                }
            }
        }

        public void ReceiveInvitation(MeetingProposal proposal, int nClients, List<string> inviteesLeft = null)
        {
            // updates clients known if it's client count is not right
            if (_knownClientUrls.Count != nClients)
            {
                bool contactedServer = false;
                try
                {
                    Parallel.Invoke(() => Timeout());
                    _knownClientUrls = _remoteServer.AskForUpdateClients();
                    contactedServer = true;
                    UpdateClients(_knownClientUrls);
                }
                catch(Exception e)
                {
                    string previousUrl = DetectServerDeath(contactedServer);
                    _knownClientUrls = _remoteServer.AskForUpdateClients();
                    UpdateClients(_knownClientUrls);
                }
            }

            if (inviteesLeft != null)
            {
                SendInvitations(inviteesLeft, proposal);
            }
            _knownMeetingProposals.Add(proposal.Topic, proposal);
            Console.WriteLine("Received proposal with topic: {0}", proposal.Topic);
        }

        // slots -> Lisboa,2019-11-14 Porto,2020-02-03
        public List<DateLocation> ParseSlots(List<string> slots)
        {
            List<DateLocation> parsedSlots = new List<DateLocation>();
            foreach (string slot in slots)
            {
                // local and date are separated by a comma
                List<string> slotDetail = slot.Split(',').ToList();
                parsedSlots.Add(new DateLocation(slotDetail[0], slotDetail[1]));
            }

            return parsedSlots;
        }
    }
}
