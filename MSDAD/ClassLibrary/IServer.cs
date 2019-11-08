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
        void UpdateServerAndSpread(string url);

        IServer UpdateServer(string url);

        void UpdateClient(string url);

        // methods to be used by Clients
        void RegisterUser(string username, string clientUrl);

        void Create(MeetingProposal proposal);

        void List(string name, Dictionary<string,MeetingProposal> knownProposals);

        void Join(string topic, MeetingRecord record);
        
        void Close(string topic);

        void AttributeNewServer(string username);

        // methods to be used by other servers
        void ReceiveNewMeeting(MeetingProposal meeting);

        void ReceiveJoin(string topic, MeetingRecord record);

    }
}
