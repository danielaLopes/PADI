using System;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Collections.Generic;
using ClassLibrary;

namespace ChatServer
{
    public delegate void UpdateMessagesDelegate(RemoteChatRoomIClient remoteClient, string nickname, string message);

    public class CServer : MarshalByRefObject, RemoteChatRoomIServer
    {     
        private List<ChatClient> _clients;

        // to send messages to clients asynchhronously, otherwise the loop would deadlock
        public UpdateMessagesDelegate _updateMessagesDelegate;
        public AsyncCallback _updateMessagesCallback;

        private const string URL_SERVER = "tcp://localhost:8086/";
        private const string NAME_SERVER = "chat-server";

        public CServer()
        {
            // creates the server's remote object
            RemotingServices.Marshal(this, NAME_SERVER, typeof(CServer));

            _clients = new List<ChatClient>();

            _updateMessagesDelegate = new UpdateMessagesDelegate(UpdateMessages);
            _updateMessagesCallback = new AsyncCallback(UpdateMessagesCallback);
        }

        public void RegisterClient(string nickname, string urlClient)
        {
            // obtain client remote object
            RemoteChatRoomIClient remoteClient = (RemoteChatRoomIClient)Activator.GetObject(typeof(RemoteChatRoomIClient), urlClient);
            _clients.Add(new ChatClient(remoteClient, nickname));

            Console.WriteLine("New user " + nickname  + " with url " + urlClient + " registered in room ");
        }

        public void BroadcastMessage(string nickname, string message)
        {
            IAsyncResult res;
            // has to broadcast new message for every client in the chat room
            foreach (ChatClient client in _clients)
            {
                // only updates the messages to clients who were not the ones to send the message
                if (!client.Nickname.Equals(nickname))
                {
                    Console.WriteLine("dentro do loop");
                    res = _updateMessagesDelegate.BeginInvoke(client.RemoteClient, nickname, message, null, null);
                    //res.AsyncWaitHandle.WaitOne();
                    //_updateMessagesDelegate.EndInvoke(res);
                }
                    
            }

            Console.WriteLine("Published new message " + message + " from " + nickname + " in room ");
        }

        // function to be used by delegate to be called asynchronously with BeginInvoke
        public void UpdateMessages(RemoteChatRoomIClient remoteClient, string nickname, string message)
        {
            if (remoteClient == null)
            {
                Console.WriteLine("Could not obtain remote client " + nickname);
            }
            else
            {
                remoteClient.UpdateMessages(nickname, message);
            }
        }

        public void UpdateMessagesCallback(IAsyncResult res)
        {
            _updateMessagesDelegate.EndInvoke(res);
            Console.WriteLine("thread acabou!");
        }

		static void Main(string[] args) {

            // A server has to open a channel only once when the server application launches
            TcpChannel serverChannel = new TcpChannel(8086);
            ChannelServices.RegisterChannel(serverChannel, false);

            // this will create the server's remote object
            CServer server = new CServer();

            System.Console.WriteLine("<enter> para sair...");
			System.Console.ReadLine();
		}
	}
}