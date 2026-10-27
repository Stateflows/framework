using Stateflows.Activities.Registration.Interfaces.Base;

namespace Stateflows.Activities.Registration.Interfaces;

/// <summary>Configures the control flow leaving a publish-event action node.</summary>
/// <seealso href="https://github.com/Stateflows/framework/wiki/activity-publish-event-action-node" />
public interface IPublishEventActionBuilder : IControlFlowBase<IPublishEventActionBuilder>;

/// <summary>Provides fluent APIs for extending elements inherited from a reusable Activity definition.</summary>
/// <seealso href="https://github.com/Stateflows/framework/wiki/activity-publish-event-action-node" />
/// <seealso href="https://github.com/Stateflows/framework/wiki/behaviors#definition-reuse-and-versions" />
public interface IOverridenPublishEventActionBuilder :
    IControlFlowBase<IOverridenPublishEventActionBuilder>,
    IOverridenControlFlowBase<IOverridenPublishEventActionBuilder>;
