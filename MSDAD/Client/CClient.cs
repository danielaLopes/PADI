using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using ClassLibrary;

namespace Client
{
    public class CClient : MarshalByRefObject, IClient
    {
        private readonly string USERNAME;
        private readonly string CLIENT_URL;

        // TODO save how many clients? for now we only need to have a list with clients
        private List<IClient> _clients;
        
        // saves the meeting proposal the client knows about (created or received invitation)
        public List<MeetingProposal> _knownMeetingProposals;

        // preferred server
        private readonly string SERVER_URL;
        // obtain server remote object
        private IServer _remoteServer;

        /// <summary>
        /// Creates TCP channel, saves relevant information for remoting, registers itself as
        /// remote object and gets preferred server's remote object.
        /// </summary>
        /// <param name="username"></param>
        /// <param name="clientUrl"></param>
        /// <param name="serverUrl"></param>
        public CClient(string username, string clientUrl, string serverUrl, List<string> clientsUrls = null)
        {
            USERNAME = username;
            CLIENT_URL = clientUrl;
            // creates TCP channel
            TcpChannel clientChannel = new TcpChannel(PortExtractor.Extract(CLIENT_URL));
            ChannelServices.RegisterChannel(clientChannel, false);
            // create the client's remote object
            RemotingServices.Marshal(this, username, typeof(CClient));

            Console.WriteLine("Client registered with username {0} with url {0}.", username, clientUrl);

            _clients = new List<IClient>();

            _knownMeetingProposals = new List<MeetingProposal>();

            SERVER_URL = serverUrl;
            // retrieve server's proxy
            _remoteServer = (IServer)Activator.GetObject(typeof(IServer), SERVER_URL);
            // register new user in remote server
            _remoteServer.RegisterUser(username, CLIENT_URL);

            _clients = new List<IClient>();

            // gets other client's remote objects and saves them
            if (clientsUrls != null)
            {
                GetMasterUpdateClients(clientsUrls);
            }
            // else : the puppet master invokes GetMasterUpdateClients method
        }


        public void List()
        {
            foreach(MeetingProposal proposal in _knownMeetingProposals)
            {
                Console.WriteLine(proposal);
            }
        }

        public void Create(string meetingTopic, string minAttendees, List<string> slots, List<string> invitees = null)
        {
            List<DateLocation> parsedSlots = ParseSlots(slots);
            MeetingProposal proposal = new MeetingProposal {
                Coordinator = USERNAME,
                Topic = meetingTopic,
                MinAttendees = Int32.Parse(minAttendees),
                DateLocationSlots = parsedSlots,
                Invitees = invitees,
                Records = new List<MeetingRecord>()

            };
            _remoteServer.Create(proposal);

            _knownMeetingProposals.Add(proposal);

            // TODO
            if (invitees != null)
            {
                // send to all invitees
            }
            else
            {
                // send to every client
            }

        }

        public void Join(string meetingTopic, List<string> slots)
        {
            List<DateLocation> parsedSlots = ParseSlots(slots);
            MeetingRecord record = new MeetingRecord
            {
                Name = USERNAME,
                DateLocationSlots = parsedSlots
            };
            _remoteServer.Join(meetingTopic, record);
        }

        public void Close(string meetingTopic)
        {

        }

        public void Wait(string milliseconds)
        {

        }

        public void GetMasterUpdateClients(List<string> clientsUrls)
        {
            foreach (string url in clientsUrls)
            {
                _clients.Add(((IClient)Activator.GetObject(typeof(IClient), url)));
            }
        }

        public void Status()
        {

        }

        public void ShutDown()
        {

        }

        public void ReceiveInvitation(MeetingProposal proposal)
        {    
            _knownMeetingProposals.Add(proposal);
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
