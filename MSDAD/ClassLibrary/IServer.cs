using System.Collections;
using System.Collections.Generic;

namespace ClassLibrary
{
    /// <summary>
    /// Hides Server implementation from client. Interface to be implemented by
    /// the server to pass a Remote Object.
    /// </summary>
    public interface IServer : ISystemNode
    {
        // methods to be used by PuppetMaster
        void GetMasterUpdateServers(List<string> urls);
        //void GetMasterUpdateServer();

        void GetMasterUpdateClients(List<string> urls);

        void GetMasterUpdateLocations(Dictionary<string, Location> locations);

        // methods to be used by Clients
        void RegisterUser(string username, string clientUrl);

        void Create(MeetingProposal proposal);

        void List(string name, Dictionary<string,MeetingProposal> knownProposals);

        void Join(string topic, MeetingRecord record);
        
        void Close(string topic);

        void AttributeNewServer(string username);

    }
}
