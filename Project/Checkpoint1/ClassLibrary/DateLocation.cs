﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ClassLibrary
{
    public class DateLocation
    {
        private String date;
        private Location location ;
        public DateLocation(string dateLocation)
        {

        }
    }

    public class Location
    {
        private string name;
        private List<Room> rooms;
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
    }

}
