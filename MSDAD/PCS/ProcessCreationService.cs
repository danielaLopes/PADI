using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Collections.Concurrent;

namespace PCS
{
    public class ProcessCreationService : MarshalByRefObject
    {
        private const int PCS_PORT = 10000;
        private const string PCS_NAME = "pcs";
        /// <summary>
        /// key->processId value->processId
        /// </summary>
        private ConcurrentDictionary<string, int> _processes;

        public ProcessCreationService()
        {
            // creates TCP channel
            TcpChannel channel = new TcpChannel(PCS_PORT);
            ChannelServices.RegisterChannel(channel, false);
            // create the PCS's remote object
            RemotingServices.Marshal(this, PCS_NAME, typeof(ProcessCreationService));
            _processes = new ConcurrentDictionary<string, int>();
        }

        public void Start(string path, string fields, string id)
        {
            Process newProcess = Process.Start(@path, fields);
            _processes.TryAdd(id, newProcess.Id);
        }

        public void ShutDownAll()
        {
            foreach(int processId in _processes.Values)
            {
                try
                {
                    Process process = Process.GetProcessById(processId);
                    process.CloseMainWindow();
                    process.Close();
                }
                catch(ArgumentException e)
                {
                    Console.WriteLine("Process with id: {0} not found.", processId);
                }
               
              
            }
            Console.WriteLine("closed all processes");
        }

        public void RoomsConfigFile(string path, List<string> locations)
        {
            Console.WriteLine("Writing into Rooms config file");
            using (StreamWriter file = new StreamWriter(@path))
            {
                foreach (string line in locations)
                {
                    file.WriteLine(line);
                }
            }
        }

        public void Crash(string serverId)
        {
            Console.WriteLine("Crashing server {0}", serverId);
            Process process = Process.GetProcessById(_processes[serverId]);
            process.CloseMainWindow();
            process.Close();
        }

        static void Main(string[] args)
        {
            new ProcessCreationService();

            System.Console.WriteLine("<enter> para sair...");
            System.Console.ReadLine();
        }
    }
   
}
