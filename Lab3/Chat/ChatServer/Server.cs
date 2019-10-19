using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Text;
using System.Threading.Tasks;
using CommonLibrary;

namespace ChatServer
{
    public class Server
    {
        private TcpChannel channel;

        public Server()
        {
            this.channel = new TcpChannel(1234);
            ChannelServices.RegisterChannel(this.channel, false);

            RemotingConfiguration.RegisterWellKnownServiceType(typeof(CServerService), "ChatServer", WellKnownObjectMode.Singleton);
        }

    }
}
