using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonLibrary
{
    public interface ICServerService
    {
        void sendMessage(string nickname, string message);
        void addUser(string nickname, string URL);
    }
}
