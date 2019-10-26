using System;
using System.Collections;
using System.Diagnostics;
using ClassLibrary;

namespace PuppetMaster
{
    public class MasterAPI
    {
        private Hashtable _servers;
        private Hashtable _clients;

        public MasterAPI()
        {
            _servers = new Hashtable();
            _clients = new Hashtable();
        }

        // Server server id URL max faults min delay max delay
        public void Server(string fields, string serverId, string url)
        {
            Process.Start(@"..\..\..\Server\bin\Debug\Server.exe", fields);
            _servers.Add(Int32.Parse(serverId), (IServer)Activator.GetObject(typeof(IServer), url));
        }

        // Client username client URL server URL script file
        public void Client(string fields)
        {
            Process.Start(@"..\..\..\Client\bin\Debug\Client.exe", fields);
        }

        // AddRoom location capacity room name
        public void AddRoom(string fields)
        {

        }

        // Status
        public void Status(string fields)
        {

        }

        // Debugging Commands


        // Crash server id
        public void Crash(string fields)
        {

        }

        // Freeze server id
        public void Freeze(string fields)
        {

        }

        // Unfreeze server id
        public void Unfreeze(string fields)
        {

        }

        // Wait x mss
        public void Wait(string fields)
        {

        }

        public void ShutDownSystem()
        {

        }
    }
}
