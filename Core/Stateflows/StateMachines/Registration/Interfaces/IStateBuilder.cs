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
        IStateComposition<IOverridenRegionalizedStateBuilder>,
        IStateOrthogonalization<IOverridenRegionalizedStateBuilder>,
        IStateSubmachine<IBehaviorOverridenStateBuilder>,
        IStateDoActivity<IBehaviorOverridenStateBuilder>
    { }
    
    public interface IOverridenRegionalizedStateBuilder :
        IStateEvents<IOverridenRegionalizedStateBuilder>,
        IStateUtils<IOverridenRegionalizedStateBuilder>,
        IStateTransitions<IOverridenRegionalizedStateBuilder>,
        IStateTransitionsOverrides<IOverridenRegionalizedStateBuilder>,
        IStateSubmachine<IBehaviorOverridenRegionalizedStateBuilder>,
        IStateDoActivity<IBehaviorOverridenRegionalizedStateBuilder>
    { }

    public interface IBehaviorOverridenStateBuilder :
        IStateEvents<IBehaviorOverridenStateBuilder>,
        IStateUtils<IBehaviorOverridenStateBuilder>,
        IStateTransitions<IBehaviorOverridenStateBuilder>,
        IStateTransitionsOverrides<IBehaviorOverridenStateBuilder>,
        IStateComposition<IBehaviorOverridenRegionalizedStateBuilder>,
        IStateOrthogonalization<IBehaviorOverridenRegionalizedStateBuilder>
    { }

    public interface IBehaviorOverridenRegionalizedStateBuilder :
        IStateEvents<IBehaviorOverridenRegionalizedStateBuilder>,
        IStateUtils<IBehaviorOverridenRegionalizedStateBuilder>,
        IStateTransitions<IBehaviorOverridenRegionalizedStateBuilder>,
        IStateTransitionsOverrides<IBehaviorOverridenRegionalizedStateBuilder>
    { }
}
