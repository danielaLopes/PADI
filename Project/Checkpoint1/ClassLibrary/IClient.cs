using System;

namespace ClassLibrary
{
    /// <summary>
    /// Hides Server implementation from client. Interface to be implemented by
    /// the client to pass a Remote Object.
    /// </summary>
    public interface IClient
    {

        void UpdateMessages(string nickname, string message);
    }
}
