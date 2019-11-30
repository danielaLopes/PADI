using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClassLibrary;

namespace Client
{
    public class ClientAPI
    {
        private CClient _client;

        public ClientAPI(CClient client)
        {
            _client = client;
        }
        /// <summary>
        /// Lists all available meetings.
        /// </summary>
        public void List()
        {
            _client.List();
        }

        /// <summary>
        /// Creates a new meeting.
        /// </summary>
        /// <param name="fields"></param>
        public void Create(List<string> fields)
        {
            int nSlots = Int32.Parse(fields[3]);
            int lowerSlotBound = 5;
            int upperSlotBound = lowerSlotBound + nSlots;

            int nInvitees = Int32.Parse(fields[4]);
            int lowerInviteesBound = upperSlotBound;
            if (nInvitees != 0)
                _client.Create(fields[1], fields[2], fields.GetRange(lowerSlotBound, nSlots), fields.GetRange(lowerInviteesBound, nInvitees));

            else _client.Create(fields[1], fields[2], fields.GetRange(lowerSlotBound, nSlots));
        }

        /// <summary>
        /// Joins an existing meeting.
        /// </summary>
        /// <param name="fields"></param>
        public void Join(List<string> fields)
        {
            _client.Join(fields[1], fields.GetRange(3, 3 + Int32.Parse(fields[2])));
        }

        /// <summary>
        /// Closes a meeting
        /// </summary>
        /// <param name="fields"></param>
        public void Close(List<string> fields)
        {
            _client.Close(fields[1]);
        }

        /// <summary>
        /// Delays the execution of the next command for milliseconds.
        /// </summary>
        /// <param name="fields"></param>
        public void Wait(List<string> fields)
        {
            _client.Wait(fields[1]);
        }
    }
}
