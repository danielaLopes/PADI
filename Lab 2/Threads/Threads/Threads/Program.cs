using System;
using System.Threading;

namespace Threads
{
    delegate void ThrWork();

    class ThrPool
    {
        /** Threads */
        private Thread[] threadPool;
        /** Requests */
        private RequestsCircularBuffer _requestsQueue;
        /** Decide when the thread pool stops running */
        private bool _running;

        public ThrPool(int thrNum, int bufSize)
        {
            Running = true;

            /** Creates the request queue with limited size */
            _requestsQueue = new RequestsCircularBuffer(bufSize);

            /** Creates thread pool */
            threadPool = new Thread[thrNum];
            for (int i = 0; i < thrNum; i++)
            {
                threadPool[i] = new Thread(ProcessRequests);
                threadPool[i].Start();
            }

            /*foreach(Thread thread in threadPool)
            {
                Console.WriteLine(thread.ManagedThreadId);
            }*/
        }

        public bool Running
        {
            set
            {
                _running = value;
            }
            get
            {
                return _running;
            }
        }

        /*
         * Method for asynchronous execution of the delegate ThrWork.
         * Writes requests into the request queue.
         */
        public void AssyncInvoke(ThrWork action)
        {
            _requestsQueue.WriteRequest(action);
        }

        /** 
         * Method to be execute by every thread in the thread pool.
         * Checks if there are requests to be processed in the request queue.
         * If there are requests, one of the threads processes it, otherwise it waits until
         * a request is inserted in the request queue.
         */
        public void ProcessRequests()
        {
            while (_running)
            {
                _requestsQueue.ReadRequest().Invoke();//.BeginInvoke(null, null, null);
            }
            Console.WriteLine("Thread finished");
        }
    }

    /**
     * Request A
     */
    class A
    {
        private int _id;

        public A(int id)
        {
            _id = id;
        }

        public void DoWorkA()
        {
            Console.WriteLine("A-{0}", _id);
        }
    }

    /**
     * Request B
     */
    class B
    {
        private int _id;

        public B(int id)
        {
            _id = id;
        }

        public void DoWorkB()
        {
            Console.WriteLine("B-{0}", _id);
        }
    }

    class EndProgram
    {
        public void End()
        {
            Console.WriteLine("Program finished executing all requests");
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            /** 
             * Invocation Requests are placed in this buffer to be managed in a circular way 
             * 5 threads
             * 10 requests (buffer size)
             */
            const int nThreads = 5;
            const int nRequests = 10;

            ThrPool tpool = new ThrPool(nThreads, nRequests);
            /** Delegate */
            for (int i = 0; i < (nRequests / 2) ; i++)
            {
                A a = new A(i);
                tpool.AssyncInvoke(new ThrWork(a.DoWorkA));
                B b = new B(i);
                tpool.AssyncInvoke(new ThrWork(b.DoWorkB));
            }
            Console.ReadLine();

            /** stops threads from reading requests */
            tpool.Running = false;
            EndProgram end = new EndProgram();
            for(int thread = 0; thread < nThreads ; thread++)
            {
                tpool.AssyncInvoke(new ThrWork(end.End));
            }
        }
    }
}
