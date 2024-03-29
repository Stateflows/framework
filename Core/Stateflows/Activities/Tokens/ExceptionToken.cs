﻿using System;
using Stateflows.Common;

namespace Stateflows.Activities
{
    public sealed class ExceptionToken<TException> : Token
        where TException : Exception
    {
        public TException Exception { get; set; }
    }
}
