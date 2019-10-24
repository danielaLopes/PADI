using System;
using System.Diagnostics;

namespace PuppetMaster
{
    class Program
    {
        static void Main(string[] args)
        {
            Process.Start("C:../../../../Server/bin/Debug/Server.exe");
            Process.Start("C:../../../../Client/bin/Debug/User.exe", "Maria 8080");
        }
    }
}
