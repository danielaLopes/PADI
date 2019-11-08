using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ClassLibrary
{
    public enum MeetingStatus
    {
        OPEN,
        CANCELLED, // not enough space in rooms
        CLOSED
    }

    [Serializable]
    public class MeetingProposal
    {
        public string Coordinator { get; set; }
        public string Topic { get; set; }
        public int MinAttendees { get; set; }
        public List<DateLocation> DateLocationSlots { get; set; }
        public List<string> Invitees { get; set; }
        public List<MeetingRecord> Records { get; set; }
        // Maintains info on records that failed due to concurrent join and closes
        public List<MeetingRecord> FailedRecords { get; set; }
        // Maintains info on records that failed due to not enough participants' slots
        public List<MeetingRecord> FullRecords { get; set; }
        public MeetingStatus MeetingStatus { get; set; }
        public List<string> Participants { get; set; }
        public DateLocation FinalDateLocation { get; set; }

        public void AddMeetingRecord(MeetingRecord record)
        {
            Records.Add(record);
        }

        public void AddFailedRecord(MeetingRecord record)
        {
            FailedRecords.Add(record);
        }

        public void AddFullRecord(MeetingRecord record)
        {
            FullRecords.Add(record);
        }

        public override bool Equals(object obj)
        {
            MeetingProposal meetingProposal = (MeetingProposal)obj;
            return (meetingProposal != null) && (Topic.Equals(meetingProposal.Topic));
        }

        public override int GetHashCode()
        {
            return Topic.GetHashCode();
        }

        // TODO : e preciiso imprimir mais coisas????
        public override string ToString()
        {
            if (MeetingStatus == MeetingStatus.CLOSED)
            {
                string participants = "";
                foreach (string part in Participants)
                {
                    participants += part.ToString() + " ";
                }
                return MeetingStatus + " " + Topic + " " + participants + " " + FinalDateLocation;
            }

            string slots = "";
            foreach (DateLocation dateLocation in DateLocationSlots)
            {
                slots += dateLocation.ToString() + " ";
            }
            return MeetingStatus + " " + Topic + " " + MinAttendees + " " + slots;
        }
    }

    public enum RecordStatus
    {
        ACCEPTED,
        FAILED, // Join came after Close
        UNKNOWN // Before reaching the server, it's the server that attributes a known Status

    }

    [Serializable]
    public class MeetingRecord
    {
        public string Name { get; set; }
        public List<DateLocation> DateLocationSlots { get; set; }
        public RecordStatus RecordStatus { get; set; }

        public override string ToString()
        {
            return Name + " " + DateLocationSlots;
        }
    }
}
