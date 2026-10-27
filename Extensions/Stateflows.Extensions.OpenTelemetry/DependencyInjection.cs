using Microsoft.Extensions.DependencyInjection;
using Stateflows.Common.Registration.Interfaces;
using Stateflows.Actions;
using Stateflows.Activities;
using Stateflows.Extensions.OpenTelemetry;
using Stateflows.StateMachines;
using Stateflows.Entities;

namespace Stateflows
{
    /// <summary>Registers Stateflows tracing and metrics instrumentation with OpenTelemetry.</summary>
    public static class DependencyInjection
    {
        /// <summary>Adds Stateflows behavior tracers, client interception, and execution metrics.</summary>
        /// <param name="builder">The Stateflows builder whose behaviors and services will be instrumented.</param>
        /// <returns>The same builder, for further Stateflows configuration.</returns>
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
                    .AddEntities(b => b
                        .AddObserver((serviceProvider, _) => serviceProvider.GetRequiredService<EntityTracer>())
                        .AddInterceptor((serviceProvider, _) => serviceProvider.GetRequiredService<EntityTracer>())
                        .AddExceptionHandler((serviceProvider, _) => serviceProvider.GetRequiredService<EntityTracer>())
                    )
                    .AddClientInterceptor<ClientInterceptor>()
                    .AddInterceptor<MetricsInterceptor>()
                ;

            builder.ServiceCollection
                .AddSingleton<StateflowsMeter>()
                .AddHostedService(provider => provider.GetRequiredService<StateflowsMeter>())
                .AddScoped<StateMachineTracer>()
                .AddScoped<ActivityTracer>()
                .AddScoped<ActionTracer>()
                .AddScoped<EntityTracer>();
            
            builder.ServiceCollection.AddOpenTelemetry()
                .WithMetrics(b => b
                    .AddMeter(StateflowsMeter.MeterName, StateflowsMeter.OrleansMeterName)
                )
                .WithTracing(b => b
                    .AddSource(StateMachineTracer.Source.Name)
                );

            return result;
        }
    }
}