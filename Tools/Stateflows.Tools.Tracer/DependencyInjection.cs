using Microsoft.Extensions.DependencyInjection;
using Stateflows.Common.Registration.Interfaces;
using Stateflows.Tools.Tracer.Classes;
using Stateflows.Activities;
using Stateflows.StateMachines;

namespace Stateflows.Tools.Tracer
{
    public static class DependencyInjection
    {
        public static IStateflowsBuilder AddTracer(this IStateflowsBuilder builder, string storageDirectory)
        {
            builder
                .AddStateMachines(b => b
                    .AddInterceptor(serviceCollection => serviceCollection.GetRequiredService<StateMachines.Classes.Tracer>())
                )
                .AddActivities(b => b
                    .AddInterceptor(serviceCollection => serviceCollection.GetRequiredService<Activities.Classes.Tracer>())
                )
                .ServiceCollection
                .AddSingleton<TraceRepository>(serviceProvider => new TraceRepository(storageDirectory))
                .AddHostedService(serviceProvider => serviceProvider.GetRequiredService<TraceRepository>())
                .AddScoped<StateMachines.Classes.Tracer>()
                .AddScoped<IStateMachineInspector>(serviceCollection => serviceCollection.GetRequiredService<StateMachines.Classes.Tracer>())
                .AddScoped<Activities.Classes.Tracer>()
                .AddScoped<IActivityInspector>(serviceCollection => serviceCollection.GetRequiredService<Activities.Classes.Tracer>())
                ;

            return builder;
        }
    }
}
