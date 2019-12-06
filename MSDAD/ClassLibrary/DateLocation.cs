using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ClassLibrary
{
    [Serializable]
    public class DateLocation
    {
        public string LocationName { get; set; }
        private string date;
        public int Invitees { get; set; }

        public DateLocation(string locationName = "", string date = "")
        {
            this.LocationName = locationName;
            this.date = date;
            this.Invitees = 0;
        }

        public override string ToString()
        {
            return LocationName + "," + date;
        }

        public override bool Equals(object obj)
        {
            DateLocation dateLocation = (DateLocation)obj;
            return (dateLocation != null) && (this.LocationName.Equals(dateLocation.LocationName)) && (this.date.Equals(dateLocation.date));
        }
    }

    [Serializable]
    public class Location
    {
        public string Name { get; set; }
        public Dictionary<string,Room> Rooms { get; set; }

        public Location(string name)
        {
            Name = name;
            Rooms = new Dictionary<string, Room>();
        }

        /*public override bool Equals(object obj)
        {
            Location location = (Location)obj;
            return (location != null) && (this.Name.Equals(location.Name)) && (this.date.Equals(dateLocation.date));
        }*/

        public void AddRoom(Room room)
        {
            Rooms.Add(room.Name, room);
        }

        public override string ToString()
        {
            string roomList = "";
            foreach (KeyValuePair<string,Room> room in Rooms)
            {
                roomList += room.Value;
            }
            return Name + " " + roomList;
        }
    }

    [Serializable]
    public class Room
    {
        public enum RoomStatus
        {
            NONBOOKED,
            BOOKED,
        }

        public string Name { get; set; }
        public int Capacity { get; set; }
        public RoomStatus RoomAvailability { get; set; }
        public List<DateLocation> BookedDays { get; set; }

        public Room(string name = "", int capacity = 0, RoomStatus roomAvailability = RoomStatus.NONBOOKED)
        {
            this.Name = name;
            this.Capacity = capacity;
            this.RoomAvailability = roomAvailability;
            this.BookedDays = new List<DateLocation>();
        }

        public void AddBookedDay(DateLocation dateLocation)
        {
            BookedDays.Add(dateLocation);
        }

        public override string ToString()
        {
            return "(" + "\"" + Name + "\", " + Capacity + ")";
        }
    }

}