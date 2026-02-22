using Microsoft.Extensions.DependencyInjection;
using Stateflows.Common;
using Stateflows.Common.Interfaces;

namespace Stateflows;

public class GrainBehaviorFactory(IServiceProvider serviceProvider) : IBehaviorFactory
{
    public IBehavior CreateBehavior(BehaviorId behaviorId)
        => new GrainBehavior(
            serviceProvider.GetRequiredService<IStateflowsTenantProvider>().GetCurrentTenantIdAsync().GetAwaiter().GetResult(),
            behaviorId,
            serviceProvider.GetRequiredService<IClusterClient>()
        );
}