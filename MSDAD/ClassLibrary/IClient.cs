using System;
using System.Collections.Generic;

namespace ClassLibrary
{
    public interface IClient : ISystemNode
    {
        void GetMasterUpdateClients(List<string> clientsUrls);

        void List();

        void Create(string meetingTopic, string minAttendees, List<string> slots, List<string> invitees = null);

        void Join(string meetingTopic, List<string> slots);

        void Close(string meetingTopic);

        void Wait(string milliseconds);

        void ReceiveInvitation(MeetingProposal proposal);
    }
}
