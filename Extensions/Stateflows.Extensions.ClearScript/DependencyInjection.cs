using Microsoft.ClearScript;
using Stateflows.Common.Registration.Interfaces;

namespace Stateflows
{
    public static class DependencyInjection
    {
        internal static Func<IServiceProvider, Task<IScriptEngine>>? EngineFactory;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="builder">The Stateflows builder.</param>
        /// <param name="engineFactory">ClearScript engine factory</param>
        /// <returns>The updated Stateflows builder.</returns>
        public static IStateflowsBuilder AddClearScript(this IStateflowsBuilder builder, Func<IServiceProvider, Task<IScriptEngine>> engineFactory)
        {
            EngineFactory = engineFactory;
            
            return builder;
        }
    }
}