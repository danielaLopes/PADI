using System;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Runtime.Remoting;

namespace ChatServer
{
    class Program
    {
        static void Main(string[] args)
        {
            Server server = new Server();
            Console.WriteLine("<enter> para sair...");
            Console.ReadLine();
        }
    }
}
