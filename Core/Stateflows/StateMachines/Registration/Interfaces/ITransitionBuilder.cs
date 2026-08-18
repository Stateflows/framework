using System.Diagnostics;
using Stateflows.Common;
using Stateflows.StateMachines.Registration.Builders;
using Stateflows.StateMachines.Registration.Interfaces.Base;

namespace Stateflows.StateMachines.Registration.Interfaces
{
    public interface ITransitionBuilder<TEvent> :
        ITriggeredTransitionUtils<ITransitionBuilder<TEvent>>,
        ITargetedTransitionUtils<ITransitionBuilder<TEvent>>,
        IEffect<TEvent, ITransitionBuilder<TEvent>>,
        IGuard<TEvent, ITransitionBuilder<TEvent>>;

    public interface IOverridenTransitionBuilder<TEvent> :
        ITriggeredTransitionUtils<IOverridenTransitionBuilder<TEvent>>,
        ITargetedTransitionUtils<IOverridenTransitionBuilder<TEvent>>,
        IEffect<TEvent, IOverridenTransitionBuilder<TEvent>>,
        IGuard<TEvent, IOverridenTransitionBuilder<TEvent>>
    {
        [DebuggerHidden]
        IOverridenTransitionBuilder<TTrigger> ChangeTrigger<TTrigger>()
            where TTrigger : TEvent
        {
            var builder = (TransitionBuilder<TEvent>)this;
            builder.Edge.TriggerType = typeof(TTrigger);
            builder.Edge.Trigger = typeof(TTrigger).GetEventName();

            var result = new TransitionBuilder<TTrigger>(builder.Edge);
            
            if (builder.Edge.VisitingTask is not null)
            {
                var graph = builder.Edge.Graph;
                var index = graph.VisitingTasks.IndexOf(builder.Edge.VisitingTask);
                
                builder.Edge.VisitingTask = visitor => visitor.TransitionAddedAsync<TTrigger>(
                    graph.Name,
                    graph.Version,
                    builder.Edge.SourceName,
                    builder.Edge.TargetName == Constants.DefaultTransitionTarget
                        ? builder.Edge.TargetName
                        : null
                );
                
                graph.VisitingTasks[index] = builder.Edge.VisitingTask;
            }

            return result;
        }
    }
}
