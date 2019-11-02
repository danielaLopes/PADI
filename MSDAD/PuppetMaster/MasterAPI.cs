using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using ClassLibrary;
using PCS;

namespace PuppetMaster
{
    delegate void ServerDelegate(string fields, string serverId, string url);
    delegate void ClientDelegate(string fields, string username, string url);
    delegate void AddRoomDelegate(List<string> fields);
    delegate void StatusDelegate(string fields);
    delegate void CrashDelegate(string fields);
    delegate void FreezeDelegate(string fields);
    delegate void UnfreezeDelegate(string fields);
    delegate void ShutDownSystemDelegate();

    public class MasterAPI
    {
        private const int PCS_PORT = 10000;
        private const string PCS_NAME = "pcs";

        public Dictionary<string, IServer> Servers { get; set; }
        public List<string> ServerUrls { get; set; }

        public Dictionary<string, IClient> Clients { get; set; }
        public List<string> ClientUrls { get; set; }

        public Dictionary<string, Location> Locations { get; set; }

        private ServerDelegate _serverDelegate;
        private ClientDelegate _clientDelegate;
        private AddRoomDelegate _addRoomDelegate;
        private StatusDelegate _statusDelegate;
        private CrashDelegate _crashDelegate;
        private FreezeDelegate _freezeDelegate;
        private UnfreezeDelegate _unfreezeDelegate;
        private ShutDownSystemDelegate _shutDownSystemDelegate;

        public MasterAPI()
        {
            Servers = new Dictionary<string, IServer>();
            ServerUrls = new List<string>();

            Clients = new Dictionary<string, IClient>();
            ClientUrls = new List<string>();

            Locations = new Dictionary<string, Location>();

            _serverDelegate = new ServerDelegate(ServerSync);
            _clientDelegate = new ClientDelegate(ClientSync);
            _addRoomDelegate = new AddRoomDelegate(AddRoomSync);
            _statusDelegate = new StatusDelegate(StatusSync);
            _crashDelegate = new CrashDelegate(CrashSync);
            _freezeDelegate = new FreezeDelegate(FreezeSync);
            _unfreezeDelegate = new UnfreezeDelegate(UnfreezeSync);
            _shutDownSystemDelegate = new ShutDownSystemDelegate(ShutDownSystemSync);
        }

        public void Server(string fields, string serverId, string url)
        {
            _serverDelegate.BeginInvoke(fields,serverId, url, null, null);
        }

        public void Client(string fields, string username, string url)
        {
            _clientDelegate.BeginInvoke(fields, username, url, null, null);
        }

        public void AddRoom(List<string> fields)
        {
            _addRoomDelegate.BeginInvoke(fields, null, null);
        }

        public void Status(string fields)
        {
            _statusDelegate.BeginInvoke(fields, null, null);
        }

        public void Crash(string fields)
        {
            _crashDelegate.BeginInvoke(fields, null, null);
        }

        public void Freeze(string fields)
        {
            _freezeDelegate.BeginInvoke(fields, null, null);
        }

        public void Unfreeze(string fields)
        {
            _unfreezeDelegate.BeginInvoke(fields, null, null);
        }

        // Wait x mss
        public void Wait(string fields)
        {

        }

        public void ShutDownSystem()
        {
            _shutDownSystemDelegate.BeginInvoke(null, null);
        }

        // Server server id URL max faults min delay max delay
        // serverId <=> location
        public void ServerSync(string fields, string serverId, string url)
        {
            ProcessCreationService pcs = (ProcessCreationService)Activator.GetObject(typeof(ProcessCreationService), BaseUrlExtractor.Extract(url) + PCS_PORT + "/" + PCS_NAME);
            pcs.Start(@"..\..\..\Server\bin\Debug\Server.exe", fields);
            //Process.Start(@"..\..\..\Server\bin\Debug\Server.exe", fields);
            Servers.Add(serverId, (IServer)Activator.GetObject(typeof(IServer), url));
            ServerUrls.Add(url);
        }

        // Client username client URL server URL script file
        public void ClientSync(string fields, string username, string url)
        {
            ProcessCreationService pcs = (ProcessCreationService)Activator.GetObject(typeof(ProcessCreationService), BaseUrlExtractor.Extract(url) + PCS_PORT + "/" + PCS_NAME);
            pcs.Start(@"..\..\..\Client\bin\Debug\Client.exe", fields);
            //Process.Start(@"..\..\..\Client\bin\Debug\Client.exe", fields);
            Clients.Add(username, (IClient)Activator.GetObject(typeof(IClient), url));
            ClientUrls.Add(url);
        }

        // AddRoom location capacity room name
        public void AddRoomSync(List<string> fields)
        {
            string locationName = fields[1];
            int capacity = Int32.Parse(fields[2]);
            string roomName = fields[3];

            Location location;
            if(Locations.ContainsKey(locationName))
            {
                location = (Location)Locations[locationName];
            }
            else
            {
                location = new Location(locationName);
                Locations.Add(locationName, location);
            }
            location.AddRoom(new Room(roomName, capacity));
        }

        // Status
        public void StatusSync(string fields)
        {

        }

        // Debugging Commands


        // Crash server id
        public void CrashSync(string fields)
        {

        }

        // Freeze server id
        public void FreezeSync(string fields)
        {

        }

        // Unfreeze server id
        public void UnfreezeSync(string fields)
        {

        }

        public void ShutDownSystemSync()
        {

        }
    }
}
