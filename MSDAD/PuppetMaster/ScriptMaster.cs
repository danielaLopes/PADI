using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using ClassLibrary;
using Server;

namespace PuppetMaster
{
    public class ScriptMaster : MasterAPI
    {
        public void ReceiveCommand(string command)

        {
            List<string> fields = command.Split().ToList();
            string strFields = command.Remove(0, fields[0].Length);

            if (fields[0].Equals("Server"))
            {
                Server(strFields, fields[1], fields[2]);
            }
            else if (fields[0].Equals("Client"))
            {
                Console.WriteLine(fields[2]);
                Client(strFields, fields[1], fields[2]);
            }
            else if (fields[0].Equals("AddRoom"))
            {
                AddRoom(fields);
            }
            else if (fields[0].Equals("Status"))
            {
                Status(strFields);
            }
            else if (fields[0].Equals("Crash"))
            {
                Crash(strFields);
            }
            else if (fields[0].Equals("Freeze"))
            {
                Freeze(strFields);
            }
            else if (fields[0].Equals("Unfreeze"))
            {
                Unfreeze(strFields);
            }
            else if (fields[0].Equals("Wait"))
            {
                Wait(strFields);
            }
        }

        public void ShareMasterInfo()
        {
            List<IServer> remoteServers = Servers.Values.ToList();
            foreach (IServer server in remoteServers)
            {
                server.GetMasterUpdateServers(ServerUrls);
                // TODO TEMPORARY
                server.GetMasterUpdateClients(ClientUrls);
                server.GetMasterUpdateLocations(Locations);
            }

            // TODO DECIDE WHAT CLIENTS THE CLIENT RECEIVES
            List<IClient> remoteClients = Clients.Values.ToList();
            foreach (IClient client in remoteClients)
            {
                client.GetMasterUpdateClients(ClientUrls);
            }
        }



        static void Main(string[] args)
        {
            ScriptMaster scriptMaster = new ScriptMaster();

            string configFilePath = "C:../../config.txt";

            string[] lines = System.IO.File.ReadAllLines(@configFilePath);

            foreach (string line in lines)
            {
                scriptMaster.ReceiveCommand(line);
            }
            scriptMaster.ShareMasterInfo();

            Console.ReadLine();
        }
    }
}
