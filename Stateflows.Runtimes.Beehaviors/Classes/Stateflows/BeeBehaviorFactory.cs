using Beehaviors.Registration.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Stateflows.Common;
using Stateflows.Common.Interfaces;

namespace Stateflows;

internal sealed class BeeBehaviorFactory(IServiceProvider serviceProvider) : IBehaviorFactory
{
    public IBehavior CreateBehavior(BehaviorId behaviorId)
        => new BeeBehavior(
            serviceProvider.GetRequiredService<IStateflowsTenantProvider>().GetCurrentTenantId(),
            behaviorId,
            serviceProvider.GetRequiredService<ILocalBeeLocator>(),
            serviceProvider.GetRequiredService<INotificationsHub>(),
            serviceProvider
        );
}
