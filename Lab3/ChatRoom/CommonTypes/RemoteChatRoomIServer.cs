using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ClassLibrary
{
    /// <summary>
    /// Hides Server implementation from client. Interface to be implemented by
    /// the server to pass a Remote Object.
    /// </summary>
    public interface RemoteChatRoomIServer
    {
        /// <summary>
        /// Gives the necessary information to the server about the client and the 
        /// chatroom the client wants to join in.
        /// </summary>
        /// <param name="nickname"> user's nickname </param>
        /// <param name="url"> contains the port and the name of the chatroom </param>
        void RegisterClient(string nickname, string url);

        /// <summary>
        /// Updates the chatroom with a user's message.
        /// </summary>
        /// <param name="nickname"> user's nickname </param>
        /// <param name="message"> message the user wants to send to the chatroom </param>
        void BroadcastMessage(string nickname, string message);
    }
}
