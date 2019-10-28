using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace PuppetMaster
{
    class ScriptMaster : MasterAPI
    {
        public void ReceiveCommand(string command)
        {
            List<string> fields = command.Split().ToList();
            string strFields = command.Replace(fields[0], "");

            if (fields[0].Equals("Server"))
            {
                Server(strFields, fields[1], fields[2]);
            }
            else if (fields[0].Equals("Client"))
            {
                Client(strFields, fields[1], fields[2]);
            }
            else if (fields[0].Equals("AddRoom"))
            {
                AddRoom(strFields);
            }
            else if (fields[0].Equals("Status"))
            {
                Status(strFields);
            }
            else if (fields[0].Equals("Crash"))
            {
                Crash(strFields);
            }
            else if (fields[0].Equals("Freeze"))
            {
                Freeze(strFields);
            }
            else if (fields[0].Equals("Unfreeze"))
            {
                Unfreeze(strFields);
            }
            else if (fields[0].Equals("Wait"))
            {
                Wait(strFields);
            }
        }

        static void Main(string[] args)
        {
            ScriptMaster scriptMaster = new ScriptMaster();

            string configFilePath = "C:config.txt";

            string[] lines = System.IO.File.ReadAllLines(@configFilePath);

            foreach (string line in lines)
            {
                scriptMaster.ReceiveCommand(line);
            }

            /*Process.Start(@"..\..\..\Server\bin\Debug\Server.exe");
            Process.Start(@"..\..\..\Client\bin\Debug\Client.exe", "Maria 8080");*/
            Console.ReadLine();
        }
    }
}
