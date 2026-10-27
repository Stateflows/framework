using Microsoft.ClearScript;
using Stateflows.Common.Registration.Interfaces;

namespace Stateflows
{
    /// <summary>Registers the shared ClearScript engine factory used by script-backed actions, activities, and state machines.</summary>
    public static class DependencyInjection
    {
        internal static Func<IServiceProvider, Task<IScriptEngine>>? EngineFactory;

        /// <summary>
        /// <summary>Configures ClearScript support for Stateflows script-backed behavior elements.</summary>
        /// </summary>
        /// <param name="builder">The Stateflows builder.</param>
        /// <param name="engineFactory">Creates an engine for the service provider that will execute a script.</param>
        /// <returns>The same builder, for further Stateflows configuration.</returns>
        public static IStateflowsBuilder AddClearScript(this IStateflowsBuilder builder, Func<IServiceProvider, Task<IScriptEngine>> engineFactory)
        {
            EngineFactory = engineFactory;
            
            return builder;
        }
    }
}