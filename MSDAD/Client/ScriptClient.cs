﻿using System;
using System.Collections;
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

        public void PrintGUI()
        {
            Console.WriteLine("");
            Console.WriteLine("        CLIENT commands:");
            Console.WriteLine("        list");
            Console.WriteLine("        create <meeting_topic> " +
                "<min_attendees> <n_slots> <n_invitees> <slots[n_slots]>" +
                " <invitees[n_invitees]>");
            Console.WriteLine("        join <meeting_topic>");
            Console.WriteLine("        close <meeting_topic>");
            Console.WriteLine("        wait [x_ms]");
            Console.WriteLine("        shutdown");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="args">
        ///     args[0]->username
        ///     args[1]->clientUrl
        ///     args[2]->serverUrl
        ///     args[3]->scriptFile
        ///     args[4]->nBackupServers
        ///     args[5:5+args[4]]->backupServer
        ///     (optional)
        ///     args[5+args[4]:]->otherClientsUrls
        /// </param>
        static void Main(string[] args)
        {
            CClient client;

            List<string> backupServers = new List<string>();
            int nBackupServers = Int32.Parse(args[4]);
            int i = 0;
            for (i = 5; i < 5 + nBackupServers; i++)
            {
                backupServers.Add(args[i]);
            }

            if (args.Length > 5 + nBackupServers)
            {
                int nClients = Int32.Parse(args[5 + nBackupServers + 1]);
                List<string> otherClientsUrl = new List<string>();
                for (; i < 5 + nBackupServers + nClients; i++)
                {
                    otherClientsUrl.Add(args[i]);
                }
                client = new CClient(args[0], args[1], args[2], backupServers, otherClientsUrl);
            }
            else
            {
                client = new CClient(args[0], args[1], args[2], backupServers);
            }
            
            string scriptPath = args[3];
            ScriptClient scriptClient = new ScriptClient(client);

            string[] lines = System.IO.File.ReadAllLines(@scriptPath);

            Console.WriteLine("Press s for executing commands sequentially and n for executing step by step");
            string mode = Console.ReadLine();

            // sequentially
            if (mode.Equals("s"))
            {
                foreach (string line in lines)
                {
                    Console.WriteLine(line);
                    scriptClient.ReceiveCommand(line);
                }
            }
            // step by step
            else if (mode.Equals("n"))
            {
                foreach (string line in lines)
                {
                    scriptClient.ReceiveCommand(line);
                    Console.WriteLine("Enter for next command");
                    Console.ReadLine();
                }
            }

            // accepts command-line commands
            string command = "";
            while (!command.Equals("shutdown"))
            {
                scriptClient.PrintGUI();
                command = Console.ReadLine();
                scriptClient.ReceiveCommand(command);
            }
        }
    }
}
