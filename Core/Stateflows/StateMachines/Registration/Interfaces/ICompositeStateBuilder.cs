using Stateflows.StateMachines.Registration.Interfaces.Base;

namespace Stateflows.StateMachines.Registration.Interfaces
{
    public interface ICompositeStateBuilder :
        ICompositeStateExtension<ICompositeStateBuilder>,
        IStateEvents<ICompositeStateBuilder>,
        IStateUtils<ICompositeStateBuilder>,
        ICompositeStateTypedEvents<ICompositeStateBuilder>,
        IStateTransitions<ICompositeStateBuilder>,
        IStateMachineInitial<IInitializedCompositeStateBuilder>,
        IStateMachineElements<IInitializedCompositeStateBuilder>,
        IStateHistory<ICompositeStateBuilder>,
        IStateDoActivity<IBehaviorCompositeStateBuilder>,
        IStateDoAction<IBehaviorCompositeStateBuilder>;
    
    public interface IInitializedCompositeStateBuilder :
        ICompositeStateExtension<IInitializedCompositeStateBuilder>,
        IStateEvents<IInitializedCompositeStateBuilder>,
        IStateUtils<IInitializedCompositeStateBuilder>,
        ICompositeStateTypedEvents<IInitializedCompositeStateBuilder>,
        IStateTransitions<IInitializedCompositeStateBuilder>,
        IStateMachineElements<IInitializedCompositeStateBuilder>,
        IStateHistory<IInitializedCompositeStateBuilder>,
        IStateMachineFinal<IFinalizedCompositeStateBuilder>,
        IStateDoActivity<IBehaviorInitializedCompositeStateBuilder>,
        IStateDoAction<IBehaviorInitializedCompositeStateBuilder>;

    public interface IFinalizedCompositeStateBuilder :
        ICompositeStateExtension<IFinalizedCompositeStateBuilder>,
        IStateEvents<IFinalizedCompositeStateBuilder>,
        IStateUtils<IFinalizedCompositeStateBuilder>,
        ICompositeStateTypedEvents<IFinalizedCompositeStateBuilder>,
        IStateTransitions<IFinalizedCompositeStateBuilder>,
        IStateDoActivity<IBehaviorFinalizedCompositeStateBuilder>,
        IStateDoAction<IBehaviorFinalizedCompositeStateBuilder>;

    public interface IOverridenCompositeStateBuilder :
        ICompositeStateExtension<IOverridenCompositeStateBuilder>,
        IStateEvents<IOverridenCompositeStateBuilder>,
        IStateUtils<IOverridenCompositeStateBuilder>,
        IStateUtilsOverrides<IOverridenCompositeStateBuilder>,
        ICompositeStateTypedEvents<IOverridenCompositeStateBuilder>,
        IStateTransitions<IOverridenCompositeStateBuilder>,
        IStateTransitionsOverrides<IOverridenCompositeStateBuilder>,
        IStateMachineElements<IOverridenCompositeStateBuilder>,
        IStateHistory<IOverridenCompositeStateBuilder>,
        IStateMachineOverrides<IOverridenCompositeStateBuilder>,
        IStateOrthogonalization<IOverridenRegionalizedCompositeStateBuilder>,
        IStateMachineFinal<IFinalizedOverridenCompositeStateBuilder>,
        IStateDoActivity<IBehaviorOverridenCompositeStateBuilder>,
        IStateDoAction<IBehaviorOverridenCompositeStateBuilder>;

    public interface IOverridenRegionalizedCompositeStateBuilder :
        ICompositeStateExtension<IOverridenRegionalizedCompositeStateBuilder>,
        IStateEvents<IOverridenRegionalizedCompositeStateBuilder>,
        IStateUtils<IOverridenRegionalizedCompositeStateBuilder>,
        IStateUtilsOverrides<IOverridenRegionalizedCompositeStateBuilder>,
        ICompositeStateTypedEvents<IOverridenRegionalizedCompositeStateBuilder>,
        IStateTransitions<IOverridenRegionalizedCompositeStateBuilder>,
        IStateTransitionsOverrides<IOverridenRegionalizedCompositeStateBuilder>,
        IStateMachineElements<IOverridenRegionalizedCompositeStateBuilder>,
        IStateHistory<IOverridenRegionalizedCompositeStateBuilder>,
        IStateMachineOverrides<IOverridenRegionalizedCompositeStateBuilder>,
        IStateMachineFinal<IFinalizedOverridenRegionalizedCompositeStateBuilder>,
        IStateDoActivity<IBehaviorOverridenRegionalizedCompositeStateBuilder>,
        IStateDoAction<IBehaviorOverridenRegionalizedCompositeStateBuilder>;

    public interface IFinalizedOverridenCompositeStateBuilder :
        ICompositeStateExtension<IFinalizedOverridenCompositeStateBuilder>,
        IStateEvents<IFinalizedOverridenCompositeStateBuilder>,
        IStateUtils<IFinalizedOverridenCompositeStateBuilder>,
        IStateUtilsOverrides<IFinalizedOverridenCompositeStateBuilder>,
        ICompositeStateTypedEvents<IFinalizedOverridenCompositeStateBuilder>,
        IStateTransitions<IFinalizedOverridenCompositeStateBuilder>,
        IStateTransitionsOverrides<IFinalizedOverridenCompositeStateBuilder>,
        IStateMachineOverrides<IFinalizedOverridenCompositeStateBuilder>,
        IStateOrthogonalization<IFinalizedOverridenRegionalizedCompositeStateBuilder>,
        IStateDoActivity<IBehaviorFinalizedOverridenCompositeStateBuilder>,
        IStateDoAction<IBehaviorFinalizedOverridenCompositeStateBuilder>;

    public interface IFinalizedOverridenRegionalizedCompositeStateBuilder :
        ICompositeStateExtension<IFinalizedOverridenRegionalizedCompositeStateBuilder>,
        IStateEvents<IFinalizedOverridenRegionalizedCompositeStateBuilder>,
        IStateUtils<IFinalizedOverridenRegionalizedCompositeStateBuilder>,
        IStateUtilsOverrides<IFinalizedOverridenRegionalizedCompositeStateBuilder>,
        ICompositeStateTypedEvents<IFinalizedOverridenRegionalizedCompositeStateBuilder>,
        IStateMachineOverrides<IFinalizedOverridenRegionalizedCompositeStateBuilder>,
        IStateTransitions<IFinalizedOverridenRegionalizedCompositeStateBuilder>,
        IStateTransitionsOverrides<IFinalizedOverridenRegionalizedCompositeStateBuilder>,
        IStateDoActivity<IBehaviorFinalizedOverridenRegionalizedCompositeStateBuilder>,
        IStateDoAction<IBehaviorFinalizedOverridenRegionalizedCompositeStateBuilder>;
    

    public interface IBehaviorCompositeStateBuilder :
        ICompositeStateExtension<IBehaviorCompositeStateBuilder>,
        IStateEvents<IBehaviorCompositeStateBuilder>,
        IStateUtils<IBehaviorCompositeStateBuilder>,
        ICompositeStateTypedEvents<IBehaviorCompositeStateBuilder>,
        IStateTransitions<IBehaviorCompositeStateBuilder>,
        IStateMachineInitial<IBehaviorInitializedCompositeStateBuilder>,
        IStateMachineElements<IBehaviorInitializedCompositeStateBuilder>,
        IStateHistory<IBehaviorCompositeStateBuilder>;
    
    public interface IBehaviorInitializedCompositeStateBuilder :
        ICompositeStateExtension<IBehaviorInitializedCompositeStateBuilder>,
        IStateEvents<IBehaviorInitializedCompositeStateBuilder>,
        IStateUtils<IBehaviorInitializedCompositeStateBuilder>,
        ICompositeStateTypedEvents<IBehaviorInitializedCompositeStateBuilder>,
        IStateTransitions<IBehaviorInitializedCompositeStateBuilder>,
        IStateMachineElements<IBehaviorInitializedCompositeStateBuilder>,
        IStateHistory<IBehaviorInitializedCompositeStateBuilder>,
        IStateMachineFinal<IBehaviorFinalizedCompositeStateBuilder>;

    public interface IBehaviorFinalizedCompositeStateBuilder :
        ICompositeStateExtension<IBehaviorFinalizedCompositeStateBuilder>,
        IStateEvents<IBehaviorFinalizedCompositeStateBuilder>,
        IStateUtils<IBehaviorFinalizedCompositeStateBuilder>,
        ICompositeStateTypedEvents<IBehaviorFinalizedCompositeStateBuilder>,
        IStateTransitions<IBehaviorFinalizedCompositeStateBuilder>;

    public interface IBehaviorOverridenCompositeStateBuilder :
        ICompositeStateExtension<IBehaviorOverridenCompositeStateBuilder>,
        IStateEvents<IBehaviorOverridenCompositeStateBuilder>,
        IStateUtils<IBehaviorOverridenCompositeStateBuilder>,
        IStateUtilsOverrides<IBehaviorOverridenCompositeStateBuilder>,
        ICompositeStateTypedEvents<IBehaviorOverridenCompositeStateBuilder>,
        IStateTransitions<IBehaviorOverridenCompositeStateBuilder>,
        IStateTransitionsOverrides<IBehaviorOverridenCompositeStateBuilder>,
        IStateMachineElements<IBehaviorOverridenCompositeStateBuilder>,
        IStateHistory<IBehaviorOverridenCompositeStateBuilder>,
        IStateMachineOverrides<IBehaviorOverridenCompositeStateBuilder>,
        IStateOrthogonalization<IBehaviorOverridenRegionalizedCompositeStateBuilder>,
        IStateMachineFinal<IBehaviorFinalizedOverridenCompositeStateBuilder>;

    public interface IBehaviorOverridenRegionalizedCompositeStateBuilder :
        ICompositeStateExtension<IBehaviorOverridenRegionalizedCompositeStateBuilder>,
        IStateEvents<IBehaviorOverridenRegionalizedCompositeStateBuilder>,
        IStateUtils<IBehaviorOverridenRegionalizedCompositeStateBuilder>,
        IStateUtilsOverrides<IBehaviorOverridenRegionalizedCompositeStateBuilder>,
        ICompositeStateTypedEvents<IBehaviorOverridenRegionalizedCompositeStateBuilder>,
        IStateTransitions<IBehaviorOverridenRegionalizedCompositeStateBuilder>,
        IStateTransitionsOverrides<IBehaviorOverridenRegionalizedCompositeStateBuilder>,
        IStateMachineElements<IBehaviorOverridenRegionalizedCompositeStateBuilder>,
        IStateHistory<IBehaviorOverridenRegionalizedCompositeStateBuilder>,
        IStateMachineOverrides<IBehaviorOverridenRegionalizedCompositeStateBuilder>,
        IStateMachineFinal<IBehaviorFinalizedOverridenRegionalizedCompositeStateBuilder>;

    public interface IBehaviorFinalizedOverridenCompositeStateBuilder :
        ICompositeStateExtension<IBehaviorFinalizedOverridenCompositeStateBuilder>,
        IStateEvents<IBehaviorFinalizedOverridenCompositeStateBuilder>,
        IStateUtils<IBehaviorFinalizedOverridenCompositeStateBuilder>,
        IStateUtilsOverrides<IBehaviorFinalizedOverridenCompositeStateBuilder>,
        ICompositeStateTypedEvents<IBehaviorFinalizedOverridenCompositeStateBuilder>,
        IStateTransitions<IBehaviorFinalizedOverridenCompositeStateBuilder>,
        IStateTransitionsOverrides<IBehaviorFinalizedOverridenCompositeStateBuilder>,
        IStateMachineOverrides<IBehaviorFinalizedOverridenCompositeStateBuilder>,
        IStateOrthogonalization<IBehaviorFinalizedOverridenRegionalizedCompositeStateBuilder>;

    public interface IBehaviorFinalizedOverridenRegionalizedCompositeStateBuilder :
        ICompositeStateExtension<IBehaviorFinalizedOverridenRegionalizedCompositeStateBuilder>,
        IStateEvents<IBehaviorFinalizedOverridenRegionalizedCompositeStateBuilder>,
        IStateUtils<IBehaviorFinalizedOverridenRegionalizedCompositeStateBuilder>,
        IStateUtilsOverrides<IBehaviorFinalizedOverridenRegionalizedCompositeStateBuilder>,
        ICompositeStateTypedEvents<IBehaviorFinalizedOverridenRegionalizedCompositeStateBuilder>,
        IStateMachineOverrides<IBehaviorFinalizedOverridenRegionalizedCompositeStateBuilder>,
        IStateTransitions<IBehaviorFinalizedOverridenRegionalizedCompositeStateBuilder>,
        IStateTransitionsOverrides<IBehaviorFinalizedOverridenRegionalizedCompositeStateBuilder>;
}
