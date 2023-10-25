using Stateflows.StateMachines.Registration.Interfaces.Base;

namespace Stateflows.StateMachines.Registration.Interfaces
{
    public interface IStateBuilder :
        IStateEvents<IStateBuilder>,
        IStateUtils<IStateBuilder>,
        IStateTransitions<IStateBuilder>,
        IStateSubmachine<ISubmachineStateBuilder>
    { }

    public interface ISubmachineStateBuilder :
        IStateEvents<ISubmachineStateBuilder>,
        IStateUtils<ISubmachineStateBuilder>,
        IStateTransitions<ISubmachineStateBuilder>
    { }

    public interface ITypedStateBuilder :
        IStateUtils<ITypedStateBuilder>,
        IStateTransitions<ITypedStateBuilder>,
        IStateSubmachine<ISubmachineTypedStateBuilder>
    { }

    public interface ISubmachineTypedStateBuilder :
        IStateUtils<ISubmachineTypedStateBuilder>,
        IStateTransitions<ISubmachineTypedStateBuilder>
    { }
}
