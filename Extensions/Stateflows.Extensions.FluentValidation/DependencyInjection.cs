using Stateflows.Common.Registration.Interfaces;
using Stateflows.Extensions.FluentValidation;

namespace Stateflows
{
    public static class DependencyInjection
    {
        /// <summary>
        /// Adds the support for FluentValidation of events.<br/>
        /// </summary>
        /// <param name="builder">The Stateflows builder.</param>
        /// <returns>The updated Stateflows builder.</returns>
        public static TBuilder AddFluentValidation<TBuilder>(this TBuilder builder)
            where TBuilder : IStateflowsClientBuilder
        {
            builder.AddValidator<FluentValidator>();

            return builder;
        }
    }
}