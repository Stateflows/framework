using Stateflows.Common.Classes;
using Stateflows.Common.Registration.Interfaces;

namespace Stateflows
{
    /// <summary>Configures client-wide Stateflows naming conventions.</summary>
    public static class IStateflowsClientBuilderExtensions
    {
        /// <summary>Replaces the process-wide selection of element categories that use fully qualified CLR names.</summary>
        /// <param name="stateflowsBuilder">The client builder used for configuration.</param>
        /// <param name="typedElements">The categories that should use full names; other categories use short names.</param>
        /// <returns>The supplied builder for fluent registration.</returns>
        /// <remarks>This updates a shared static setting rather than a setting local to the service collection. Configure it before resolving element names.</remarks>
        public static IStateflowsClientBuilder UseFullNamesFor(this IStateflowsClientBuilder stateflowsBuilder, TypedElements typedElements)
        {
            StateflowsSettings.FullNames = typedElements;

            return stateflowsBuilder;
        }
    }
}
