using System.Diagnostics;

namespace Stateflows.Activities.Registration.Interfaces.Base
{
    public interface IPublishEventOverrides<out TReturn>
    {
        TReturn UsePublishEventAction<TEvent>(string actionNodeName, OverridenPublishEventActionBuildAction buildAction);

        [DebuggerHidden]
        TReturn UsePublishEventAction<TEvent, TPublishEventAction>(OverridenPublishEventActionBuildAction buildAction)
            where TPublishEventAction : class, IPublishEventActionNode<TEvent>
            => UsePublishEventAction<TEvent>(ActivityNode<TPublishEventAction>.Name, buildAction);
    }
}
