using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using ClassLibrary;

namespace PuppetMaster
{
    public class MasterAPI
    {
        private Hashtable _servers;
        private Hashtable _clients;
        private Hashtable _locations;

        public MasterAPI()
        {
            _servers = new Hashtable();
            _clients = new Hashtable();
            _locations = new Hashtable();
        }

        // Server server id URL max faults min delay max delay
        // serverId <=> location
        public void Server(string fields, string serverId, string url)
        {
            Process.Start(@"..\..\..\Server\bin\Debug\Server.exe", fields);
            _servers.Add(serverId, (IServer)Activator.GetObject(typeof(IServer), url));
        }

        // Client username client URL server URL script file
        public void Client(string fields, string username, string url)
        {
            Process.Start(@"..\..\..\Client\bin\Debug\Client.exe", fields);
            _servers.Add(username, (IServer)Activator.GetObject(typeof(IServer), url));
        }

        // AddRoom location capacity room name
        public void AddRoom(List<string> fields)
        {
            string locationName = fields[1];
            int capacity = Int32.Parse(fields[2]);
            string roomName = fields[3];

            Location location;
            if(_locations.ContainsKey(locationName))
            {
                location = (Location)_locations[locationName];
            }
            else
            {
                location = new Location(locationName);
            }
            location.AddRoom(new Room(roomName, capacity));

            Console.WriteLine(location);
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
