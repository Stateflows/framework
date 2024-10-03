using Stateflows.Common.Registration.Interfaces;
using Stateflows.Extensions.OneOf;

namespace Stateflows
{
    public static class DependencyInjection
    {
        /// <summary>
        /// Adds the support for multiple triggers using OneOf library.<br/>
        /// This method must be called before all state machine and activity registrations.
        /// </summary>
        /// <param name="builder">The Stateflows builder.</param>
        /// <returns>The updated Stateflows builder.</returns>
        public static IStateflowsBuilder AddOneOf(this IStateflowsBuilder builder)
            => builder.AddTypeMapper<OneOfTypeMapper>();
    }
}