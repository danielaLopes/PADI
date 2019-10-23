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
    public interface IServer
    {

        void RegisterUser(string username, string clientUrl);

        List<MeetingProposal> List(string username);

        void Create(MeetingProposal proposal);
    }
}
