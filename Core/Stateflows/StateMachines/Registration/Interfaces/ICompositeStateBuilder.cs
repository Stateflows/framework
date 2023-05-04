using Stateflows.StateMachines.Registration.Interfaces.Base;

namespace Stateflows.StateMachines.Registration.Interfaces
{
    public interface ICompositeStateBuilder :
        IStateEventsBuilderBase<ICompositeStateBuilder>,
        IStateUtilsBuilderBase<ICompositeStateBuilder>,
        ICompositeStateBuilderBase<ICompositeStateBuilder>,
        IStateTransitionsBuilderBase<ICompositeStateBuilder>,
        IStateMachineBuilderBase<ICompositeStateBuilder>
    { }

    public interface ICompositeStateInitialBuilder :
        IStateEventsBuilderBase<ICompositeStateInitialBuilder>,
        IStateUtilsBuilderBase<ICompositeStateInitialBuilder>,
        ICompositeStateBuilderBase<ICompositeStateInitialBuilder>,
        IStateTransitionsBuilderBase<ICompositeStateInitialBuilder>,
        IStateMachineInitialBuilderBase<ICompositeStateBuilder>
    { }

    public interface ITypedCompositeStateBuilder :
        IStateUtilsBuilderBase<ITypedCompositeStateBuilder>,
        IStateTransitionsBuilderBase<ITypedCompositeStateBuilder>,
        IStateMachineBuilderBase<ITypedCompositeStateBuilder>
    { }

    public interface ITypedCompositeStateInitialBuilder :
        IStateUtilsBuilderBase<ITypedCompositeStateInitialBuilder>,
        IStateTransitionsBuilderBase<ITypedCompositeStateInitialBuilder>,
        IStateMachineInitialBuilderBase<ITypedCompositeStateBuilder>
    { }
}
