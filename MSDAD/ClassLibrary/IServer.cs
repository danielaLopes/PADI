﻿using System.Collections;
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
        void Crash();

        void Freeze();

        void Unfreeze();

        // methods to be used by Clients
        List<string> AskForUpdateClients();

        void RegisterUser(string username, string clientUrl);

        void Create(MeetingProposal proposal);

        void List(string name, Dictionary<string,MeetingProposal> knownProposals);

        void Join(string username, string topic, MeetingRecord record);
        
        void Close(string topic);

        // methods to be used by other servers
        void ReceiveNewClient(string url);

        void ReceiveNewMeeting(MeetingProposal meeting);

        void ReceiveJoin(string username, MeetingProposal proposal, MeetingRecord record);

        void ReceiveClose(MeetingProposal proposal);

        void ReceiveUpdateLocation(Location location);

        IServer RegisterServer(string serverUrl);
    }
}
