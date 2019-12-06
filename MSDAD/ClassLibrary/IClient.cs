using System;
using System.Collections;
using System.Collections.Generic;

namespace ClassLibrary
{
    public interface IClient : ISystemNode
    {
        // methods to be used by servers
        void UpdateList(Dictionary<string,MeetingProposal> proposals);

        void SwitchServer();

        void UpdateClients(List<string> urls);

        // methods to be used by other clients
        void ReceiveInvitation(MeetingProposal proposal, int nClients, 
                List<string> inviteesLeft = null, string previousUrl = null);
    }
}
