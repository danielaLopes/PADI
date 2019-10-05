using System;

namespace Delegates
{
    delegate void MyDelegate(string s);

    class Program
    {
        public static void Hello(string s)
        {
            Console.WriteLine("  Hello, {0}!", s);
        }

        public static void Goodbye(string s)
        {
            Console.WriteLine("  Goodbye, {0}!", s);
        }

        public static void DelegateTest(MyDelegate d)
        {
            Console.WriteLine("Invoking delegate inside method:");
            d("Inside Method");
        }

        public static void Main()
        {
            MyDelegate a, b, c, d, e;

            // Create the delegate object a that references
            // the method Hello:
            a = new MyDelegate(Hello);
            // Create the delegate object b that references
            // the method Goodbye:
            b = new MyDelegate(Goodbye);
            // The two delegates, a and b, are composed to form c:
            c = a + b;
            // Remove a from the composed delegate, leaving d,
            // which calls only the method Goodbye:
            d = c - a;
            // Calling a null delegate
            e = null;
            // Attributing a new method to a delegate with methods in it
            c = new MyDelegate(Hello);

            Console.WriteLine("Invoking delegate a:");
            a("A");
            Console.WriteLine("Invoking delegate b:");
            b("B");
            Console.WriteLine("Invoking delegate c:");
            c("C"); // old methods are overriden by the new ones
            Console.WriteLine("Invoking delegate d:");
            d("D");
            // Console.WriteLine("Invoking delegate e:");
            // e("E"); 
            // Internal error

            // testing delegate call inside method
            DelegateTest(d);
        }
    }
}
