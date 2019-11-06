using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;

namespace PCS
{
    public class ProcessCreationService : MarshalByRefObject
    {
        private const int PCS_PORT = 10000;
        private const string PCS_NAME = "pcs";

        public ProcessCreationService()
        {
            // creates TCP channel
            TcpChannel channel = new TcpChannel(PCS_PORT);
            ChannelServices.RegisterChannel(channel, false);
            // create the PCS's remote object
            RemotingServices.Marshal(this, PCS_NAME, typeof(ProcessCreationService));
        }

        public void Start(string path, string fields)
        {
            Process.Start(@path, fields);
        }

        public void RoomsConfigFile(string path, List<string> locations)
        {
            if (!File.Exists(path))
            {
                using (StreamWriter file = new StreamWriter(@path))
                {
                    foreach (string line in locations)
                    {
                        file.WriteLine(line);
                    }
                }
            }
        }

        static void Main(string[] args)
        {
            new ProcessCreationService();

            System.Console.WriteLine("<enter> para sair...");
            System.Console.ReadLine();
        }
    }
}
