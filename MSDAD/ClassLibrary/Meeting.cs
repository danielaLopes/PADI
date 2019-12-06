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
        public Room FinalRoom { get; set; }

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

        public override string ToString()
        {
            if (MeetingStatus == MeetingStatus.CLOSED)
            {
                string participants = "";
                foreach (string part in Participants)
                {
                    participants += part.ToString() + " ";
                }
                string failedParticipants = "";
                foreach (MeetingRecord part in FailedRecords)
                {
                    failedParticipants += part.ToString() + " ";
                }
                string fullParticipants = "";
                foreach (MeetingRecord part in FullRecords)
                {
                    fullParticipants += part.ToString() + " ";
                }

                return MeetingStatus + " " + Topic + " " + participants + " " + FinalDateLocation + " " + FinalRoom + "\n" +
                        "Failed Records : " + failedParticipants + "\n" + "Not enough space Participants : " + fullParticipants;
            }

            string slots = "";
            foreach (DateLocation dateLocation in DateLocationSlots)
            {
                slots += dateLocation.ToString() + " ";
            }
            return MeetingStatus + " " + Topic + " " + MinAttendees + " " + slots;
        }
    }

    [Serializable]
    public class MeetingRecord : IEquatable<MeetingRecord>, IComparable
    {
        public string Name { get; set; }
        public List<DateLocation> DateLocationSlots { get; set; }

        public VectorClock _vector { get; set; }

        public override string ToString()
        {
            string slots = "";
            foreach(DateLocation slot in DateLocationSlots)
            {
                slots += slot.ToString() + " ";
            }
            return Name + " " + slots;
        }

        // Default comparer for MeetingRecord type.
        public int CompareTo(object compareRecord)
        {
            // A null value means that this object is greater.
            if (compareRecord == null)
                return 1;

            MeetingRecord rec = compareRecord as MeetingRecord;

            int clockComparison = this._vector.CompareTo(rec._vector);

            // if an order between vector clocks can't be established => alphabetical order
            if (clockComparison == 0)
                return Name.CompareTo(rec.Name);

            return clockComparison;
        }

        public bool Equals(MeetingRecord compareRecord)
        {
            if (compareRecord == null)
                return false;
            return compareRecord.Name.Equals(Name);
        }
    }
}
