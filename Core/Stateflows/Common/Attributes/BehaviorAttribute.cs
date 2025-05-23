﻿using System;

namespace Stateflows.Common.Attributes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public abstract class BehaviorAttribute : Attribute
    {
        public string Name { get; private set; }

        public int Version { get; private set; }

        protected BehaviorAttribute(string name, int version = 1)
        {
            Name = name;
            Version = version;
        }
    }
}
