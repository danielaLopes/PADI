using System;
using System.Collections.Generic;

namespace Client
{

    interface ILocalClient
    {
        void List(string previousUrl = null);

        void Create(string meetingTopic, string minAttendees, List<string> slots, List<string> invitees = null);

        void Join(string meetingTopic, List<string> slots);

        void Close(string meetingTopic, string previousUrl = null);

        void Wait(string milliseconds);
    }
}
