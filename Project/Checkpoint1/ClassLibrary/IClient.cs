using System;

namespace ClassLibrary
{

    public interface IClient
    {
        void List();

        void Create(string meetingTopic, int minAttendees, string slots, string invitees = null);

        void Join(string meetingTopic);

        void Close(string meetingTopic);

        void Wait(int milliseconds);
    }
}
