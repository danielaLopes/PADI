using System;
using System.Collections;
using System.Collections.Generic;

namespace ClassLibrary
{
    public interface IClient : ISystemNode
    {
        void GetMasterUpdateClients(List<string> clientsUrls);

        void UpdateList(Dictionary<string,MeetingProposal> proposals);

        void ReceiveInvitation(MeetingProposal proposal);
    }
}
