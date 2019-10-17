using System;
using System.Windows.Forms;
using System.Net.Sockets;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using ChatServer;
using ClassLibrary;

namespace ChatClient
{
    public partial class ChatRoomForm : Form
    {
        private CClient _client;
        private RemoteChatRoomIServer _remoteServer;
        const string URL_SERVER = "tcp://localhost:8086/";
        const string NAME_SERVER = "chat-server";

        public ChatRoomForm()
        {
            InitializeComponent();

            _client = new CClient(this);
        }

        public string ConversationTextText
        {
            get
            {
                return conversationText.Text;
            }
            set
            {
                conversationText.Text = value;
            }
        }

        public TextBox ConversationTextBox
        {
            get
            {
                return conversationText;
            }
            set
            {
                conversationText = value;
            }
        }


        private void splitter1_SplitterMoved(object sender, SplitterEventArgs e)
        {
                    }

        private void connectButton_Click(object sender, EventArgs e)
        {
            Console.WriteLine("Registering client ...");

            // ------------------- CLIENT PART -------------------
            int portClient = Int32.Parse(portText.Text);
            string nicknameClient = nicknameText.Text;  
            string urlClient = "tcp://localhost:" + portClient + "/" + nicknameClient;

            _client.RegisterChannel(portClient);

            // converts _client into a remote object registered with name urlClient, with the provided type
            RemotingServices.Marshal(_client, nicknameClient, typeof(RemoteChatRoomIClient));
            /*RemotingConfiguration.RegisterWellKnownClientType(
                typeof(RemoteChatRoomIClient),
                urlServer);*/

            // retrieve server's "proxy" (server's remote object)
            _remoteServer = (RemoteChatRoomIServer)Activator.GetObject(typeof(RemoteChatRoomIServer), URL_SERVER + NAME_SERVER);
            try
            {
                if (_remoteServer == null)
                {
                    Console.WriteLine("Could not obtain remote server");
                }
                else
                {
                    // ---------------- REMOTE SERVER PART ----------------
                    // do what needs to be done with the remote server object, in this case register a client
                    _remoteServer.RegisterClient(nicknameClient, urlClient);
                    Console.WriteLine("Registering client " + nicknameClient + " was successful!");
                }
            }
            catch (SocketException)
            {
                Console.WriteLine("Could not locate remote server");
            }
        }

        private void sendButton_Click(object sender, EventArgs e)
        {
            string nickname = nicknameText.Text;
            string message = messageText.Text;

            // update the new message in other clients
            _remoteServer.BroadcastMessage(nickname, message);
            // server does not need to update the new message to the client that sent the message
            _client.UpdateMessages(nickname, message);
        }
    }
}
