using Stateflows.Common.Interfaces;
using Stateflows.StateMachines.Registration.Interfaces.Base;

namespace Stateflows.StateMachines.Registration.Interfaces
{
    /// <summary>Configures guards and effects for a transition that preserves the active state configuration.</summary>
    /// <typeparam name="TEvent">Trigger event payload type. Exception triggers also match derived exception types;
    /// ordinary event triggers use exact matching.</typeparam>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-internal-transition" />
    public interface IInternalTransitionBuilder<TEvent> :
        IElementMetadataBuilder<IInternalTransitionBuilder<TEvent>>,
        ITriggeredTransitionUtils<IInternalTransitionBuilder<TEvent>>,
        IEffect<TEvent, IInternalTransitionBuilder<TEvent>>,
        IGuard<TEvent, IInternalTransitionBuilder<TEvent>>;
    
    /// <summary>Extends guards and effects of an inherited internal transition.</summary>
    /// <typeparam name="TEvent">Trigger event payload type. Exception triggers also match derived exception types;
    /// ordinary event triggers use exact matching.</typeparam>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-internal-transition" />
    /// <seealso href="https://github.com/Stateflows/framework/wiki/behaviors#definition-reuse-and-versions" />
    public interface IOverridenInternalTransitionBuilder<TEvent> :
        IElementMetadataBuilder<IOverridenInternalTransitionBuilder<TEvent>>,
        ITriggeredTransitionUtils<IOverridenInternalTransitionBuilder<TEvent>>,
        IEffect<TEvent, IOverridenInternalTransitionBuilder<TEvent>>,
        IGuard<TEvent, IOverridenInternalTransitionBuilder<TEvent>>;
}
