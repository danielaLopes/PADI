using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassLibrary
{
    [Serializable]
    public class VectorClock: IComparable 
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

        public int getVectorSum()
        {
            int sum = 0;
            foreach (KeyValuePair<string, int> pair in _currentVectorClock)
                sum += pair.Value;

            return sum;
        }

        public int CompareTo(object compareVector)
        {
            // A null value means that this object is greater.
            if (compareVector == null)
                return 1;

            VectorClock clk = compareVector as VectorClock;

            List<string> keys = new List<string>(_currentVectorClock.Keys);

            string initialKey = keys[0];
            List<int> comparisons = new List<int> { _currentVectorClock[initialKey].CompareTo(clk._currentVectorClock[initialKey]) };
            int decision = comparisons[0];


            for (int i = 1; i < keys.Count; i++)
            {

                int currentComparison = _currentVectorClock[keys[i]].CompareTo(clk._currentVectorClock[keys[i]]);

                foreach (int previousComparison in comparisons)
                    if ((previousComparison == -1 && currentComparison == 1) || (previousComparison == 1 && currentComparison == -1))
                        return 0;


                if (currentComparison != 0)
                    decision = currentComparison;

                comparisons.Add(currentComparison);
            }

            return decision;
        }
    }
}
