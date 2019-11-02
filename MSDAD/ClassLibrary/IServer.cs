using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ClassLibrary
{
    /// <summary>
    /// Hides Server implementation from client. Interface to be implemented by
    /// the server to pass a Remote Object.
    /// </summary>
    public interface IServer : ISystemNode
    {
        void GetMasterUpdateServers(List<string> serversUrls);

        void GetMasterUpdateClients(List<string> clientUrls);

        void GetMasterUpdateLocations(Dictionary<string, Location> locations);

        void RegisterUser(string username, string clientUrl);

        void Create(MeetingProposal proposal);

        void Join(string topic, MeetingRecord record);
    }
}
