using Stateflows.Activities.Registration.Interfaces.Base;

namespace Stateflows.Activities.Registration.Interfaces
{
    /// <summary>Configures the control flow leaving a send-event action node.</summary>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/activity-send-event-action-node" />
    public interface ISendEventActionBuilder : IControlFlowBase<ISendEventActionBuilder>;
    
    /// <summary>Provides fluent APIs for extending elements inherited from a reusable Activity definition.</summary>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/activity-send-event-action-node" />
    /// <seealso href="https://github.com/Stateflows/framework/wiki/behaviors#definition-reuse-and-versions" />
    public interface IOverridenSendEventActionBuilder :
        IControlFlowBase<IOverridenSendEventActionBuilder>,
        IOverridenControlFlowBase<IOverridenSendEventActionBuilder>;
}
