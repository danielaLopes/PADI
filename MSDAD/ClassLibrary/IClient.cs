using System;
using System.Collections;
using System.Collections.Generic;

namespace ClassLibrary
{
    public interface IClient : ISystemNode
    {
        // methods to be used by servers
        void UpdateList(Dictionary<string,MeetingProposal> proposals);

        void SwitchServer(string url);

        void UpdateClients(List<string> urls);

        // methods to be used by other clients
        void ReceiveInvitation(MeetingProposal proposal, List<string> inviteesLeft = null);
    }
}
