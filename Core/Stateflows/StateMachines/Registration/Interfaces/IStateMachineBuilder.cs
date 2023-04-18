using Stateflows.StateMachines.Registration.Interfaces.Base;

namespace Stateflows.StateMachines
{
    public interface IStateMachineBuilder : 
        IStateMachineBuilderBase<IStateMachineBuilder>,
        IStateMachineUtilsBuilderBase<IStateMachineBuilder>,
        IStateMachineEventsBuilderBase<IStateMachineBuilder>
    { }

    public interface IStateMachineInitialBuilder :
        IStateMachineInitialBuilderBase<IStateMachineBuilder>,
        IStateMachineUtilsBuilderBase<IStateMachineInitialBuilder>,
        IStateMachineEventsBuilderBase<IStateMachineInitialBuilder>
    { }
}
