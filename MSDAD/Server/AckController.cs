using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Server
{
    public class AckController
    {
        public int N_acks { get; set; }

        public AckController()
        {
            N_acks = 0;
        }
    }
}
