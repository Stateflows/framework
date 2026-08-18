using System.Diagnostics;
using Stateflows.Activities.Registration.Builders;
using Stateflows.Common;

namespace Stateflows.Activities.Registration.Interfaces.Base
{
    public interface IAcceptEventOverrides<out TReturn>
    {
        TReturn UseAcceptEventAction<TEvent>(string actionNodeName, OverridenAcceptEventActionBuildAction<TEvent> buildAction);

        [DebuggerHidden]
        TReturn UseAcceptEventAction<TEvent>(OverridenAcceptEventActionBuildAction<TEvent> buildAction)
            => UseAcceptEventAction<TEvent>(AcceptEventActionNode<TEvent>.Name, buildAction);
        
        [DebuggerHidden]
        TReturn UseAcceptEventAction<TEvent, TAcceptEventAction>(OverridenAcceptEventActionBuildAction<TEvent, TAcceptEventAction> buildAction)
            where TAcceptEventAction : class, IAcceptEventActionNode<TEvent>
            => UseAcceptEventAction<TEvent>(ActivityNode<TAcceptEventAction>.Name,
                b =>
                {
                    var nodeBuilder = (NodeBuilder)b;
                    buildAction?.Invoke(new AcceptEventNodeBuilder<TEvent, TAcceptEventAction>(nodeBuilder.Node, nodeBuilder.ActivityBuilder));
                });

        TReturn UseTimeEventAction<TTimeEvent>(string actionNodeName, OverridenTimeEventNodeBuildAction buildAction)
            where TTimeEvent : TimeEvent, new();
        
        [DebuggerHidden]
        TReturn UseTimeEventAction<TTimeEvent>(OverridenTimeEventNodeBuildAction buildAction)
            where TTimeEvent : TimeEvent, new()
            => UseTimeEventAction<TTimeEvent>(TimeEventActionNode<TTimeEvent>.Name, buildAction);
        
        [DebuggerHidden]
        TReturn UseTimeEventAction<TTimeEvent, TTimeEventAction>(OverridenTimeEventNodeBuildAction<TTimeEventAction> buildAction)
            where TTimeEvent : TimeEvent, new()
            where TTimeEventAction : class, ITimeEventActionNode
            => UseTimeEventAction<TTimeEvent>(ActivityNode<TTimeEventAction>.Name, 
                b =>
                {
                    var nodeBuilder = (NodeBuilder)b;
                    buildAction?.Invoke(new TimeEventNodeBuilder<TTimeEventAction>(nodeBuilder.Node, nodeBuilder.ActivityBuilder));
                });
    }
}
