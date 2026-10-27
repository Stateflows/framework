using Stateflows.Common.Registration.Interfaces;
using Stateflows.Extensions.FluentValidation;

namespace Stateflows
{
    /// <summary>Registers FluentValidation integration for event validation.</summary>
    public static class DependencyInjection
    {
        /// <summary>Adds support for resolving and executing FluentValidation validators for Stateflows events.</summary>
        /// <typeparam name="TBuilder">A Stateflows client builder that supports validator registration.</typeparam>
        /// <param name="builder">The Stateflows builder.</param>
        /// <returns>The same builder, for further Stateflows client configuration.</returns>
        public static TBuilder AddFluentValidation<TBuilder>(this TBuilder builder)
            where TBuilder : IStateflowsClientBuilder
        {
            builder.AddValidator<FluentValidator>();

            return builder;
        }
    }
}