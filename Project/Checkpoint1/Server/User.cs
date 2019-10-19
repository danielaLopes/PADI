using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ClassLibrary;

namespace Server
{
    class User
    {
        private IClient _remoteClient;
        public IClient RemoteClient
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

        public User(IClient remoteClient, string nickname)
        {
            _remoteClient = remoteClient;
            _nickname = nickname;
        }
    }
}
