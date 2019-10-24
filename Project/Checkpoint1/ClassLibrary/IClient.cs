using System;

namespace ClassLibrary
{

    public interface IClient
    {
        void List();

        void Create(string meetingTopic, string minAttendees, string slots, string invitees = null);

        void Join(string meetingTopic);

        void Close(string meetingTopic);

        void Wait(string milliseconds);
    }
}
