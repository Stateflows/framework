using Stateflows.StateMachines.Registration.Interfaces.Base;

namespace Stateflows.StateMachines.Registration.Interfaces
{
    public interface ICompositeStateBuilder :
        IStateEventsBuilderBase<ICompositeStateBuilder>,
        ICompositeStateEventsBuilderBase<ICompositeStateBuilder>,
        IStateTransitionsBuilderBase<ICompositeStateBuilder>,
        IStateMachineBuilderBase<ICompositeStateBuilder>
    { }

    public interface ICompositeStateInitialBuilder :
        IStateEventsBuilderBase<ICompositeStateInitialBuilder>,
        ICompositeStateEventsBuilderBase<ICompositeStateInitialBuilder>,
        IStateTransitionsBuilderBase<ICompositeStateInitialBuilder>,
        IStateMachineInitialBuilderBase<ICompositeStateBuilder>
    { }

    public interface ICompositeStateTransitionsBuilder :
        ICompositeStateEventsBuilderBase<ICompositeStateTransitionsBuilder>,
        IStateTransitionsBuilderBase<ICompositeStateTransitionsBuilder>,
        IStateMachineBuilderBase<ICompositeStateTransitionsBuilder>
    { }

    public interface ICompositeStateInitialTransitionsBuilder :
        ICompositeStateEventsBuilderBase<ICompositeStateTransitionsBuilder>,
        IStateMachineInitialBuilderBase<ICompositeStateTransitionsBuilder>
    { }
}
