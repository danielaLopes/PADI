using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassLibrary
{
    /// <summary>
    /// Library class to extract the base of the url without the endpoint and the port
    /// for example : tcp://localhost:9898/123 -> tcp://localhost:
    /// </summary>
    public static class BaseUrlExtractor
    {
        /// <summary>
        /// Returns the base of the url from a string with an url
        /// </summary>
        /// <param name="url">string of the url</param>
        /// <returns></returns>
        public static string Extract(string url)
        {
            string[] parts = url.Split('/');
            string basePart = parts[0] + "//" + parts[2].Split(':')[0];

            return basePart + ":";
        }
    }
}
