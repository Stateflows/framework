using System;
using System.Collections.Generic;

namespace Stateflows.Entities.Attributes;

/// <summary>Marks a template method as an initializer. Without names the initializer is selected by its event parameter type;
/// a method whose parameter is <see cref="Stateflows.Payloads.Payload"/> lists the payload event names it handles.</summary>
/// <seealso href="https://github.com/Stateflows/framework/wiki/4.6.-Payloads" />
[AttributeUsage(AttributeTargets.Method)]
public sealed class InitializerAttribute : Attribute
{
    /// <summary>Creates the attribute.</summary>
    /// <param name="eventNames">Payload event names handled by the method; empty for CLR-typed methods.</param>
    public InitializerAttribute(params string[] eventNames)
    {
        EventNames = eventNames ?? [];
    }

    /// <summary>Gets the payload event names handled by the method; empty for CLR-typed methods.</summary>
    public IReadOnlyList<string> EventNames { get; }
}
