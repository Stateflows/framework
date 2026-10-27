using System;

namespace Stateflows.Entities.Attributes;

/// <summary>Marks a static method as a mutation handler for its event parameter type.</summary>
[AttributeUsage(AttributeTargets.Method)]
public sealed class MutationAttribute : Attribute;