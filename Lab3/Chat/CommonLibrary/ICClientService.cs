﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonLibrary
{
    public interface ICClientService
    {
        void sendClient(List<string> message);

        int messagesAmount();

    }
}
