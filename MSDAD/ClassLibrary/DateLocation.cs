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
        public int Invitees { get; set; }

        public DateLocation(string locationName = "", string date = "")
        {
            this.locationName = locationName;
            this.date = date;
            this.Invitees = 0;
        }

        public override string ToString()
        {
            return locationName + "," + date;
        }

        public override bool Equals(object obj)
        {
            DateLocation dateLocation = (DateLocation) obj;
            return (dateLocation != null) && (this.locationName.Equals(dateLocation.locationName)) && (this.date.Equals(dateLocation.date));
        }

    }

    public class Location
    {
        public string Name { get; set; }
        public List<Room> Rooms { get; set; }

        public Location(string name)
        {
            Name = name;
            Rooms = new List<Room>();
        }

        public void AddRoom(Room room)
        {
            Rooms.Add(room);
        }

        public override string ToString()
        {
            string roomList = "";
            foreach(Room room in Rooms)
            {
                roomList += room;
            }
            return Name + " " + roomList;
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
