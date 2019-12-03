using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassLibrary
{
    public class Operation
    {
        private VectorClock _vector;
        

        public Operation(VectorClock vector)
        {
            _vector = vector;
        }

        public void printVectorClock(String meetingTopic)
        {
            _vector.printVectorClock(meetingTopic);
        }
    }

    public class JoinOperation: Operation
    {
        private MeetingRecord _record;
        public JoinOperation(VectorClock vector, MeetingRecord record): base(vector)
        {
            _record = record;
            Console.WriteLine("Constructed new join operation.");
        }
    }

    public class CloseOperation: Operation
    {
        public CloseOperation(VectorClock vector) : base(vector)
        {
            Console.WriteLine("Constructed new close operation.");
        }
    }
}
