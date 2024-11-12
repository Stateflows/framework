using Stateflows.StateMachines.Registration.Interfaces.Base;

namespace Stateflows.StateMachines.Registration.Interfaces
{
    public interface IInitializedCompositeStateBuilder :
        IStateEvents<IInitializedCompositeStateBuilder>,
        IStateUtils<IInitializedCompositeStateBuilder>,
        ICompositeStateEvents<IInitializedCompositeStateBuilder>,
        IStateTransitions<IInitializedCompositeStateBuilder>,
        IStateMachine<IInitializedCompositeStateBuilder>,
        IStateMachineFinal<IFinalizedCompositeStateBuilder>
    { }

    public interface IFinalizedCompositeStateBuilder :
        IStateEvents<IFinalizedCompositeStateBuilder>,
        IStateUtils<IFinalizedCompositeStateBuilder>,
        ICompositeStateEvents<IFinalizedCompositeStateBuilder>,
        IStateTransitions<IFinalizedCompositeStateBuilder>
    { }

    public interface ICompositeStateBuilder :
        IStateEvents<ICompositeStateBuilder>,
        IStateUtils<ICompositeStateBuilder>,
        ICompositeStateEvents<ICompositeStateBuilder>,
        IStateTransitions<ICompositeStateBuilder>,
        IStateMachineInitial<IInitializedCompositeStateBuilder>
    { }

    public interface IFinalizedOverridenCompositeStateBuilder :
        IStateEvents<IFinalizedOverridenCompositeStateBuilder>,
        IStateUtils<IFinalizedOverridenCompositeStateBuilder>,
        ICompositeStateEvents<IFinalizedOverridenCompositeStateBuilder>,
        IStateTransitions<IFinalizedOverridenCompositeStateBuilder>,
        IStateTransitionsOverrides<IFinalizedOverridenCompositeStateBuilder>
    { }

    public interface IOverridenCompositeStateBuilder :
        IStateEvents<IOverridenCompositeStateBuilder>,
        IStateUtils<IOverridenCompositeStateBuilder>,
        ICompositeStateEvents<IOverridenCompositeStateBuilder>,
        IStateTransitions<IOverridenCompositeStateBuilder>,
        IStateTransitionsOverrides<IOverridenCompositeStateBuilder>,
        IStateMachine<IOverridenCompositeStateBuilder>,
        IStateMachineOverrides<IOverridenCompositeStateBuilder>,
        IStateMachineFinal<IFinalizedOverridenCompositeStateBuilder>
    { }
}
