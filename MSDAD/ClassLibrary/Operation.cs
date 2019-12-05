using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ClassLibrary
{
    [Serializable]
    public abstract class Operation
    {
        private VectorClock _vector;
        

        public Operation(VectorClock vector)
        {
            _vector = vector;
        }

        public VectorClock GetVectorClock()
        {
            return _vector;
        }

        public void printVectorClock(String meetingTopic)
        {
            _vector.printVectorClock(meetingTopic);
        }

        public abstract void executeOperation(IServer server, String topic);

        public abstract void printOperation();
    }

    [Serializable]
    public class CreateOperation: Operation
    {
        private MeetingProposal _proposal;
        public CreateOperation(VectorClock vector, MeetingProposal proposal) : base(vector)
        {
            _proposal = proposal;
        }

        public override void executeOperation(IServer server, string topic = null)
        {
            server.Create(_proposal);
        }

        public override void printOperation()
        {
            Console.WriteLine("create operation with proposal: {0}", _proposal.Topic);
        }
    }

    [Serializable]
    public class JoinOperation: Operation
    {
        private MeetingRecord _record;

        private string _username;
        public JoinOperation(VectorClock vector, MeetingRecord record, string username): base(vector)
        {
            _record = record;
            _username = username;
        }

        public override void executeOperation(IServer server, String topic)
        {
            server.Join(_username, topic, _record, false);
        }

        public override void printOperation()
        {
            Console.WriteLine("join operation with record: {0}", _record.Name);
        }
    }

    [Serializable]
    public class CloseOperation: Operation
    {
        private MeetingProposal _proposal;
        public CloseOperation(VectorClock vector, MeetingProposal proposal) : base(vector)
        {
            _proposal = proposal;
        }

        public override void executeOperation(IServer server, String topic)
        {
            server.ReceiveClose(_proposal, GetVectorClock());
        }

        public override void printOperation()
        {
            Console.WriteLine("close operation");
        }
    }
}
