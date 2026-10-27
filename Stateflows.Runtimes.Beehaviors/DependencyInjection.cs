using Beehaviors.Registration.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Stateflows.Common.Interfaces;
using Stateflows.Common.Registration.Interfaces;
using Stateflows.Storage;

namespace Stateflows;

/// <summary>Registers Stateflows behavior processing with the Beehaviors actor runtime.</summary>
public static class DependencyInjection
{
    /// <summary>Registers the runtime bee, storage bridge, behavior factory, and Stateflows definitions.</summary>
    /// <param name="builder">Beehaviors registration builder receiving the runtime bridge.</param>
    /// <param name="buildAction">Callback registering Stateflows behaviors and services.</param>
    /// <returns>The original Beehaviors builder for further runtime configuration.</returns>
    /// <remarks>The bridge processes each addressed behavior through a tenant-specific runtime bee.</remarks>
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
