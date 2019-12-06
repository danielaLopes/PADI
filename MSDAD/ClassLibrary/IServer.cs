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
        // methods to be used by PuppetMaster to simulate faults and delays
        void Freeze();

        void Unfreeze();

        // methods to be used by Clients
        List<string> AskForUpdateClients(string urlFailed = null);

        void RegisterUser(string username, string clientUrl, string urlFailed = null);

        void Create(MeetingProposal proposal, string urlFailed = null);

        void List(string name, Dictionary<string,MeetingProposal> knownProposals, string urlFailed = null);

        void Join(string username, string topic, MeetingRecord record, string urlFailed = null, bool local = true);
     
        void Close(string topic, string urlFailed = null);


        // methods to be used by other servers

        void ReceiveNewMeeting(MeetingProposal meeting, VectorClock newVector);

        void ReceiveNewClient(string url);

        void ReceiveJoin(string username, MeetingProposal proposal, MeetingRecord record, VectorClock newVector);

        void ReceiveClose(MeetingProposal proposal, VectorClock newVector);

        void ReceiveUpdateLocation(Location location);

        void ReceiveDeadServers(string deadServer);

        Room getAvailableRoom(DateLocation finalDateLocation, MeetingProposal proposal);

        IServer RegisterServer(string serverUrl);

        // vector clocks related methods

        List<Operation> retrieveOperations(string meetingTopic, int minClock, int maxClock);
    }
}
