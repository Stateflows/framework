using System.Diagnostics;

namespace Stateflows.Activities.Registration.Interfaces.Base
{
    public interface ISendEventOverrides<out TReturn>
    {
        TReturn UseSendEventAction<TEvent>(string actionNodeName, OverridenSendEventActionBuildAction buildAction);
        
        [DebuggerHidden]
        TReturn UseSendEventAction<TEvent, TSendEventAction>(OverridenSendEventActionBuildAction buildAction)
            where TSendEventAction : class, ISendEventActionNode<TEvent>
            => UseSendEventAction<TEvent>(ActivityNode<TSendEventAction>.Name, buildAction);
    }
}
