using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using ClassLibrary;

namespace PuppetMaster
{
    public class ScriptMaster : MasterAPI
    {
        public List<WaitHandle> WaitLocationHandles { get; set; }

        // to know when rooms are all created
        bool SendLocationsInfo = true;

        public ScriptMaster(string[] pcsUrls) : base(pcsUrls) {
            WaitLocationHandles = new List<WaitHandle>();
        }

        public void ReceiveCommand(string command)
        {
            List<string> fields = command.Split().ToList();
            string strFields = command.Remove(0, fields[0].Length);

            if (fields[0].Equals("AddRoom"))
            {
                WaitLocationHandles.Add(AddRoom(fields).AsyncWaitHandle);
            }
            else
            {
                if (fields[0].Equals("Server"))
                {
                    Server(strFields, fields[1], fields[2]);
                }
                else if (fields[0].Equals("Client"))
                {
                    Client(strFields, fields[1], fields[2]);
                }
                else if (command.Equals("Status"))
                {
                    Status();
                }
                else if (command.Equals("Crash"))
                {
                    Crash(strFields);
                }
                else if (command.Equals("Freeze"))
                {
                    Freeze(strFields);
                }
                else if (command.Equals("Unfreeze"))
                {
                    Unfreeze(strFields);
                }
                else if (command.Equals("Wait"))
                {
                    Wait(strFields);
                }

                if (SendLocationsInfo == true)
                {
                    SendLocationsInfo = false;

                    // to garantee locations are all added before creating servers, which will 
                    // have a file with the locations when they are created
                    if (WaitLocationHandles.Count > 0)
                    {
                        WaitHandle.WaitAll(WaitLocationHandles.ToArray());
                    }
                }
            }
        }

        public void PrintGUI()
        {
            Console.WriteLine("");
            Console.WriteLine("        PUPPETMASTER commands:");
            Console.WriteLine("        Status");
            Console.WriteLine("        Crash [server_id]");
            Console.WriteLine("        Freeze [server_id]");
            Console.WriteLine("        Unfreeze [server_id]");
            Console.WriteLine("        Wait [x_ms]");
            Console.WriteLine("        Exit");
        }

        public void ShareMasterInfo()
        {
            Console.WriteLine("sharing master info");

            List<string> serverUrlsToSend = ServerUrls.ToList();
            List<string> clientUrlsToSend = ClientUrls.ToList();

            foreach (KeyValuePair<string, IServer> server in Servers)
            {
                server.Value.GetMasterUpdateServers(serverUrlsToSend);

                // TODO TEMPORARY
                if (Clients.Count() > 0)
                {
                    server.Value.GetMasterUpdateClients(clientUrlsToSend);
                }
            }

            // TODO DECIDE WHAT CLIENTS THE CLIENT RECEIVES
            foreach (KeyValuePair<string, IClient> client in Clients)
            {
                client.Value.GetMasterUpdateClients(clientUrlsToSend);
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

            scriptMaster.PrintGUI();
            // accepts command-line commands
            string command = "";
            while(!command.Equals("Exit"))
            {
                command = Console.ReadLine();
                
                scriptMaster.ReceiveCommand(command);
            }

           
        }
    }
}
