using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ClassLibrary
{
    public class MeetingProposal
    {
        private string coordinator;
        private string topic;
        private int minParticipants;
        private List<DateLocation> dateLocationSlots;
        private List<string> invitees;
        private List<MeetingRecord> records;

        public MeetingProposal(string coordinator, string topic, int minParticipants, List<DateLocation> dateLocationSlots, List<string> invitees)
        {
            this.coordinator = coordinator;
            this.topic = topic;
            this.minParticipants = minParticipants;
            this.dateLocationSlots = dateLocationSlots;
            this.invitees = invitees;
            this.records = new List<MeetingRecord>();
        }

        public void AddMeetingRecord(MeetingRecord record)
        {
            records.Add(record);
        }
        public override string ToString()
        {
            //TODO
            return null;
        }
    }

    public class MeetingRecord
    {
        private string name;
        private List<DateLocation> dateLocationSlots;
    }
}
 