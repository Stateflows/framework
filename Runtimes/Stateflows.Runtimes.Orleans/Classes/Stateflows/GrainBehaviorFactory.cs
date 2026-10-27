using Microsoft.Extensions.DependencyInjection;
using Stateflows.Common;
using Stateflows.Common.Interfaces;
using Microsoft.Extensions.Logging;

namespace Stateflows;

public class GrainBehaviorFactory(IServiceProvider serviceProvider) : IBehaviorFactory
{
    public IBehavior CreateBehavior(BehaviorId behaviorId)
        => new GrainBehavior(
            serviceProvider.GetRequiredService<IStateflowsTenantProvider>().GetCurrentTenantId(),
            behaviorId,
            serviceProvider.GetRequiredService<IClusterClient>(),
            serviceProvider.GetRequiredService<ILogger<GrainBehavior>>()
        );
}