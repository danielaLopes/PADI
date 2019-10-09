using System;
using System.Threading;

namespace Threads
{
    delegate void ThrWork();

    class ThrPool
    {
        //private readonly int _bufSize;
        //private int _nRequests;

        public ThrPool(int thrNum, int bufSize)
        {
            // TODO
            ThreadPool.SetMaxThreads(5, 5);
            ThreadPool.SetMinThreads(1, 1);

            //_bufSize = bufSize;
           // _nRequests = 0;
        }

        /*
         * Method for asynchronous execution of the delegate ThrWork
         */
        public void AssyncInvoke(ThrWork action)
        {
            // TODO
            // check if we need to limit the number of requests ?????
            ThreadPool.QueueUserWorkItem(delegate { action(); });
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

    class Program
    {
        static void Main(string[] args)
        {
            /** 
             * Invocation Requests are placed in this buffer to be managed in a circular way 
             * 5 threads
             * 10 requests (buffer size)
             */
            ThrPool tpool = new ThrPool(5, 10);
            /** Delegate */
            ThrWork work = null; // why this delegate ????
            for (int i = 0; i < 10; i++)
            {
                A a = new A(i);
                tpool.AssyncInvoke(new ThrWork(a.DoWorkA));
                B b = new B(i);
                tpool.AssyncInvoke(new ThrWork(b.DoWorkB));
            }
            Console.ReadLine(); // whythis realine ??????
        }
    }
}
