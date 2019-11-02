using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using ClassLibrary;
using System.Linq;

namespace PuppetMaster
{
    public class MasterAPI
    {
        public Dictionary<string, IServer> Servers { get; set; }
        public List<string> ServerUrls { get; set; }

        public Dictionary<string, IClient> Clients { get; set; }
        public List<string> ClientUrls { get; set; }

        public Dictionary<string, Location> Locations { get; set; }

        public MasterAPI()
        {
            Servers = new Dictionary<string, IServer>();
            ServerUrls = new List<string>();

            Clients = new Dictionary<string, IClient>();
            ClientUrls = new List<string>();

            Locations = new Dictionary<string, Location>();
        }

        // Server server id URL max faults min delay max delay
        // serverId <=> location
        public void Server(string fields, string serverId, string url)
        {
            Process.Start(@"..\..\..\Server\bin\Debug\Server.exe", fields);
            Servers.Add(serverId, (IServer)Activator.GetObject(typeof(IServer), url));
            ServerUrls.Add(url);
        }

        // Client username client URL server URL script file
        public void Client(string fields, string username, string url)
        {
            Process.Start(@"..\..\..\Client\bin\Debug\Client.exe", fields);
            Clients.Add(username, (IClient)Activator.GetObject(typeof(IClient), url));
            ClientUrls.Add(url);
        }

        // AddRoom location capacity room name
        public void AddRoom(List<string> fields)
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
