using Stateflows.Common.Registration.Interfaces;
using Stateflows.Extensions.UnionTypes;

namespace Stateflows;

/// <summary>Registers mapping support for C# union trigger types.</summary>
public static class UnionTypesDependencyInjection
{
    /// <summary>Allows state machines and activities to use a C# union as a set of trigger event types.</summary>
    /// <remarks>Call this before registering state machines or activities so their trigger types are mapped during registration.</remarks>
    /// <param name="builder">The Stateflows builder.</param>
    /// <returns>The updated Stateflows builder.</returns>
    public static IStateflowsBuilder AddUnionTypes(this IStateflowsBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        return builder.AddTypeMapper<UnionTypeMapper>();
    }
}
