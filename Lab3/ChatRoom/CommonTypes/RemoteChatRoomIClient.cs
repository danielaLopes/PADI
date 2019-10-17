using System;

namespace ClassLibrary
{
    /// <summary>
    /// Hides Server implementation from client. Interface to be implemented by
    /// the client to pass a Remote Object.
    /// </summary>
    public interface RemoteChatRoomIClient
    {
        /// <summary>
        /// Updates the chatroom with a user's message.
        /// </summary>
        /// <param name="nickname"> user's nickname </param>
        /// <param name="message"> message the user wants to send to the chatroom </param>
        void UpdateMessages(string nickname, string message);
    }
}
