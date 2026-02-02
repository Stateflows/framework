using Stateflows.StateMachines.Registration.Interfaces.Base;

namespace Stateflows.StateMachines.Registration.Interfaces
{
    public interface IInitializedCompositeStateBuilder :
        ICompositeStateExtension<IInitializedCompositeStateBuilder>,
        IStateEvents<IInitializedCompositeStateBuilder>,
        IStateUtils<IInitializedCompositeStateBuilder>,
        ICompositeStateTypedEvents<IInitializedCompositeStateBuilder>,
        IStateTransitions<IInitializedCompositeStateBuilder>,
        IStateMachineElements<IInitializedCompositeStateBuilder>,
        IStateHistory<IInitializedCompositeStateBuilder>,
        IStateMachineFinal<IFinalizedCompositeStateBuilder>;

    public interface IFinalizedCompositeStateBuilder :
        ICompositeStateExtension<IFinalizedCompositeStateBuilder>,
        IStateEvents<IFinalizedCompositeStateBuilder>,
        IStateUtils<IFinalizedCompositeStateBuilder>,
        ICompositeStateTypedEvents<IFinalizedCompositeStateBuilder>,
        IStateTransitions<IFinalizedCompositeStateBuilder>;

    public interface ICompositeStateBuilder :
        ICompositeStateExtension<ICompositeStateBuilder>,
        IStateEvents<ICompositeStateBuilder>,
        IStateUtils<ICompositeStateBuilder>,
        ICompositeStateTypedEvents<ICompositeStateBuilder>,
        IStateTransitions<ICompositeStateBuilder>,
        IStateMachineInitial<IInitializedCompositeStateBuilder>,
        IStateMachineElements<IInitializedCompositeStateBuilder>,
        IStateHistory<ICompositeStateBuilder>;

    public interface IFinalizedOverridenCompositeStateBuilder :
        ICompositeStateExtension<IFinalizedOverridenCompositeStateBuilder>,
        IStateEvents<IFinalizedOverridenCompositeStateBuilder>,
        IStateUtils<IFinalizedOverridenCompositeStateBuilder>,
        IStateUtilsOverrides<IFinalizedOverridenCompositeStateBuilder>,
        ICompositeStateTypedEvents<IFinalizedOverridenCompositeStateBuilder>,
        IStateTransitions<IFinalizedOverridenCompositeStateBuilder>,
        IStateTransitionsOverrides<IFinalizedOverridenCompositeStateBuilder>,
        IStateMachineOverrides<IFinalizedOverridenCompositeStateBuilder>,
        IStateOrthogonalization<IFinalizedOverridenRegionalizedCompositeStateBuilder>;

    public interface IFinalizedOverridenRegionalizedCompositeStateBuilder :
        ICompositeStateExtension<IFinalizedOverridenRegionalizedCompositeStateBuilder>,
        IStateEvents<IFinalizedOverridenRegionalizedCompositeStateBuilder>,
        IStateUtils<IFinalizedOverridenRegionalizedCompositeStateBuilder>,
        IStateUtilsOverrides<IFinalizedOverridenRegionalizedCompositeStateBuilder>,
        ICompositeStateTypedEvents<IFinalizedOverridenRegionalizedCompositeStateBuilder>,
        IStateMachineOverrides<IFinalizedOverridenRegionalizedCompositeStateBuilder>,
        IStateTransitions<IFinalizedOverridenRegionalizedCompositeStateBuilder>,
        IStateTransitionsOverrides<IFinalizedOverridenRegionalizedCompositeStateBuilder>;

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
        IStateMachineFinal<IFinalizedOverridenCompositeStateBuilder>;

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
        IStateMachineFinal<IFinalizedOverridenRegionalizedCompositeStateBuilder>;
}
