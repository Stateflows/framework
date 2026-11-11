using Stateflows.StateMachines.Registration.Interfaces.Base;

namespace Stateflows.StateMachines.Registration.Interfaces
{
    /// <summary>Configures a fallback internal transition that consumes an event without exiting or entering states.
    /// </summary>
    /// <typeparam name="TEvent">Trigger event payload type. Exception triggers also match derived exception types;
    /// ordinary event triggers use exact matching.</typeparam>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/5.4.2.4.3.-Internal-Transition" />
    /// <seealso href="https://github.com/Stateflows/framework/wiki/5.4.1.3.-Evaluation-of-Transitions#else-transitions" />
    public interface IElseInternalTransitionBuilder<TEvent> :
        ITriggeredTransitionUtils<IElseInternalTransitionBuilder<TEvent>>,
        IEffect<TEvent, IElseInternalTransitionBuilder<TEvent>>
    { }
    
    /// <summary>Extends effects on an inherited fallback internal transition.</summary>
    /// <typeparam name="TEvent">Trigger event payload type. Exception triggers also match derived exception types;
    /// ordinary event triggers use exact matching.</typeparam>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/5.4.2.4.3.-Internal-Transition" />
    /// <seealso href="https://github.com/Stateflows/framework/wiki/5.-Behaviors#definition-reuse-and-versions" />
    /// <seealso href="https://github.com/Stateflows/framework/wiki/5.4.1.3.-Evaluation-of-Transitions#else-transitions" />
    public interface IOverridenElseInternalTransitionBuilder<TEvent> :
        ITriggeredTransitionUtils<IOverridenElseInternalTransitionBuilder<TEvent>>,
        IEffect<TEvent, IOverridenElseInternalTransitionBuilder<TEvent>>
    { }
}
