using System;
using Stateflows.Common.Registration.Builders;
using Stateflows.Common.Registration.Interfaces;

namespace Stateflows.Events
{
    /// <summary>Registers structural event schemas with a Stateflows host.</summary>
    public static class EventsDependencyInjection
    {
        /// <summary>Configures structural events and request contracts during host registration.</summary>
        /// <param name="builder">The Stateflows host builder.</param>
        /// <param name="buildAction">Declares the structural event schemas.</param>
        /// <returns>The same builder for fluent configuration.</returns>
        public static IStateflowsBuilder AddEvents(this IStateflowsBuilder builder, EventsBuildAction buildAction)
        {
            ArgumentNullException.ThrowIfNull(buildAction);
            buildAction(((StateflowsBuilder)builder).Events);
            return builder;
        }
    }
}
