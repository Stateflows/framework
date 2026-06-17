using System.Diagnostics;

namespace Stateflows.Activities.Registration.Interfaces.Base
{
    public interface ISendEventOverrides<out TReturn>
    {
        TReturn UseSendEventAction<TEvent>(string actionNodeName, OverridenSendEventActionBuildAction buildAction);
        
        [DebuggerHidden]
        TReturn UseSendEventAction<TEvent, TAcceptEventAction>(OverridenSendEventActionBuildAction buildAction)
            where TAcceptEventAction : class, IAcceptEventActionNode<TEvent>
            => UseSendEventAction<TEvent>(ActivityNode<TAcceptEventAction>.Name, buildAction);
    }
}
