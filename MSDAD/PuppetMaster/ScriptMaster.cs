using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using ClassLibrary;

namespace PuppetMaster
{
    public class ScriptMaster : MasterAPI
    {
        public List<WaitHandle> WaitHandles{ get; set; }

        public ScriptMaster() : base() {
            WaitHandles = new List<WaitHandle>();
        }

        public void ReceiveCommand(string command)
        {
            List<string> fields = command.Split().ToList();
            string strFields = command.Remove(0, fields[0].Length);

            if (fields[0].Equals("Server"))
            {
                WaitHandles.Add(Server(strFields, fields[1], fields[2]).AsyncWaitHandle);
            }
            else if (fields[0].Equals("Client"))
            {
                WaitHandles.Add(Client(strFields, fields[1], fields[2]).AsyncWaitHandle);
            }
            else if (fields[0].Equals("AddRoom"))
            {
                WaitHandles.Add(AddRoom(fields).AsyncWaitHandle);
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
                //Console.WriteLine("ola");
                //Console.WriteLine(Locations["Lisboa"]);
                //server.GetMasterUpdateLocations(Locations);
                Console.WriteLine("after updating server");
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

            string[] lines = System.IO.File.ReadAllLines(configFilePath);

            Console.WriteLine("Press s for executing commands sequentially and n for executing step by step");
            string mode = Console.ReadLine();
            // sequentially
            if (mode.Equals("s"))
            {
                foreach (string line in lines)
                {
                    scriptMaster.ReceiveCommand(line);
                }
            }
            // step by step
            else if (mode.Equals("n"))
            {
                foreach (string line in lines)
                {
                    scriptMaster.ReceiveCommand(line);
                    Console.WriteLine("Enter for next command");
                    Console.ReadLine(); 
                }
            }
            
            // waits for all servers, clients and rooms to be added before sharing information
            WaitHandle.WaitAll(scriptMaster.WaitHandles.ToArray());
            scriptMaster.ShareMasterInfo();

            Console.ReadLine();
        }
    }
}
