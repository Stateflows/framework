using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Trace;
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
                        .AddObserver(serviceProvider => serviceProvider.GetRequiredService<StateMachineTracer>())
                        .AddInterceptor(serviceProvider => serviceProvider.GetRequiredService<StateMachineTracer>())
                        .AddExceptionHandler(serviceProvider => serviceProvider.GetRequiredService<StateMachineTracer>())
                    )
                    .AddClientInterceptor<ClientInterceptor>();
                ;

            builder.ServiceCollection
                .AddScoped<StateMachineTracer>();
            
            builder.ServiceCollection.AddOpenTelemetry()
                .WithTracing(b => b
                    .AddSource(StateMachineTracer.Source.Name)
                );

            return result;
        }
    }
}