using System;
using System.Diagnostics;
using System.IO;

namespace PuppetMaster
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine(Directory.GetCurrentDirectory());
            Console.ReadLine();
            Process.Start(@"..\..\..\Server\bin\Debug\Server.exe");
            Process.Start(@"..\..\..\Client\bin\Debug\Client.exe", "Maria 8080");
            Console.ReadLine();
        }
    }
}
