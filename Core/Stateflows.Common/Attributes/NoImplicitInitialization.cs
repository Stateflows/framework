﻿using System;

namespace Stateflows.Common
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public sealed class NoImplicitInitializationAttribute : Attribute
    { }
}
