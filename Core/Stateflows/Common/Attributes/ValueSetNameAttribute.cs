using System;

namespace Stateflows.Common.Attributes
{
    [Obsolete("ValueSetNameAttribute is obsolete, use GlobalNamespaceAttribute, StateNamespaceAttribute, SourceStateNamespaceAttribute or TargetStateNamespaceAttribute with INamespace<T> instead.")]
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
