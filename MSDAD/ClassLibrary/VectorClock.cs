using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassLibrary
{
    [Serializable]
    public class VectorClock
    {
        /// <summary>
        /// Simulates a vector clock with n positions. n = number of known servers
        /// key: String corresponding to server url
        /// Value: int corresponding to relative time
        /// </summary>
        public ConcurrentDictionary<string, int> _currentVectorClock { get; set; } = new ConcurrentDictionary<string, int>();

        public VectorClock(String selfUrl, ICollection<String> otherUrls)
        {
            _currentVectorClock[selfUrl] = 0;
            foreach (String url in otherUrls)
                _currentVectorClock[url] = 0;
        }

        public VectorClock(ConcurrentDictionary<string, int> vec)
        {
            _currentVectorClock = new ConcurrentDictionary<string, int>(vec);
        }


        public void incrementVectorClock(String serverUrl)
        {
            _currentVectorClock[serverUrl]++;
        }

        public void printVectorClock(String meeting)
        {
            Console.WriteLine("Vector clock of meeting {0}: ", meeting);
            foreach (KeyValuePair<string, int> vectorPosition in _currentVectorClock)
            {
                Console.WriteLine("server: {0} clock: {1}", vectorPosition.Key, vectorPosition.Value);
            }
        }
    }
}
