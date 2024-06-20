using Stateflows.StateMachines.Registration.Interfaces.Base;

namespace Stateflows.StateMachines
{
    public interface IInitializedStateMachineBuilder :
        IStateMachine<IInitializedStateMachineBuilder>,
        IStateMachineFinal<IFinalizedStateMachineBuilder>,
        IStateMachineUtils<IInitializedStateMachineBuilder>,
        IStateMachineEvents<IInitializedStateMachineBuilder>
    { }

    public interface IFinalizedStateMachineBuilder :
        IStateMachineUtils<IFinalizedStateMachineBuilder>,
        IStateMachineEvents<IFinalizedStateMachineBuilder>
    { }

    public interface IStateMachineBuilder :
        IStateMachineInitial<IInitializedStateMachineBuilder>,
        IStateMachineUtils<IStateMachineBuilder>,
        IStateMachineEvents<IStateMachineBuilder>
    { }

    //public interface ITypedInitializedStateMachineBuilder :
    //    IStateMachine<ITypedInitializedStateMachineBuilder>,
    //    IStateMachineFinal<ITypedFinalizedStateMachineBuilder>,
    //    IStateMachineUtils<ITypedInitializedStateMachineBuilder>,
    //    IStateMachineEvents<ITypedStateMachineBuilder>
    //{ }

    //public interface ITypedFinalizedStateMachineBuilder :
    //    IStateMachineUtils<ITypedFinalizedStateMachineBuilder>,
    //    IStateMachineEvents<ITypedStateMachineBuilder>
    //{ }

    //public interface ITypedStateMachineBuilder :
    //    IStateMachineInitial<ITypedInitializedStateMachineBuilder>,
    //    IStateMachineUtils<ITypedStateMachineBuilder>,
    //    IStateMachineEvents<ITypedStateMachineBuilder>
    //{ }
}
