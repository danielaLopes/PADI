using System;
using System.Collections;
using System.Collections.Generic;

namespace ClassLibrary
{
    public interface IClient : ISystemNode
    {
        // methods to be used by PuppetMaster
        void GetMasterUpdateClients(List<string> clientsUrls);

        // methods to be used by servers
        void UpdateList(Dictionary<string,MeetingProposal> proposals);

        void ReceiveInvitation(MeetingProposal proposal);

        void SwitchServer(string url);
    }
}
