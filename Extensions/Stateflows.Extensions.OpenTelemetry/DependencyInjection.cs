using Microsoft.Extensions.DependencyInjection;
using Stateflows.Actions;
using Stateflows.Activities;
using Stateflows.Common;
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
                        .AddObserver((serviceProvider, _) => serviceProvider.GetRequiredService<StateMachineTracer>())
                        .AddInterceptor((serviceProvider, _) => serviceProvider.GetRequiredService<StateMachineTracer>())
                        .AddExceptionHandler((serviceProvider, _) => serviceProvider.GetRequiredService<StateMachineTracer>())
                    )
                    .AddActivities(b => b
                        .AddObserver(serviceProvider => serviceProvider.GetRequiredService<ActivityTracer>())
                        .AddInterceptor(serviceProvider => serviceProvider.GetRequiredService<ActivityTracer>())
                        .AddExceptionHandler(serviceProvider => serviceProvider.GetRequiredService<ActivityTracer>())
                    )
                    .AddActions(b => b
                        .AddInterceptor(serviceProvider => serviceProvider.GetRequiredService<ActionTracer>())
                        .AddExceptionHandler(serviceProvider => serviceProvider.GetRequiredService<ActionTracer>())
                    )
                    .AddClientInterceptor<ClientInterceptor>()
                    .AddInterceptor<MetricsInterceptor>()
                ;

            builder.ServiceCollection
                .AddHostedService<StateflowsMeter>()
                .AddScoped<StateMachineTracer>()
                .AddScoped<ActivityTracer>()
                .AddScoped<ActionTracer>();
            
            builder.ServiceCollection.AddOpenTelemetry()
                .WithMetrics(b => b
                    .AddMeter(StateflowsMeter.Meter.Name)
                )
                .WithTracing(b => b
                    .AddSource(StateMachineTracer.Source.Name)
                );

            return result;
        }
    }
}