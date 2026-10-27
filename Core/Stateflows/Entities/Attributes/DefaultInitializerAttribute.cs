using System;

namespace Stateflows.Entities.Attributes;

/// <summary>Marks a static method as the default entity-instance initializer.</summary>
[AttributeUsage(AttributeTargets.Method)]
public sealed class DefaultInitializerAttribute : Attribute;