using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ClassLibrary;

namespace ChatServer
{
    class ChatClient        
    {
        private RemoteChatRoomIClient _remoteClient;
        public RemoteChatRoomIClient RemoteClient
        {
            get
            {
                return _remoteClient;
            }
            set
            {
                _remoteClient = value;
            }
        }

        private string _nickname;
        public string Nickname
        {
            get
            {
                return _nickname;
            }
            set
            {
                _nickname = value;
            }
        }

        public ChatClient(RemoteChatRoomIClient remoteClient, string nickname)
        {
            _remoteClient = remoteClient;
            _nickname = nickname;
        }
    }
}
