using System;
using Stateflows.Common.Registration.Builders;
using Stateflows.Common.Registration.Interfaces;

namespace Stateflows.Payloads
{
    /// <summary>Registers payload schemas with a Stateflows host.</summary>
    public static class PayloadsDependencyInjection
    {
        /// <summary>Configures payloads and request contracts during host registration.</summary>
        /// <param name="builder">The Stateflows host builder.</param>
        /// <param name="buildAction">Declares the payload schemas.</param>
        /// <returns>The same builder for fluent configuration.</returns>
        public static IStateflowsBuilder AddPayloads(this IStateflowsBuilder builder, PayloadsBuildAction buildAction)
        {
            ArgumentNullException.ThrowIfNull(buildAction);
            buildAction(((StateflowsBuilder)builder).Payloads);
            return builder;
        }
    }
}
