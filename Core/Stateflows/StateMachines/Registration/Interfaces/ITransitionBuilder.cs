using System.Diagnostics;
using Stateflows.Common;
using Stateflows.Common.Interfaces;
using Stateflows.StateMachines.Registration.Builders;
using Stateflows.StateMachines.Registration.Interfaces.Base;

namespace Stateflows.StateMachines.Registration.Interfaces
{
    /// <summary>Configures an event-triggered transition's metadata, guard, effect, and target semantics.</summary>
    /// <typeparam name="TEvent">Trigger event payload type. Exception triggers also match derived exception types;
    /// ordinary event triggers use exact matching.</typeparam>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-transition" />
    public interface ITransitionBuilder<TEvent> :
        IElementMetadataBuilder<ITransitionBuilder<TEvent>>,
        ITriggeredTransitionUtils<ITransitionBuilder<TEvent>>,
        ITargetedTransitionUtils<ITransitionBuilder<TEvent>>,
        IEffect<TEvent, ITransitionBuilder<TEvent>>,
        IGuard<TEvent, ITransitionBuilder<TEvent>>;

    /// <summary>Extends an inherited transition, including narrowing its trigger to a compatible event type.</summary>
    /// <typeparam name="TEvent">Trigger event payload type. Exception triggers also match derived exception types;
    /// ordinary event triggers use exact matching.</typeparam>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-transition" />
    /// <seealso href="https://github.com/Stateflows/framework/wiki/behaviors#definition-reuse-and-versions" />
    public interface IOverridenTransitionBuilder<TEvent> :
        IElementMetadataBuilder<IOverridenTransitionBuilder<TEvent>>,
        ITriggeredTransitionUtils<IOverridenTransitionBuilder<TEvent>>,
        ITargetedTransitionUtils<IOverridenTransitionBuilder<TEvent>>,
        IEffect<TEvent, IOverridenTransitionBuilder<TEvent>>,
        IGuard<TEvent, IOverridenTransitionBuilder<TEvent>>
    {
        /// <summary>Replaces the inherited trigger with a compatible subtype while retaining the transition's other
        /// configuration.</summary>
        /// <typeparam name="TTrigger">Replacement trigger type assignable to the transition's current event type.
        /// </typeparam>
        /// <returns>A builder for the same transition with the narrowed payload type.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-transition" />
        [DebuggerHidden]
        IOverridenTransitionBuilder<TTrigger> ChangeTrigger<TTrigger>()
            where TTrigger : TEvent
        {
            var builder = (TransitionBuilder<TEvent>)this;
            builder.Edge.TriggerType = typeof(TTrigger);
            builder.Edge.StructuralEventName = null;
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

        /// <summary>Changes a compatible inherited transition to match an explicitly named structural event.</summary>
        /// <param name="eventName">Registered structural-event name to match, not the CLR wrapper type name. Must not be
        /// null, empty, or whitespace.</param>
        /// <returns>A builder for the same transition whose callbacks receive the structural payload.</returns>
        /// <exception cref="System.ArgumentException">The event name is null, empty, or whitespace.</exception>
        /// <exception cref="System.InvalidOperationException">The original trigger cannot accept a
        /// <see cref="StructuralEvent" /> payload.</exception>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/events#define-structural-events" />
        IOverridenTransitionBuilder<StructuralEvent> ChangeTrigger(string eventName)
        {
            System.ArgumentException.ThrowIfNullOrWhiteSpace(eventName);
            if (!typeof(TEvent).IsAssignableFrom(typeof(StructuralEvent)))
                throw new System.InvalidOperationException("A CLR-typed transition cannot be changed to an incompatible structural trigger.");
            var builder = (TransitionBuilder<TEvent>)this;
            var edge = builder.Edge;
            edge.StructuralEventName = eventName;
            edge.TriggerType = typeof(StructuralEvent);
            edge.Trigger = eventName;
            edge.DeclaredTriggerTypes = null;
            if (edge.VisitingTask != null)
            {
                var index = edge.Graph.VisitingTasks.IndexOf(edge.VisitingTask);
                edge.VisitingTask = visitor => visitor.TransitionAddedAsync(
                    edge.Graph.StateflowsBuilder.Events.GetEvent(eventName),
                    edge.Graph.Name, edge.Graph.Version, edge.SourceName, edge.TargetName, edge.IsElse);
                edge.Graph.VisitingTasks[index] = edge.VisitingTask;
            }
            return new TransitionBuilder<StructuralEvent>(edge);
        }
    }
}
