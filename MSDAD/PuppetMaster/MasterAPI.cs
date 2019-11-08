using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using ClassLibrary;
using PCS;

namespace PuppetMaster
{
    delegate void StartProcessDelegate(string pcsBaseUrl, string fields, string exePath);

    delegate void ServerDelegate(string fields, string serverId, string url);
    delegate void ClientDelegate(string fields, string username, string url);
    delegate void AddRoomDelegate(List<string> fields);
    delegate void StatusDelegate();
    delegate void CrashDelegate(string fields);
    delegate void FreezeDelegate(string fields);
    delegate void UnfreezeDelegate(string fields);
    delegate void WaitDelegate(string fields);
    delegate void ShutDownSystemDelegate();

    delegate void CheckNodeStatus(ISystemNode node);

    public class MasterAPI
    {
        private const int PCS_PORT = 10000;
        private const string PCS_NAME = "pcs";

        public ConcurrentDictionary<string, IServer> Servers { get; set; }
        public ConcurrentBag<string> ServerUrls { get; set; }

        public ConcurrentDictionary<string, IClient> Clients { get; set; }
        public ConcurrentBag<string> ClientUrls { get; set; }

        /// <summary>
        /// string->base url of pcs to match with server/client's urls
        /// ProcessCreationService->pcs's remote object
        /// </summary>
        public ConcurrentDictionary<string, ProcessCreationService> PCSs { get; set; }

        public List<string> _locationsText = new List<string>();
        public const string LOCATIONS_PCS_PATH = @"..\..\..\Server\rooMConfig.txt";

        private StartProcessDelegate _startProcessDelegate;

        private ServerDelegate _serverDelegate;
        private ClientDelegate _clientDelegate;
        private AddRoomDelegate _addRoomDelegate;
        private StatusDelegate _statusDelegate;
        private CrashDelegate _crashDelegate;
        private FreezeDelegate _freezeDelegate;
        private UnfreezeDelegate _unfreezeDelegate;
        private WaitDelegate _waitDelegate;
        private ShutDownSystemDelegate _shutDownSystemDelegate;

        private CheckNodeStatus _checkNodeStatusDelegate;
        private AsyncCallback _checkNodeCallbackDelegate;

        public MasterAPI(string[] pcsUrls)
        {
            Servers = new ConcurrentDictionary<string, IServer>();
            ServerUrls = new ConcurrentBag<string>();

            Clients = new ConcurrentDictionary<string, IClient>();
            ClientUrls = new ConcurrentBag<string>();

            PCSs = new ConcurrentDictionary<string, ProcessCreationService>();
            foreach (string url in pcsUrls) {
                PCSs.TryAdd(BaseUrlExtractor.Extract(url), (ProcessCreationService)Activator.GetObject(typeof(ProcessCreationService), url));
            }

            _startProcessDelegate = new StartProcessDelegate(StartProcess);

            _serverDelegate = new ServerDelegate(ServerSync);
            _clientDelegate = new ClientDelegate(ClientSync);
            _addRoomDelegate = new AddRoomDelegate(AddRoomSync);
            _statusDelegate = new StatusDelegate(StatusSync);
            _crashDelegate = new CrashDelegate(CrashSync);
            _freezeDelegate = new FreezeDelegate(FreezeSync);
            _unfreezeDelegate = new UnfreezeDelegate(UnfreezeSync);
            _waitDelegate = new WaitDelegate(WaitSync);
            _shutDownSystemDelegate = new ShutDownSystemDelegate(ShutDownSystemSync);

            _checkNodeStatusDelegate = new CheckNodeStatus(CheckNode);
            _checkNodeCallbackDelegate = new AsyncCallback(CheckNodeCallback);
        }

        public IAsyncResult Server(string fields, string serverId, string url)
        {
            return _serverDelegate.BeginInvoke(fields, serverId, url, null, null);
        }

        public IAsyncResult Client(string fields, string username, string url)
        {
            return _clientDelegate.BeginInvoke(fields, username, url, null, null);
        }

        public IAsyncResult AddRoom(List<string> fields)
        {
            return _addRoomDelegate.BeginInvoke(fields, null, null);
        }

        public void Status()
        {
            _statusDelegate.BeginInvoke(null, null);
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
            _waitDelegate.BeginInvoke(fields, null, null);
        }

        public void ShutDownSystem()
        {
            _shutDownSystemDelegate.BeginInvoke(null, null);
        }

        // Server server id URL max faults min delay max delay
        // serverId <=> location
        public void ServerSync(string fields, string serverId, string url)
        {
            fields += " " + LOCATIONS_PCS_PATH;
            // sends server pre-existing servers' urls as part of the arguments
            /*if (ServerUrls.Count > 0)
            {
                fields += " " + ServerUrls.Count.ToString();
                foreach (string serverUrl in ServerUrls)
                {
                    fields += " " + serverUrl;
                }
            }

            Console.WriteLine("Server args {0}", fields);*/

            IAsyncResult result = _startProcessDelegate.BeginInvoke(url, fields, @"..\..\..\Server\bin\Debug\Server.exe", null, null);
            result.AsyncWaitHandle.WaitOne();

            UpdateServerInfo(url);

            // TODO SEE IF TRYADD IS PROBLEMATIC
            if (Servers.TryAdd(serverId, (IServer)Activator.GetObject(typeof(IServer), url)))
            {
                Console.WriteLine("Successfully added Server {0}", url);
            }
            else
            {
                Console.WriteLine("Could not add Server {0}", url);
            }
            ServerUrls.Add(url); 

            Console.WriteLine("Server {0} created!", serverId);
        }

        // Client username client URL server URL script file
        public void ClientSync(string fields, string username, string url)
        {
            // TODO client still does not receive other clients
            IAsyncResult result = _startProcessDelegate.BeginInvoke(url, fields, @"..\..\..\Client\bin\Debug\Client.exe", null, null);
            result.AsyncWaitHandle.WaitOne();

            UpdateClientInfo(url);

            // TODO SEE IF TRYADD IS PROBLEMATIC
            Clients.TryAdd(username, (IClient)Activator.GetObject(typeof(IClient), url));
            ClientUrls.Add(url);

            Console.WriteLine("Client {0} created!", username);
        }

        public void StartProcess(string url, string fields, string exePath)
        {
            // match right PCS
            string basePcsUrl = BaseUrlExtractor.Extract(url);
            ProcessCreationService pcs = PCSs[basePcsUrl];
            pcs.Start(@exePath, fields);
        }

        public void UpdateServerInfo(string newServerUrl)
        {
            foreach (KeyValuePair<string, IServer> server in Servers)
            {
                server.Value.UpdateServerAndSpread(newServerUrl);
            }
        }

        public void UpdateClientInfo(string newClientUrl)
        {
            foreach (KeyValuePair<string, IServer> server in Servers)
            {
                server.Value.UpdateClient(newClientUrl);
            }
        }

        public void SpreadLocationsFile()
        {
            foreach (KeyValuePair<string, ProcessCreationService> pcs in PCSs) {
                pcs.Value.RoomsConfigFile(LOCATIONS_PCS_PATH, _locationsText);
            }
        }

        // AddRoom location capacity room name
        public void AddRoomSync(List<string> fields)
        {
            string locationName = fields[1];
            int capacity = Int32.Parse(fields[2]);
            string roomName = fields[3];

            lock(_locationsText)
            {
                _locationsText.Add(fields[1] + " " + fields[2] + " " + fields[3]);
            }

            Console.WriteLine("Room {0} created!", roomName);
        }

            // Status
        public void StatusSync()
        {
            WaitHandle[] handles = new WaitHandle[Servers.Count + Clients.Count];

            lock (Servers)
            {
                lock (Clients)
                {
                 
                    int i = 0;
                    foreach (KeyValuePair<string, IServer> server in Servers)
                    {
                        Console.WriteLine("check status of: {0}", server.Key);
                        handles[i] = _checkNodeStatusDelegate.BeginInvoke(server.Value, CheckNodeCallback, null).AsyncWaitHandle;
                        i++;
                    }

                    foreach (KeyValuePair<string, IClient> client in Clients)
                    {
                        Console.WriteLine("check status of: {0}", client.Key);
                        handles[i] = _checkNodeStatusDelegate.BeginInvoke(client.Value, CheckNodeCallback, null).AsyncWaitHandle;
                        i++;
                    }
                }

            }

            WaitHandle.WaitAll(handles, 10000);
        }

        private void CheckNode(ISystemNode node)
        {
            node.Status();
        }

        private void CheckNodeCallback(IAsyncResult res)
        {
            Console.WriteLine("received response");
            _checkNodeStatusDelegate.EndInvoke(res);
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

        public void WaitSync(string fields)
        {
            Thread.Sleep(Int32.Parse(fields));
        }

        public void ShutDownSystemSync()
        {

        }
    }
}
