using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ClassLibrary
{
    [Serializable]
    public class MeetingProposal
    {
        public string Coordinator { get; set; }
        public string Topic { get; set; }
        public int MinAttendees { get; set; }
        public List<DateLocation> DateLocationSlots { get; set; }
        public List<string> Invitees { get; set; }
        public List<MeetingRecord> Records { get; set; }

        public void AddMeetingRecord(MeetingRecord record)
        {
            Records.Add(record);
        }
        // TODO : e preciiso imprimir mais coisas????
        public override string ToString()
        {
            string slots = "";
            foreach (DateLocation dateLocation in DateLocationSlots)
            {
                slots += dateLocation.ToString() + " ";
            }
            return Topic + " " + MinAttendees + " " + slots;
        }
    }

    [Serializable]
    public class MeetingRecord
    {
        public string Name { get; }
        public List<DateLocation> DateLocationSlots { get; }

        public override string ToString()
        {
            return Name + " " + DateLocationSlots;
        }

        
    }
}
 