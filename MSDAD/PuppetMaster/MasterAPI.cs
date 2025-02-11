﻿using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Sockets;
using System.Threading;
using ClassLibrary;
using PCS;

namespace PuppetMaster
{
    delegate void StartProcessDelegate(string pcsBaseUrl, string fields, string exePath, string id);

    delegate void ServerDelegate(string fields, string serverId, string url);
    delegate void ClientDelegate(string fields, string username, string url, string serverUrl);
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

        /// <summary>
        /// key->serverId value->remoteObject
        /// </summary>
        public ConcurrentDictionary<string, IServer> Servers { get; set; }
        public string ServerUrls { get; set; }
        /// <summary>
        /// key->serverId value->serverUrl
        /// </summary>
        public ConcurrentDictionary<string, string> ServerIdsToUrls { get; set; }
        public int MaxFaults;

        public ConcurrentDictionary<string, IClient> Clients { get; set; }

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
        private ShutDownSystemDelegate _shutDownSystemDelegate;

        private CheckNodeStatus _checkNodeStatusDelegate;

        private int _nodesCreated;

        public MasterAPI(string[] pcsUrls)
        {
            Servers = new ConcurrentDictionary<string, IServer>();
            ServerUrls = "";
            ServerIdsToUrls = new ConcurrentDictionary<string, string>();

            Clients = new ConcurrentDictionary<string, IClient>();

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
            _shutDownSystemDelegate = new ShutDownSystemDelegate(ShutDownSystemSync);

            _checkNodeStatusDelegate = new CheckNodeStatus(CheckNode);

            _nodesCreated = 0;
        }

        public IAsyncResult Server(string fields, string serverId, string url, int maxFaults)
        {
            _nodesCreated++;
            MaxFaults = maxFaults;
            return _serverDelegate.BeginInvoke(fields, serverId, url, null, null);
        }

        public IAsyncResult Client(string fields, string username, string url, string serverUrl)
        {
            _nodesCreated++;
            return _clientDelegate.BeginInvoke(fields, username, url, serverUrl,null, null);
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
            // wait is the only task supposed to be executed synchronously
            Console.WriteLine("Goingo to wait {0} miliseconds", fields);
            Thread.Sleep(Int32.Parse(fields));
            Console.WriteLine("finished waiting");
        }

        public void ShutDownSystem()
        {
            _shutDownSystemDelegate.BeginInvoke(null, null);
        }

        public void ServerSync(string fields, string serverId, string url)
        {
            fields += " " + LOCATIONS_PCS_PATH;

            // sends server pre-existing servers' urls as part of the arguments
            IAsyncResult result;

            lock (ServerUrls)
            {
                // sends server pre-existing servers' urls as part of the arguments
                result = _startProcessDelegate.BeginInvoke(url,
                        fields + " " + Servers.Count.ToString() + " " + ServerUrls,
                        @"..\..\..\Server\bin\Debug\Server.exe", serverId, null, null);
                Servers.TryAdd(serverId, (IServer)Activator.GetObject(typeof(IServer), url));
                ServerUrls += " " + url;
            }
            ServerIdsToUrls.TryAdd(serverId, url);
            result.AsyncWaitHandle.WaitOne();
 
            Console.WriteLine("Server {0} created!", serverId);
        }

        // Client username client URL server URL script file
        public void ClientSync(string fields, string username, string url, string serverUrl)
        {
            IAsyncResult result = _startProcessDelegate.BeginInvoke(url,
                    fields + " " + MaxFaults + " " + ChooseBackupServers(serverUrl), 
                    @"..\..\..\Client\bin\Debug\Client.exe", username, null, null);
               
            Clients.TryAdd(username, (IClient)Activator.GetObject(typeof(IClient), url));

            result.AsyncWaitHandle.WaitOne();

            Console.WriteLine("Client {0} created!", username);
        }

        /// <summary>
        /// Chooses f servers in the system to attribute as backup
        /// server to a client, in case of client's server failing
        /// </summary>
        /// <param name="urlMainServer"></param>
        /// <returns> a string with the urls of f backup servers </returns>
        public string ChooseBackupServers(string urlMainServer)
        {
            string backupUrls = "";
            if (MaxFaults > 0) {
                List<string> serverUrlList = new List<string>(ServerIdsToUrls.Values);

                Random randomizer = new Random();
                int randomStartIndex = randomizer.Next(serverUrlList.Count);

                int nBackups = 0;
                for (int i = randomStartIndex; i < serverUrlList.Count; i++)
                {
                    if (!urlMainServer.Equals(serverUrlList[i]))
                    {
                        backupUrls += serverUrlList[i] + " ";
                        nBackups++;
                    }
                    if (nBackups == MaxFaults) return backupUrls;
                }
                for (int i = 0; i < randomStartIndex; i++)
                {
                    if (!urlMainServer.Equals(serverUrlList[i]))
                    {
                        backupUrls += serverUrlList[i] + " ";
                        nBackups++;
                        Console.WriteLine("backupUrls {0}", backupUrls);
                    }
                    if (nBackups == MaxFaults) return backupUrls;
                }
            }     
            return backupUrls;
        }

        public void StartProcess(string url, string fields, string exePath, string id)
        {
            // match right PCS
            string basePcsUrl = BaseUrlExtractor.Extract(url);
            ProcessCreationService pcs = PCSs[basePcsUrl];
            pcs.Start(@exePath, fields, id);
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
            // we only want to check nodes after they are created
            while((Servers.Count + Clients.Count) < _nodesCreated)
            {
                Thread.Sleep(500);
            }

            WaitHandle[] handles = new WaitHandle[_nodesCreated];

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

            WaitHandle.WaitAll(handles, 10000);
        }

        private void CheckNode(ISystemNode node)
        {
            try {
                node.Status();
            }
            catch(SocketException e)
            {
                Console.WriteLine("Some nodes are down");
                Console.WriteLine(e.Message);
            }
            
        }

        private void CheckNodeCallback(IAsyncResult res)
        {
            Console.WriteLine("received response");
            _checkNodeStatusDelegate.EndInvoke(res);
        }

        // Debugging Commands


        // Crash server_id
        public void CrashSync(string fields)
        {
            // match right PCS
            string basePcsUrl = BaseUrlExtractor.Extract(ServerIdsToUrls[fields]);
            ProcessCreationService pcs = PCSs[basePcsUrl];
            pcs.Crash(fields);
        }

        // Freeze server id
        public void FreezeSync(string fields)
        {
            Servers[fields].Freeze();
        }

        // Unfreeze server id
        public void UnfreezeSync(string fields)
        {
            Console.WriteLine("before unfreeze");
            Servers[fields].Unfreeze();
            Console.WriteLine("after unfreeze");
        }

        public void ShutDownSystemSync()
        {
            foreach (KeyValuePair<string, ProcessCreationService> pcs in PCSs)
                pcs.Value.ShutDownAll();

            Environment.Exit(0);
        }
    }
}
