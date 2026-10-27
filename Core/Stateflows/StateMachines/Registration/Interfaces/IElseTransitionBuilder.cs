using Stateflows.StateMachines.Registration.Interfaces.Base;

namespace Stateflows.StateMachines.Registration.Interfaces
{
    /// <summary>Configures effects and targeting options for a fallback event-triggered transition.</summary>
    /// <typeparam name="TEvent">Trigger event payload type. Exception triggers also match derived exception types;
    /// ordinary event triggers use exact matching.</typeparam>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-transition" />
    /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-evaluation-of-transitions#else-transitions" />
    public interface IElseTransitionBuilder<TEvent> :
        ITargetedTransitionUtils<IElseTransitionBuilder<TEvent>>,
        ITriggeredTransitionUtils<IElseTransitionBuilder<TEvent>>,
        IEffect<TEvent, IElseTransitionBuilder<TEvent>>
    { }
    
    /// <summary>Extends the effects and target of an inherited fallback event-triggered transition.</summary>
    /// <typeparam name="TEvent">Trigger event payload type. Exception triggers also match derived exception types;
    /// ordinary event triggers use exact matching.</typeparam>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-transition" />
    /// <seealso href="https://github.com/Stateflows/framework/wiki/behaviors#definition-reuse-and-versions" />
    /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-evaluation-of-transitions#else-transitions" />
    public interface IOverridenElseTransitionBuilder<TEvent> :
        ITargetedTransitionUtils<IOverridenElseTransitionBuilder<TEvent>>,
        ITriggeredTransitionUtils<IOverridenElseTransitionBuilder<TEvent>>,
        IEffect<TEvent, IOverridenElseTransitionBuilder<TEvent>>
    { }
}
