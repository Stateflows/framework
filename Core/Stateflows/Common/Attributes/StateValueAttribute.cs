﻿using System;
using Stateflows.Common.Attributes;

#nullable enable
namespace Stateflows.StateMachines.Attributes
{
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false)]
    public sealed class StateValueAttribute : ValueAttribute
    {
        public StateValueAttribute(string? name = null, bool required = true) : base(name, required)
        { }
    }
}
