using Stateflows.StateMachines.Registration.Interfaces.Base;

namespace Stateflows.StateMachines.Registration.Interfaces
{
    public interface IStateBuilder :
        IStateEvents<IStateBuilder>,
        IStateUtils<IStateBuilder>,
        IStateTransitions<IStateBuilder>,
        IStateSubmachine<IBehaviorStateBuilder>,
        IStateDoActivity<IBehaviorStateBuilder>,
        IStateDoAction<IBehaviorStateBuilder>
    { }

    public interface IBehaviorStateBuilder :
        IStateEvents<IBehaviorStateBuilder>,
        IStateUtils<IBehaviorStateBuilder>,
        IStateTransitions<IBehaviorStateBuilder>
    { }
    
    public interface IOverridenStateBuilder :
        IStateExtension<IOverridenStateBuilder>,
        IStateEvents<IOverridenStateBuilder>,
        IStateUtils<IOverridenStateBuilder>,
        IStateTransitions<IOverridenStateBuilder>,
        IStateTransitionsOverrides<IOverridenStateBuilder>,
        IStateComposition<IOverridenRegionalizedStateBuilder>,
        IStateOrthogonalization<IOverridenRegionalizedStateBuilder>,
        IStateSubmachine<IBehaviorOverridenStateBuilder>,
        IStateDoActivity<IBehaviorOverridenStateBuilder>,
        IStateDoAction<IBehaviorOverridenStateBuilder>
    { }
    
    public interface IOverridenRegionalizedStateBuilder :
        IStateExtension<IOverridenRegionalizedStateBuilder>,
        IStateEvents<IOverridenRegionalizedStateBuilder>,
        IStateUtils<IOverridenRegionalizedStateBuilder>,
        IStateTransitions<IOverridenRegionalizedStateBuilder>,
        IStateTransitionsOverrides<IOverridenRegionalizedStateBuilder>,
        IStateSubmachine<IBehaviorOverridenRegionalizedStateBuilder>,
        IStateDoActivity<IBehaviorOverridenRegionalizedStateBuilder>,
        IStateDoAction<IBehaviorOverridenRegionalizedStateBuilder>
    { }

    public interface IBehaviorOverridenStateBuilder :
        IStateExtension<IBehaviorOverridenStateBuilder>,
        IStateEvents<IBehaviorOverridenStateBuilder>,
        IStateUtils<IBehaviorOverridenStateBuilder>,
        IStateTransitions<IBehaviorOverridenStateBuilder>,
        IStateTransitionsOverrides<IBehaviorOverridenStateBuilder>,
        IStateComposition<IBehaviorOverridenRegionalizedStateBuilder>,
        IStateOrthogonalization<IBehaviorOverridenRegionalizedStateBuilder>
    { }

    public interface IBehaviorOverridenRegionalizedStateBuilder :
        IStateExtension<IBehaviorOverridenRegionalizedStateBuilder>,
        IStateEvents<IBehaviorOverridenRegionalizedStateBuilder>,
        IStateUtils<IBehaviorOverridenRegionalizedStateBuilder>,
        IStateTransitions<IBehaviorOverridenRegionalizedStateBuilder>,
        IStateTransitionsOverrides<IBehaviorOverridenRegionalizedStateBuilder>
    { }
}
