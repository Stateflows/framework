using Microsoft.Extensions.DependencyInjection;
using Stateflows.Common.Interfaces;
using Stateflows.Common.Registration.Interfaces;
using Stateflows.Filters;

namespace Stateflows;

public static class DependencyInjection
{
    public static ISiloBuilder AddStateflows(this ISiloBuilder builder, Action<IStateflowsBuilder> buildAction)
    {
        builder
            .AddIncomingGrainCallFilter<ValidationFilter>()
            .AddIncomingGrainCallFilter<ResourceFilter>()
            .Services
            .AddTransient<IBehaviorFactory, GrainBehaviorFactory>()
            .AddTransient<IStateflowsSubscriber, GrainSubscriber>()
            .AddStateflows(buildAction);
            ;

        return builder;
    }
}