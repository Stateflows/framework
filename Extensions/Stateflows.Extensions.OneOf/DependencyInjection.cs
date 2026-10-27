using Stateflows.Common.Registration.Interfaces;
using Stateflows.Extensions.OneOf;

namespace Stateflows
{
    /// <summary>Registers mapping support for OneOf trigger types.</summary>
    public static class DependencyInjection
    {
        /// <summary>Allows state machines and activities to use OneOf values as a set of trigger event types.</summary>
        /// <remarks>Call this before registering state machines or activities so their trigger types are mapped during registration.</remarks>
        /// <param name="builder">The Stateflows builder.</param>
        /// <returns>The updated Stateflows builder.</returns>
        public static IStateflowsBuilder AddOneOf(this IStateflowsBuilder builder)
            => builder.AddTypeMapper<OneOfTypeMapper>();
    }
}