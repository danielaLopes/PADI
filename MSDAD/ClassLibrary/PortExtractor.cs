using System;
using System.Collections.Generic;
using System.Linq;

namespace ClassLibrary
{
    /// <summary>
    /// Library class to extract the port number from a given string containing an url
    /// </summary>
    public static class PortExtractor
    {
        /// <summary>
        /// Returns the port from a string with an url
        /// </summary>
        /// <param name="url">string of the url</param>
        /// <returns></returns>
        public static int Extract(string url)
        {
            string[] parts = url.Split(':');
            string urlPart = parts[2].Split('/')[0];

            return Int32.Parse(urlPart);
        }
    }
}
