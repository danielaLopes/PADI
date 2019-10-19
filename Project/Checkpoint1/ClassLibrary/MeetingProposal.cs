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

        public MeetingProposal(string coordinator, string topic, int minParticipants, List<DateLocation> dateLocationSlots, List<string> invitees)
        {
            this.coordinator = coordinator;
            this.topic = topic;
            this.minParticipants = minParticipants;
            this.dateLocationSlots = dateLocationSlots;
            this.invitees = invitees;
        }

        public override string ToString()
        {
            //TODO
            return null;
        }
    }
}
 