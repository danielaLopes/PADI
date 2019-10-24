using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClassLibrary;

namespace Client
{
    class ScriptClient : ClientAPI
    {
        CClient _client;

        public ScriptClient(CClient client)
        {
            _client = client;

        }

        public void ReceiveCommand(string command)
        {
            List<string> fields = command.Split().ToList();
            if (fields[0].Equals("list"))
            {
                _client.List();
            }
            else if (fields[0].Equals("create"))
            {
                int nSlots = Int32.Parse(fields[3]);
                int lowerSlotBound = 5;
                int upperSlotBound = lowerSlotBound + nSlots;

                int nInvitees = Int32.Parse(fields[4]);
                int lowerInviteesBound = upperSlotBound;
                int upperInviteesBound = lowerInviteesBound + nInvitees;

                Console.WriteLine(fields.GetRange(lowerSlotBound, nSlots)[0]);
                Console.WriteLine(fields.GetRange(lowerSlotBound, nSlots)[1]);

                _client.Create(fields[1], fields[2], fields.GetRange(lowerSlotBound, nSlots), fields.GetRange(lowerInviteesBound, nInvitees));
            }
            else if (fields[0].Equals("join"))
            {
                _client.Join(fields[1], fields.GetRange(2, fields.Count - 2));
            }
            else if (fields[0].Equals("close"))
            {
                _client.Close(fields[1]);
            }
            else if (fields[0].Equals("wait"))
            {
                _client.Wait(fields[1]);
            }
        }

        public void List()
        {

        }

        public void Create(string meetingTopic, int minAttendees, string slots, string invitees = null)
        {

        }

        public void Join(string meetingTopic, List<DateLocation> slots)
            {

            }

        public void Close(string meetingTopic)
        {

        }

        public void Wait(int milliseconds)
        {

        }

        static void Main(string[] args)
        {
            CClient client = new CClient("Maria", 8090);
            string path = "C:/Users/user/OneDrive/Documents/Mestrado/PADI/PADI/Project/Checkpoint1/Client/commands.txt";
            ScriptClient scriptClient = new ScriptClient(client);

            string[] lines = System.IO.File.ReadAllLines(@path);

            foreach (string line in lines)
            {
                scriptClient.ReceiveCommand(line);
            }

            /*while()
            {
                int caseSwitch = 1;

                switch (caseSwitch)
                {
                    case 1:
                        Console.WriteLine("Case 1");
                        break;
                    case 2:
                        Console.WriteLine("Case 2");
                        break;
                    default:
                        Console.WriteLine("Default case");
                        break;
                }
            }*/

            System.Console.WriteLine("<enter> para sair...");
            System.Console.ReadLine();
        }
    }
}
