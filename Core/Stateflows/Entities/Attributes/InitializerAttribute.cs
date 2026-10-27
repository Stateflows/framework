using System;

namespace Stateflows.Entities.Attributes;

/// <summary>Marks a static method as an event-triggered entity initializer.</summary>
[AttributeUsage(AttributeTargets.Method)]
public sealed class InitializerAttribute : Attribute;