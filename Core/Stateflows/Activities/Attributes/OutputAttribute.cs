using System;

namespace Stateflows.Activities.Attributes
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public class OutputAttribute : Attribute
    { }
}
