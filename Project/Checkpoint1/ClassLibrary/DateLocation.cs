using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ClassLibrary
{
    [Serializable]
    public class DateLocation
    {
        private string locationName;
        private string date;

        public DateLocation(string locationName, string date)
        {
            this.locationName = locationName;
            this.date = date;
        }

        public override string ToString()
        {
            return locationName + "," + date;
        }
    }

    public class Location
    {
        private string name;
        private List<Room> rooms;

        // TODO : receive room list or add them one by one
        public Location(string name, List<Room> rooms)
        {
            this.name = name;
            this.rooms = rooms;
        }
        public override string ToString()
        {
            string roomList = "";
            foreach(Room room in rooms)
            {
                roomList += room;
            }
            return name + ", " + roomList;
        }
    }

    public class Room
    {
        private string name;
        private int capacity;

        public Room(string name, int capacity)
        {
            this.name = name;
            this.capacity = capacity;
        }

        public override string ToString()
        {
            return "(" + "\"" + name + "\", " + capacity + ")";
        }
    }

}
