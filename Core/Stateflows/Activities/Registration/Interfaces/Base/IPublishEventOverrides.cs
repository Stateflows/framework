using System.Diagnostics;

namespace Stateflows.Activities.Registration.Interfaces.Base
{
    public interface IPublishEventOverrides<out TReturn>
    {
        TReturn UsePublishEventAction<TEvent>(string actionNodeName, OverridenPublishEventActionBuildAction buildAction);
        
        [DebuggerHidden]
        TReturn UsePublishEventAction<TEvent, TAcceptEventAction>(OverridenPublishEventActionBuildAction buildAction)
            where TAcceptEventAction : class, IAcceptEventActionNode<TEvent>
            => UsePublishEventAction<TEvent>(ActivityNode<TAcceptEventAction>.Name, buildAction);
    }
}
