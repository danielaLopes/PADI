using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClassLibrary;

namespace Client
{
    interface ClientAPI
    {
        /// <summary>
        /// Lists all available meetings.
        /// </summary>
        void List();

        /// <summary>
        /// Creates a new meeting.
        /// </summary>
        /// <param name="meetingTopic">meeting's identifier</param>
        /// <param name="minAttendees">minimum number of attendees required</param>
        /// <param name="slots">set of possible dates and locations</param>
        /// <param name="invitees">optional group of invite users</param>
        void Create(string meetingTopic, int minAttendees, string slots, string invitees = null);

        /// <summary>
        /// Joins an existing meeting.
        /// </summary>
        /// <param name="meetingTopic"></param>
        void Join(string meetingTopic);

        /// <summary>
        /// Closes a meeting
        /// </summary>
        /// <param name="meetingTopic"></param>
        void Close(string meetingTopic);

        /// <summary>
        /// Delays the execution of the next command for milliseconds.
        /// </summary>
        /// <param name="milliseconds">number of milliseconds to delay</param>
        void Wait(int milliseconds);
    }
}
