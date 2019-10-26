﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClassLibrary;

namespace Client
{
    public class ScriptClient : ClientAPI
    {
        public ScriptClient(CClient client) : base(client) { }

        public void ReceiveCommand(string command)
        {
            List<string> fields = command.Split().ToList();
            if (fields[0].Equals("list"))
            {
                List();
            }
            else if (fields[0].Equals("create"))
            {
                Create(fields);
            }
            else if (fields[0].Equals("join"))
            {
                Join(fields);
            }
            else if (fields[0].Equals("close"))
            {
                Close(fields);
            }
            else if (fields[0].Equals("wait"))
            {
                Wait(fields);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="args">
        ///     args[0]->username
        ///     args[1]->clientUrl
        ///     args[2]->serverUrl
        ///     args[3]->scriptFile
        /// </param>
        static void Main(string[] args)
        {
            CClient client = new CClient(args[0], args[1], args[2]);
            //string path = "C:../../commands.txt";
            string scriptPath = args[3];
            ScriptClient scriptClient = new ScriptClient(client);

            string[] lines = System.IO.File.ReadAllLines(@scriptPath);

            foreach (string line in lines)
            {
                scriptClient.ReceiveCommand(line);
            }

            System.Console.WriteLine("<enter> para sair...");
            System.Console.ReadLine();
        }
    }
}
