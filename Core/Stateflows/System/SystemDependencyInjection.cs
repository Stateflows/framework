using Microsoft.Extensions.DependencyInjection;
using Stateflows.Common.Extensions;
using Stateflows.Common.Interfaces;
using Stateflows.Common.Registration.Interfaces;
using Stateflows.System.Engine;
using Stateflows.System.EventHandlers;

namespace Stateflows.System
{
    internal static class SystemDependencyInjection
    {
        public static IStateflowsBuilder EnsureSystemServices(this IStateflowsBuilder stateflowsBuilder)
        {
            if (!stateflowsBuilder.ServiceCollection.IsServiceRegistered<ISystemEventHandler>())
            {
                stateflowsBuilder
                    .ServiceCollection
                    .AddTransient<ISystemEventHandler, AvailableBehaviorClassesHandler>()
                    .AddTransient<ISystemEventHandler, BehaviorInstancesHandler>()
                    .AddSingleton<IEventProcessor, Processor>()
                    .AddTransient<IBehaviorProvider, Provider>()
                    ;
            }

            return stateflowsBuilder;
        }
    }
}
