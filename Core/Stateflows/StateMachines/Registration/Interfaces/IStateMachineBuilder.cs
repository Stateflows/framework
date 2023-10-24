using Microsoft.Extensions.DependencyInjection;
using Stateflows.StateMachines.Registration.Interfaces.Base;

namespace Stateflows.StateMachines
{
    public interface IStateMachineBuilder :
        IStateMachine<IStateMachineBuilder>,
        IStateMachineFinal<IFinalizedStateMachineBuilder>,
        IStateMachineUtils<IStateMachineBuilder>,
        IStateMachineEvents<IStateMachineBuilder>
    { }

    public interface IFinalizedStateMachineBuilder :
        IStateMachineUtils<IFinalizedStateMachineBuilder>,
        IStateMachineEvents<IFinalizedStateMachineBuilder>
    { }

    public interface IStateMachineInitialBuilder :
        IStateMachineInitial<IStateMachineBuilder>,
        IStateMachineUtils<IStateMachineInitialBuilder>,
        IStateMachineEvents<IStateMachineInitialBuilder>
    { }

    public interface ITypedStateMachineBuilder :
        IStateMachine<ITypedStateMachineBuilder>,
        IStateMachineFinal<ITypedFinalizedStateMachineBuilder>,
        IStateMachineUtils<ITypedStateMachineBuilder>
    { }

    public interface ITypedFinalizedStateMachineBuilder :
        IStateMachine<ITypedFinalizedStateMachineBuilder>,
        IStateMachineUtils<ITypedFinalizedStateMachineBuilder>
    { }

    public interface ITypedStateMachineInitialBuilder :
        IStateMachineInitial<ITypedStateMachineBuilder>,
        IStateMachineUtils<ITypedStateMachineInitialBuilder>
    { }
}
