using System;

namespace Stateflows.Common.Attributes
{
    /// <summary>Provides shared registration metadata for a behavior definition.</summary>
    /// <remarks>Derived behavior-kind attributes use this metadata to name and version a registered behavior and select its resource group.</remarks>
    /// <param name="name">Optional registered behavior name; when omitted, the framework infers a name from the type.</param>
    /// <param name="version">Behavior definition version.</param>
    /// <param name="resourceName">Optional resource group that hosts the behavior.</param>
    [AttributeUsage(AttributeTargets.Class)]
    public abstract class BehaviorAttribute(string? name = null, int version = 1, string? resourceName = null) : Attribute
    {
        /// <summary>Gets the registered behavior name, or <see langword="null"/> to infer one.</summary>
        public string? Name { get; init; } = name;

        /// <summary>Gets the behavior definition version.</summary>
        public int Version { get; init; } = version;

        /// <summary>Gets the name of the resource group hosting this behavior, when configured.</summary>
        public string? ResourceName { get; init; } = resourceName;
    }
}
