using Microsoft.Extensions.DependencyInjection;
using Stateflows.Common.Interfaces;
using Stateflows.Common.Registration.Interfaces;

namespace Stateflows;

public static class DependencyInjection
{
    public static IStateflowsBuilder AddOrleansHosting(this IStateflowsBuilder builder)
    {
        builder.ServiceCollection
            .AddTransient<IBehaviorFactory, GrainBehaviorFactory>()
            .AddTransient<IStateflowsSubscriber, GrainSubscriber>();

        return builder;
    }
}