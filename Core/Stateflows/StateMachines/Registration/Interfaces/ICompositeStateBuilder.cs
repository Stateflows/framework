using Stateflows.StateMachines.Registration.Interfaces.Base;

namespace Stateflows.StateMachines.Registration.Interfaces
{
    public interface IInitializedCompositeStateBuilder :
        ICompositeStateExtension<IInitializedCompositeStateBuilder>,
        IStateEvents<IInitializedCompositeStateBuilder>,
        IStateUtils<IInitializedCompositeStateBuilder>,
        ICompositeStateTypedEvents<IInitializedCompositeStateBuilder>,
        IStateTransitions<IInitializedCompositeStateBuilder>,
        IStateMachine<IInitializedCompositeStateBuilder>,
        IStateMachineFinal<IFinalizedCompositeStateBuilder>
    { }

    public interface IFinalizedCompositeStateBuilder :
        ICompositeStateExtension<IFinalizedCompositeStateBuilder>,
        IStateEvents<IFinalizedCompositeStateBuilder>,
        IStateUtils<IFinalizedCompositeStateBuilder>,
        ICompositeStateTypedEvents<IFinalizedCompositeStateBuilder>,
        IStateTransitions<IFinalizedCompositeStateBuilder>
    { }

    public interface ICompositeStateBuilder :
        ICompositeStateExtension<ICompositeStateBuilder>,
        IStateEvents<ICompositeStateBuilder>,
        IStateUtils<ICompositeStateBuilder>,
        ICompositeStateTypedEvents<ICompositeStateBuilder>,
        IStateTransitions<ICompositeStateBuilder>,
        IStateMachineInitial<IInitializedCompositeStateBuilder>,
        IStateMachine<IInitializedCompositeStateBuilder>
    { }

    public interface IFinalizedOverridenCompositeStateBuilder :
        ICompositeStateExtension<IFinalizedOverridenCompositeStateBuilder>,
        IStateEvents<IFinalizedOverridenCompositeStateBuilder>,
        IStateUtils<IFinalizedOverridenCompositeStateBuilder>,
        ICompositeStateTypedEvents<IFinalizedOverridenCompositeStateBuilder>,
        IStateTransitions<IFinalizedOverridenCompositeStateBuilder>,
        IStateTransitionsOverrides<IFinalizedOverridenCompositeStateBuilder>,
        IStateMachineOverrides<IFinalizedOverridenCompositeStateBuilder>,
        IStateOrthogonalization<IFinalizedOverridenRegionalizedCompositeStateBuilder>
    { }

    public interface IFinalizedOverridenRegionalizedCompositeStateBuilder :
        ICompositeStateExtension<IFinalizedOverridenRegionalizedCompositeStateBuilder>,
        IStateEvents<IFinalizedOverridenRegionalizedCompositeStateBuilder>,
        IStateUtils<IFinalizedOverridenRegionalizedCompositeStateBuilder>,
        ICompositeStateTypedEvents<IFinalizedOverridenRegionalizedCompositeStateBuilder>,
        IStateMachineOverrides<IFinalizedOverridenRegionalizedCompositeStateBuilder>,
        IStateTransitions<IFinalizedOverridenRegionalizedCompositeStateBuilder>,
        IStateTransitionsOverrides<IFinalizedOverridenRegionalizedCompositeStateBuilder>
    { }

    public interface IOverridenCompositeStateBuilder :
        ICompositeStateExtension<IOverridenCompositeStateBuilder>,
        IStateEvents<IOverridenCompositeStateBuilder>,
        IStateUtils<IOverridenCompositeStateBuilder>,
        ICompositeStateTypedEvents<IOverridenCompositeStateBuilder>,
        IStateTransitions<IOverridenCompositeStateBuilder>,
        IStateTransitionsOverrides<IOverridenCompositeStateBuilder>,
        IStateMachine<IOverridenCompositeStateBuilder>,
        IStateMachineOverrides<IOverridenCompositeStateBuilder>,
        IStateOrthogonalization<IOverridenRegionalizedCompositeStateBuilder>,
        IStateMachineFinal<IFinalizedOverridenCompositeStateBuilder>
    { }

    public interface IOverridenRegionalizedCompositeStateBuilder :
        ICompositeStateExtension<IOverridenRegionalizedCompositeStateBuilder>,
        IStateEvents<IOverridenRegionalizedCompositeStateBuilder>,
        IStateUtils<IOverridenRegionalizedCompositeStateBuilder>,
        ICompositeStateTypedEvents<IOverridenRegionalizedCompositeStateBuilder>,
        IStateTransitions<IOverridenRegionalizedCompositeStateBuilder>,
        IStateTransitionsOverrides<IOverridenRegionalizedCompositeStateBuilder>,
        IStateMachine<IOverridenRegionalizedCompositeStateBuilder>,
        IStateMachineOverrides<IOverridenRegionalizedCompositeStateBuilder>,
        IStateMachineFinal<IFinalizedOverridenRegionalizedCompositeStateBuilder>
    { }
}
