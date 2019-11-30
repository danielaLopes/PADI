using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Threading;
using ClassLibrary;

namespace Client
{
    public class CClient : MarshalByRefObject, IClient, ILocalClient
    {
        private readonly string USERNAME;
        private readonly string CLIENT_URL;

        private Dictionary<string, IClient> _clients;
        
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

            Console.WriteLine("Client registered with username {0} with url {0}.", username, clientUrl);

            RegisterNewServer(serverUrl);
            _backupServerUrl = backupServer;

            _clients = new Dictionary<string, IClient>();

            _knownMeetingProposals = new Dictionary<string, MeetingProposal>();

            // gets other client's remote objects and saves them
            if (clientsUrls != null)
            {
                UpdateClients(clientsUrls);
            }
            // else : the puppet master invokes GetMasterUpdateClients method
        }

        public void RegisterNewServer(string serverUrl)
        {
            // retrieve server's proxy
            _remoteServer = (IServer)Activator.GetObject(typeof(IServer), serverUrl);
            // register new user in remote server
            _remoteServer.RegisterUser(USERNAME, CLIENT_URL);
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

        public void UpdateClients(List<string> clientsUrls)
        {
            foreach (string url in clientsUrls)
            {
                string name = url.Split('/')[3];
                Console.WriteLine("client name: {0}", name);
                UpdateClient(name, url);
            }
        }

        public void UpdateClient(string name, string clientUrl)
        {
            Console.WriteLine("Updating client {0}", clientUrl);

            IClient client = RegisterClient(name, clientUrl);

            client.RegisterClient(USERNAME, CLIENT_URL);
        }

        public IClient RegisterClient(string name, string clientUrl)
        {
            Console.WriteLine("Registering client {0}", clientUrl);

            IClient client = (IClient)Activator.GetObject(typeof(IServer), clientUrl);
            if (!_clients.ContainsKey(name)) _clients.Add(name, client);

            return client;
        }

        public void Status()
        {
            Console.WriteLine("Client is active. URL: {0}", CLIENT_URL);
        }

        public void ShutDown()
        {

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
