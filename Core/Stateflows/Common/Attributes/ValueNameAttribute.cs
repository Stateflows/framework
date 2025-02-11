using System;

namespace Stateflows.Common.Attributes
{
    [Obsolete("ValueNameAttribute is obsolete, use GlobalValueAttribute, StateValueAttribute, SourceStateValueAttribute or TargetStateValueAttribute with IValue<T> or target type directly instead.")]
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false)]
    public sealed class ValueNameAttribute : Attribute
    {
        public string Name { get; set; }

        public ValueNameAttribute(string name)
        {
            Name = name;
        }
    }
}
