using System;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Collections.Generic;
using System.Linq;
using ClassLibrary;
using System.Collections;

namespace Server
{
    //public delegate void UpdateMessagesDelegate(IClient remoteClient, string nickname, string message);

    public class CServer : MarshalByRefObject, IServer
    {     
        private Hashtable _currentMeetingProposals;

        private List<IServer> _servers;

        private Dictionary<string, Location> _locations;

        private Dictionary<string, IClient> _clients;

        private readonly string SERVER_ID;
        private readonly string SERVER_URL;

        public CServer(string serverId, string url, int maxFaults, int minDelay, int maxDelay, List<string> serverUrls = null)
        {
            SERVER_ID = serverId;
            SERVER_URL = url;
   
            TcpChannel serverChannel = new TcpChannel(PortExtractor.Extract(SERVER_URL));
            ChannelServices.RegisterChannel(serverChannel, false);

            // creates the server's remote object
            RemotingServices.Marshal(this, SERVER_ID, typeof(IServer));

            _currentMeetingProposals = new Hashtable();

            _clients = new Dictionary<string, IClient>();

            _servers = new List<IServer>();
            // gets other server's remote objects and saves them
            if (serverUrls != null)
            {
                GetMasterUpdateServers(serverUrls);
            }
            // else : the puppet master invokes GetMasterUpdateServers method
        }

        public void RegisterUser(string username, string clientUrl)
        {
            // obtain client remote object
            _clients.Add(username, (IClient)Activator.GetObject(typeof(IClient), clientUrl));

            Console.WriteLine("New user " + username  + " with url " + clientUrl + " registered.");
        }

        public void Create(MeetingProposal proposal)
        {
            _currentMeetingProposals.Add(proposal.Topic, proposal);
            Console.WriteLine("Created new meeting proposal for " + proposal.Topic + ".");
        }

        public void Join(string topic, MeetingRecord record)
        {
            MeetingProposal proposal = (MeetingProposal) _currentMeetingProposals[topic];
            proposal.Records.Add(record);
            Console.WriteLine(record.Name + " joined meeting proposal " + proposal.Topic + ".");
        }

        public void GetMasterUpdateServers(List<string> serverUrls)
        {
            foreach(string url in serverUrls)
            {
                _servers.Add((IServer)Activator.GetObject(typeof(IServer), url));
            }
        }

        public void GetMasterUpdateLocations(Dictionary<string, Location> locations)
        {
            _locations = locations;
        }

        public void Status()
        {

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