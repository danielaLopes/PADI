using System;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels.Tcp;
using System.Runtime.Remoting.Channels;
using System.Net.Sockets;
using ClassLibrary;

namespace ChatClient
{
    public delegate void UpdateMessagesDelegate(string nickname, string message);

    class CClient : MarshalByRefObject, RemoteChatRoomIClient
    {
        private ChatRoomForm _form;

        private TcpChannel _clientChannel;

        private const string URL_SERVER = "tcp://localhost:8086/";

        public CClient(ChatRoomForm form)
        {
            _form = form;
        }

        public void RegisterChannel(int clientPort)
        {
            _clientChannel = new TcpChannel(clientPort);
            ChannelServices.RegisterChannel(_clientChannel, false);
        }

        public void UpdateMessages(string nickname, string message)
        {
            if (_form.ConversationTextBox.InvokeRequired)
            {
                Console.WriteLine("invoke");
                UpdateMessagesDelegate updateMessagesDelegate = new UpdateMessagesDelegate(UpdateMessages);
                _form.ConversationTextBox.Invoke(updateMessagesDelegate, new object[] { nickname, message });
            }
            else
            {
                Console.WriteLine("non invoke");
                _form.ConversationTextText += nickname + ": " + message + "\r\n";
            }

        }
    }
}
