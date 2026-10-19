using Beehaviors.Interfaces;
using Stateflows.Common;

namespace Stateflows;

internal sealed class ProcessBehaviorEventRequest(
    string tenantId,
    BehaviorId behaviorId,
    EventHolder eventHolder
) : Beehaviors.Interfaces.IRequest<ProcessBehaviorEventResponse>
{
    public string TenantId { get; } = tenantId;

    public BehaviorId BehaviorId { get; } = behaviorId;

    public EventHolder EventHolder { get; } = eventHolder;
}
