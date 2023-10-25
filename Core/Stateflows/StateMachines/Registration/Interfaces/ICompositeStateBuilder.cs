using Stateflows.StateMachines.Registration.Interfaces.Base;

namespace Stateflows.StateMachines.Registration.Interfaces
{
    public interface ICompositeStateBuilder :
        IStateEvents<ICompositeStateBuilder>,
        IStateUtils<ICompositeStateBuilder>,
        ICompositeStateEvents<ICompositeStateBuilder>,
        IStateTransitions<ICompositeStateBuilder>,
        IStateMachine<ICompositeStateBuilder>,
        IStateMachineFinal<IFinalizedCompositeStateBuilder>
    { }

    public interface IFinalizedCompositeStateBuilder :
        IStateEvents<IFinalizedCompositeStateBuilder>,
        IStateUtils<IFinalizedCompositeStateBuilder>,
        ICompositeStateEvents<IFinalizedCompositeStateBuilder>,
        IStateTransitions<IFinalizedCompositeStateBuilder>
    { }

    public interface ICompositeStateInitialBuilder :
        IStateEvents<ICompositeStateInitialBuilder>,
        IStateUtils<ICompositeStateInitialBuilder>,
        ICompositeStateEvents<ICompositeStateInitialBuilder>,
        IStateTransitions<ICompositeStateInitialBuilder>,
        IStateMachineInitial<ICompositeStateBuilder>
    { }

    public interface ITypedCompositeStateBuilder :
        IStateUtils<ITypedCompositeStateBuilder>,
        IStateTransitions<ITypedCompositeStateBuilder>,
        IStateMachine<ITypedCompositeStateBuilder>,
        IStateMachineFinal<ITypedFinalizedCompositeStateBuilder>
    { }

    public interface ITypedFinalizedCompositeStateBuilder :
        IStateUtils<ITypedFinalizedCompositeStateBuilder>,
        IStateTransitions<ITypedFinalizedCompositeStateBuilder>
    { }

    public interface ITypedCompositeStateInitialBuilder :
        IStateUtils<ITypedCompositeStateInitialBuilder>,
        IStateTransitions<ITypedCompositeStateInitialBuilder>,
        IStateMachineInitial<ITypedCompositeStateBuilder>
    { }
}
