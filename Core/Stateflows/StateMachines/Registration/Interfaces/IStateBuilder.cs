using Stateflows.StateMachines.Registration.Interfaces.Base;

namespace Stateflows.StateMachines.Registration.Interfaces
{
    public partial interface IStateBuilder :
        IStateEventsBuilderBase<IStateBuilder>,
        IStateUtilsBuilderBase<IStateBuilder>,
        IStateTransitionsBuilderBase<IStateBuilder>,
        IStateSubmachineBuilderBase<ISubmachineStateBuilder>
    { }

    public partial interface ISubmachineStateBuilder :
        IStateEventsBuilderBase<ISubmachineStateBuilder>,
        IStateUtilsBuilderBase<ISubmachineStateBuilder>,
        IStateTransitionsBuilderBase<ISubmachineStateBuilder>
    { }

    public partial interface ITypedStateBuilder :
        IStateUtilsBuilderBase<ITypedStateBuilder>,
        IStateTransitionsBuilderBase<ITypedStateBuilder>,
        IStateSubmachineBuilderBase<ISubmachineTypedStateBuilder>
    { }

    public partial interface ISubmachineTypedStateBuilder :
        IStateUtilsBuilderBase<ISubmachineTypedStateBuilder>,
        IStateTransitionsBuilderBase<ISubmachineTypedStateBuilder>
    { }
}
