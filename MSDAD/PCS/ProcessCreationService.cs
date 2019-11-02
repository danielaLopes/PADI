using System;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;

namespace PCS
{
    class ProcessCreationService : MarshalByRefObject
    {
        private const int PCS_PORT = 10000;
        private readonly string PCS_NAME;
        private readonly string PCS_URL;

        public ProcessCreationService(string pcsName, string pcsUrl)
        {
            PCS_NAME = pcsName;
            PCS_URL = pcsUrl;
            // creates TCP channel
            TcpChannel channel = new TcpChannel(PCS_PORT);
            ChannelServices.RegisterChannel(channel, false);
            // create the PCS's remote object
            RemotingServices.Marshal(this, PCS_NAME, typeof(ProcessCreationService));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="args">
        ///     args[0]->pcsName
        ///     args[1]->pcsUrl
        /// </param>
        static void Main(string[] args)
        {
            new ProcessCreationService(args[0], args[1]);

            System.Console.WriteLine("<enter> para sair...");
            System.Console.ReadLine();
        }
    }
}
