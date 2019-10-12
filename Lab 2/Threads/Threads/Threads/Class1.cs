using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Threads
{
    /**
     * Circular buffer to store requests for threads in the ThreadPool to execute.
     * Has a limited size for writes and a read is always done to the first 
     * element of the buffer. When a read is executed, the element is removed from 
     * the list and the more recent requests go to the front of the queue.
     * First in first out (FIFO).
     */
    class RequestsCircularBuffer
    {
        private Queue<ThrWork> _requestsQueue = new Queue<ThrWork>();
        private readonly int _maxSize;

        public RequestsCircularBuffer(int maxSize)
        {
            _maxSize = maxSize;
        }

        public int getRequestsCount()
        {
            return _requestsQueue.Count;
        }

        /** Equivalent to a dequeue */
        public ThrWork ReadRequest()
        {
            lock (_requestsQueue)
            {
                // No requests to read, wait until a new request goes into the queue
                while (_requestsQueue.Count == 0)
                {
                    Console.WriteLine("Read - Thread {0} is sleeping", Thread.CurrentThread.ManagedThreadId);
                    Monitor.Wait(_requestsQueue);
                }
                Console.WriteLine("Read - Thread {0} is reading", Thread.CurrentThread.ManagedThreadId);
                ThrWork request = _requestsQueue.Dequeue();
                if (_requestsQueue.Count == _maxSize - 1)
                {
                    // wake up some blocked thread to come write a new request
                    Monitor.PulseAll(_requestsQueue); // needs to have pulseAll!
                }

                return request;
            }
        }

        /** Equivalent to an enqueue */
        public void WriteRequest(ThrWork request)
        {
            lock (_requestsQueue)
            {
                // No space for more requests, wait until a request is processed 
                while (_requestsQueue.Count >= _maxSize)
                {
                    Console.WriteLine("Write - Thread {0} is sleeping", Thread.CurrentThread.ManagedThreadId);
                    Monitor.Wait(_requestsQueue);
                }
                Console.WriteLine("Write - Thread {0} is writing", Thread.CurrentThread.ManagedThreadId);
                _requestsQueue.Enqueue(request);
                if (_requestsQueue.Count == 1)
                {
                    // wake up some blocked thread to come process request
                    Monitor.PulseAll(_requestsQueue); // needs to have pulseAll!
                }
            }
        }


        public override string ToString()
        {
            string queue = "";
            for(int i = 0; i < _requestsQueue.Count; i++) {
                queue += "Request number : " + i + "\r\n";
            }

            return queue;
        }
    }
}
