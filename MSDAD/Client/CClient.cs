using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Threading;
using System.Collections.Concurrent;
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

        private string _backupServerUrl;

        /// <summary>
        /// Creates TCP channel, saves relevant information for remoting, registers itself as
        /// remote object and gets preferred server's remote object.
        /// </summary>
        /// <param name="username"></param>
        /// <param name="clientUrl"></param>
        /// <param name="serverUrl"></param>
        public CClient(string username, string clientUrl, string serverUrl, string backupServer, List<string> clientsUrls = null)
        {
            USERNAME = username;
            CLIENT_URL = clientUrl;
            // creates TCP channel
            TcpChannel clientChannel = new TcpChannel(PortExtractor.Extract(CLIENT_URL));
            ChannelServices.RegisterChannel(clientChannel, false);
            // create the client's remote object
            RemotingServices.Marshal(this, username, typeof(CClient));

            Console.WriteLine("Client registered with username {0} with url {1}.", username, clientUrl);

            RegisterNewServer(serverUrl);
            Console.WriteLine("Backup server assigned {0}", backupServer);
            _backupServerUrl = backupServer;

            _clients = new ConcurrentDictionary<string, IClient>();
            _knownClientUrls = new List<string>();

            _knownMeetingProposals = new Dictionary<string, MeetingProposal>();

            // gets other client's remote objects and saves them
            if (clientsUrls != null)
            {
                UpdateClients(clientsUrls);  
            }
        }

        public void RegisterNewServer(string serverUrl)
        {
            // retrieve server's proxy
            _remoteServer = (IServer)Activator.GetObject(typeof(IServer), serverUrl);
            // register new user in remote server
            _remoteServer.RegisterUser(USERNAME, CLIENT_URL);
            Console.WriteLine("Registered with server {0}", serverUrl);
        }

        public void List()
        {
            _remoteServer.List(USERNAME, _knownMeetingProposals);
            foreach (KeyValuePair<string, MeetingProposal> meetingProposal in _knownMeetingProposals)
            {
                MeetingProposal proposal = meetingProposal.Value;
                Console.WriteLine(proposal);
            }
        }

        public void Create(string meetingTopic, string minAttendees, List<string> slots, List<string> invitees = null)
        {
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
            _remoteServer.Create(proposal);

            _knownMeetingProposals.Add(proposal.Topic, proposal);

            // TODO
            if (invitees != null)
            {
                // send to all invitees
            }
            else
            {
                Console.WriteLine("invitees:" + invitees);
                // send to every client
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
                _remoteServer.Join(USERNAME, meetingTopic, record);
            }
        }

        public void Close(string meetingTopic)
        {
            _remoteServer.Close(meetingTopic);
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

        public void SwitchServer(string serverUrl)
        {
            RegisterNewServer(serverUrl);
        }

        /// <summary>
        /// When client is initilized, he receives a list with
        /// other clients' urls and then gets the respective
        /// remote objects
        /// </summary>
        /// <param name="clientsUrls"></param>
        public void UpdateClients(List<string> clientsUrls)
        {
            foreach (string url in clientsUrls)
            {
                string name = url.Split('/')[3];
                IClient client = RegisterClient(name, url);
                if (client != null) client.RegisterClient(USERNAME, CLIENT_URL);
            }
        }

        /// <summary>
        /// Remote method in which a client registers another client and sends 
        /// that client itself and the clients he knows to be registered
        /// </summary>
        /// <param name="name"></param>
        /// <param name="clientUrl"></param>
        /// <param name="otherClientUrls"></param>
        public void BroadcastNewClient(string name, string url)
        {
            List<KeyValuePair<string, IClient>> knownClients = new List<KeyValuePair<string, IClient>>(_clients);
            foreach (KeyValuePair<string, IClient> knownClient in knownClients)
            {   
                if (!knownClient.Key.Equals(name))
                {
                    knownClient.Value.RegisterClient(name, url);
                }
            }
        }

        public void ReceiveClientsList(List<string> urls)
        {
            foreach(string url in urls)
            {   if (!url.Equals(CLIENT_URL))
                {
                    string name = url.Split('/')[3];
                    RegisterClient(name, url);
                }            
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
            IClient client = null;
            
            if (!_clients.ContainsKey(name) && !name.Equals(USERNAME))
            {
                Console.WriteLine("new client {0}", name);
                client = (IClient)Activator.GetObject(typeof(IClient), clientUrl);
                lock(_knownClientUrls)
                {
                    client.ReceiveClientsList(_knownClientUrls);
                    _clients.TryAdd(name, client);
                    _knownClientUrls.Add(clientUrl);
                }

                BroadcastNewClient(name, clientUrl);
            }
            return client;
        }

        public void Status()
        {
            Console.WriteLine("Client is active. URL: {0}", CLIENT_URL);
        }

        public void ShutDown()
        {

        }

        public void SendInvitations(List<string> invitees, MeetingProposal proposal)
        {
            int minSpreaders = 1;
            int threshold = 10;

            List<IClient> directMessages = new List<IClient>();
            foreach (string invitee in invitees)
            {
                if (_clients.ContainsKey(invitee))
                {
                    directMessages.Add(_clients[invitee]);
                }
            }
            if (directMessages.Count >= minSpreaders)
            {
                foreach (IClient client in directMessages)
                {
                    client.ReceiveInvitation(proposal);
                }
                return;
            }

            int nSends = 0;
            foreach (KeyValuePair<string, IClient> client in _clients)
            {
                if (nSends >= threshold) return;
                client.Value.ReceiveInvitation(proposal);
                nSends++;
            }
        }

        public void ReceiveInvitation(MeetingProposal proposal)
        {    
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
