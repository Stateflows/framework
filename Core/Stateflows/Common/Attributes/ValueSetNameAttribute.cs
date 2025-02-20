using System;

namespace Stateflows.Common.Attributes
{
    [Obsolete("ValueSetNameAttribute is obsolete, use GlobalValueSetAttribute, StateValueSetAttribute, SourceStateValueSetAttribute or TargetStateValueSetAttribute with IValue<T> or target type directly instead.")]
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false)]
    public sealed class ValueSetNameAttribute : Attribute
    {
        public string Name { get; set; }

        public ValueSetNameAttribute(string name)
        {
            Name = name;
        }
    }
}
