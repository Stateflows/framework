using Stateflows.StateMachines.Registration.Interfaces.Base;

namespace Stateflows.StateMachines.Registration.Interfaces
{
    public partial interface IStateBuilder : 
        IStateBuilderBase<IStateBuilder>,
        IStateTransitionsBuilderBase<IStateBuilder>
    { }

    public partial interface ITypedStateBuilder : 
        IStateTransitionsBuilderBase<ITypedStateBuilder>
    { }
}
