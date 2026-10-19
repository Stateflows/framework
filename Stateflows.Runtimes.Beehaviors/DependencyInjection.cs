using Beehaviors.Registration.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Stateflows.Common.Interfaces;
using Stateflows.Common.Registration.Interfaces;
using Stateflows.Storage;

namespace Stateflows;

public static class DependencyInjection
{
    public static IBeehaviorsBuilder AddStateflows(
        this IBeehaviorsBuilder builder,
        Action<IStateflowsBuilder> buildAction
    )
    {
        builder
            .AddBee<BeeStateflowsState, StateflowsRuntimeBee>(StateflowsRuntimeBee.BeeName)
            .ConfigureServices(services =>
            {
                services
                    .AddSingleton<IStateflowsStorage, BeeStorageBridge>()
                    .AddTransient<IBehaviorFactory, BeeBehaviorFactory>()
                    .AddStateflows(buildAction);
            });

        return builder;
    }
}
