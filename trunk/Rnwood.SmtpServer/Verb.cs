﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rnwood.SmtpServer
{
    public abstract class Verb
    {
        public abstract void Process(ConnectionProcessor connectionProcessor, SmtpRequest request);
    }
}
