using System;

namespace Stateflows.Activities.Attributes
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false)]
    public class InputAttribute : Attribute
    { }
}
