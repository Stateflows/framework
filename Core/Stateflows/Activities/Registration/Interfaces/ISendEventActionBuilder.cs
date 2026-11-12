using Stateflows.Activities.Registration.Interfaces.Base;

namespace Stateflows.Activities.Registration.Interfaces
{
    /// <summary>Configures the control flow leaving a send-event action node.</summary>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/5.5.2.1.11.-Send-Event-Action-Node" />
    public interface ISendEventActionBuilder : IControlFlowBase<ISendEventActionBuilder>;
    
    /// <summary>Provides fluent APIs for extending elements inherited from a reusable Activity definition.</summary>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/5.5.2.1.11.-Send-Event-Action-Node" />
    /// <seealso href="https://github.com/Stateflows/framework/wiki/5.-Behaviors#definition-reuse-and-versions" />
    public interface IOverridenSendEventActionBuilder :
        IControlFlowBase<IOverridenSendEventActionBuilder>,
        IOverridenControlFlowBase<IOverridenSendEventActionBuilder>;
}
