using Stateflows.Common.Registration.Interfaces;

namespace Stateflows
{
    /// <summary>Configures naming conventions through a Stateflows host builder.</summary>
    public static class IStateflowsBuilderExtensions
    {
        /// <summary>Sets the process-wide categories that use fully qualified CLR names.</summary>
        /// <param name="stateflowsBuilder">The host builder used for configuration.</param>
        /// <param name="typedElements">The categories that use full names; unselected categories use short names.</param>
        /// <returns>The supplied host builder for fluent registration.</returns>
        /// <remarks>The selection replaces the shared client naming setting and should be configured before resolving element names.</remarks>
        public static IStateflowsBuilder UseFullNamesFor(this IStateflowsBuilder stateflowsBuilder, TypedElements typedElements)
        {
            (stateflowsBuilder as IStateflowsClientBuilder).UseFullNamesFor(typedElements);

            return stateflowsBuilder;
        }
    }
}
