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
        public List<WaitHandle> WaitLocationHandles { get; set; }

        // we assume all the AddRoom commands come in the beginning
        private bool _readingLocations = true;

        public ScriptMaster(string[] pcsUrls) : base(pcsUrls) {
            WaitHandles = new List<WaitHandle>();
            WaitLocationHandles = new List<WaitHandle>();
        }

        public void ReceiveCommand(string command)
        {
            List<string> fields = command.Split().ToList();
            string strFields = command.Remove(0, fields[0].Length);

            if (_readingLocations == true)
            {
                if (fields[0].Equals("AddRoom"))
                {
                    WaitLocationHandles.Add(AddRoom(fields).AsyncWaitHandle);
                }
                else
                {
                    _readingLocations = false;
                    while (WaitLocationHandles.Count == 0) { }
                    WaitHandle.WaitAll(WaitLocationHandles.ToArray());
                }
            }
            if (_readingLocations == false)
            {
                if (fields[0].Equals("Server"))
                {
                    WaitHandles.Add(Server(strFields, fields[1], fields[2]).AsyncWaitHandle);
                }
                else if (fields[0].Equals("Client"))
                {
                    WaitHandles.Add(Client(strFields, fields[1], fields[2]).AsyncWaitHandle);
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
        }

        public void ShareMasterInfo()
        {
            Console.WriteLine("sharing master info");

            List<IServer> remoteServers = Servers.Values.ToList();
            List<IClient> remoteClients = Clients.Values.ToList();

            foreach (IServer server in remoteServers)
            {
                server.GetMasterUpdateServers(ServerUrls);

                // TODO TEMPORARY
                if (remoteClients.Count() > 0)
                {
                    server.GetMasterUpdateClients(ClientUrls);
                }
            }

            // TODO DECIDE WHAT CLIENTS THE CLIENT RECEIVES
            foreach (IClient client in remoteClients)
            {
                client.GetMasterUpdateClients(ClientUrls);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="args">
        ///     args[0]->pcsFilePath
        ///     args[1]->configFilePath
        /// </param>
        static void Main(string[] args)
        {
            //string pcsFilePath = "C:../../pcsPool.txt";
            string[] pcsUrls = System.IO.File.ReadAllLines(args[0]);

            ScriptMaster scriptMaster = new ScriptMaster(pcsUrls);

            //string configFilePath = "C:../../config.txt";

            string[] lines = System.IO.File.ReadAllLines(args[1]);

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
            while(scriptMaster.WaitHandles.Count == 0) {}
            WaitHandle.WaitAll(scriptMaster.WaitHandles.ToArray());
            scriptMaster.ShareMasterInfo();

            Console.ReadLine();
        }
    }
}
