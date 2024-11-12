using Stateflows.StateMachines.Registration.Interfaces.Base;

namespace Stateflows.StateMachines.Registration.Interfaces
{
    public interface IStateBuilder :
        IStateEvents<IStateBuilder>,
        IStateUtils<IStateBuilder>,
        IStateTransitions<IStateBuilder>,
        IStateSubmachine<IBehaviorStateBuilder>,
        IStateDoActivity<IBehaviorStateBuilder>
    { }

    public interface IBehaviorStateBuilder :
        IStateEvents<IBehaviorStateBuilder>,
        IStateUtils<IBehaviorStateBuilder>,
        IStateTransitions<IBehaviorStateBuilder>
    { }
    
    public interface IOverridenStateBuilder :
        IStateEvents<IOverridenStateBuilder>,
        IStateUtils<IOverridenStateBuilder>,
        IStateTransitions<IOverridenStateBuilder>,
        IStateTransitionsOverrides<IOverridenStateBuilder>,
        IStateComposition<IOverridenCompositedStateBuilder>,
        IStateSubmachine<IBehaviorOverridenStateBuilder>,
        IStateDoActivity<IBehaviorOverridenStateBuilder>
    { }
    
    public interface IOverridenCompositedStateBuilder :
        IStateEvents<IOverridenCompositedStateBuilder>,
        IStateUtils<IOverridenCompositedStateBuilder>,
        IStateTransitions<IOverridenCompositedStateBuilder>,
        IStateTransitionsOverrides<IOverridenCompositedStateBuilder>,
        IStateSubmachine<IBehaviorOverridenCompositedStateBuilder>,
        IStateDoActivity<IBehaviorOverridenCompositedStateBuilder>
    { }

    public interface IBehaviorOverridenStateBuilder :
        IStateEvents<IBehaviorOverridenStateBuilder>,
        IStateUtils<IBehaviorOverridenStateBuilder>,
        IStateTransitions<IBehaviorOverridenStateBuilder>,
        IStateTransitionsOverrides<IBehaviorOverridenStateBuilder>,
        IStateComposition<IBehaviorOverridenCompositedStateBuilder>
    { }

    public interface IBehaviorOverridenCompositedStateBuilder :
        IStateEvents<IBehaviorOverridenCompositedStateBuilder>,
        IStateUtils<IBehaviorOverridenCompositedStateBuilder>,
        IStateTransitions<IBehaviorOverridenCompositedStateBuilder>,
        IStateTransitionsOverrides<IBehaviorOverridenCompositedStateBuilder>
    { }
}
