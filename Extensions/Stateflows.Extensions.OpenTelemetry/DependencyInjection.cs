using Microsoft.Extensions.DependencyInjection;
using Stateflows.Activities;
using Stateflows.Common.Registration.Interfaces;
using Stateflows.StateMachines;

namespace Stateflows.Extensions.OpenTelemetry
{
    public static class DependencyInjection
    {
        public static IStateflowsBuilder AddOpenTelemetry(this IStateflowsBuilder builder)
        {
            var result = builder
                    .AddStateMachines(b => b
                        .AddObserver(serviceProvider => serviceProvider.GetRequiredService<StateMachineTracer>())
                        .AddInterceptor(serviceProvider => serviceProvider.GetRequiredService<StateMachineTracer>())
                        .AddExceptionHandler(serviceProvider => serviceProvider.GetRequiredService<StateMachineTracer>())
                    )
                    .AddActivities(b => b
                        .AddObserver(serviceProvider => serviceProvider.GetRequiredService<ActivityTracer>())
                        .AddInterceptor(serviceProvider => serviceProvider.GetRequiredService<ActivityTracer>())
                        .AddExceptionHandler(serviceProvider => serviceProvider.GetRequiredService<ActivityTracer>())
                    )
                    .AddClientInterceptor<ClientInterceptor>();
                ;

            builder.ServiceCollection
                .AddScoped<StateMachineTracer>()
                .AddScoped<ActivityTracer>();
            
            builder.ServiceCollection.AddOpenTelemetry()
                .WithTracing(b => b
                    .AddSource(StateMachineTracer.Source.Name)
                );

            return result;
        }
    }
}