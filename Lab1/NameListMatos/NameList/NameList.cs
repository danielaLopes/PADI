using System;
using System.Collections;

namespace NameList
{
    internal class NameList: INameList
    {
        private ArrayList nameList;

        public NameList()
        {
            this.nameList = new ArrayList();
        }

        public void addName(string name)
        {
            this.nameList.Add(name);
            Console.WriteLine("added name {0}", name);
        }

        public string getNames()
        {
            string allNames = "";

            foreach (var name in this.nameList)
            {
                allNames += name;
                allNames += "\r\n";
            }

            Console.WriteLine("all names {0}", allNames);
            return allNames;
        }

        public void clearNames()
        {
            this.nameList.Clear();
            Console.WriteLine("list cleared");

        }

    }
}