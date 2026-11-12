using Stateflows.Activities.Registration.Interfaces.Base;
using Stateflows.Entities.Enums;

namespace Stateflows.Activities.Registration.Interfaces;

/// <summary>Configures publication settings and control flow for a publish-event action node.</summary>
/// <seealso href="https://github.com/Stateflows/framework/wiki/5.5.2.1.12.-Publish-Event-Action-Node" />
public interface IPublishEventActionBuilder : IControlFlowBase<IPublishEventActionBuilder>
{
    /// <summary>Sets the behaviors on whose behalf notifications are published.</summary>
    IPublishEventActionBuilder SetPublishScope(PublishScope scope);
    /// <summary>Sets the notification lifetime in seconds; zero disables history and subscription retries unless retained.</summary>
    IPublishEventActionBuilder SetTimeToLive(int secondsToLive);
    /// <summary>Sets whether the latest notification of each type is retained.</summary>
    IPublishEventActionBuilder SetRetain(bool retain);
    /// <summary>Sets whether publication bypasses the current turn's commit.</summary>
    IPublishEventActionBuilder SetImmediatePublish(bool immediatePublish);
}

/// <summary>Provides fluent APIs for extending elements inherited from a reusable Activity definition.</summary>
/// <seealso href="https://github.com/Stateflows/framework/wiki/5.5.2.1.12.-Publish-Event-Action-Node" />
/// <seealso href="https://github.com/Stateflows/framework/wiki/5.-Behaviors#definition-reuse-and-versions" />
public interface IOverridenPublishEventActionBuilder :
    IControlFlowBase<IOverridenPublishEventActionBuilder>,
    IOverridenControlFlowBase<IOverridenPublishEventActionBuilder>;
