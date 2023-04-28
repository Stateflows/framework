using Stateflows.StateMachines.Registration.Interfaces.Base;

namespace Stateflows.StateMachines.Registration.Interfaces
{
    public interface ICompositeStateBuilder :
        IStateBuilderBase<ICompositeStateBuilder>,
        ICompositeStateBuilderBase<ICompositeStateBuilder>,
        IStateTransitionsBuilderBase<ICompositeStateBuilder>,
        IStateMachineBuilderBase<ICompositeStateBuilder>
    { }

    public interface ICompositeStateInitialBuilder :
        IStateBuilderBase<ICompositeStateInitialBuilder>,
        ICompositeStateBuilderBase<ICompositeStateInitialBuilder>,
        IStateTransitionsBuilderBase<ICompositeStateInitialBuilder>,
        IStateMachineInitialBuilderBase<ICompositeStateBuilder>
    { }

    public interface ITypedCompositeStateBuilder :
        ICompositeStateBuilderBase<ITypedCompositeStateBuilder>,
        IStateTransitionsBuilderBase<ITypedCompositeStateBuilder>,
        IStateMachineBuilderBase<ITypedCompositeStateBuilder>
    { }

    public interface ITypedCompositeStateInitialBuilder :
        ICompositeStateBuilderBase<ITypedCompositeStateBuilder>,
        IStateMachineInitialBuilderBase<ITypedCompositeStateBuilder>
    { }
}
