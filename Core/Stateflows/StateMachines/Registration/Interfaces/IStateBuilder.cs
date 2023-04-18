using Stateflows.StateMachines.Registration.Interfaces.Base;

namespace Stateflows.StateMachines.Registration.Interfaces
{
    public partial interface IStateBuilder : 
        IStateEventsBuilderBase<IStateBuilder>,
        IStateTransitionsBuilderBase<IStateBuilder>
    { }

    public partial interface IStateTransitionsBuilder : 
        IStateTransitionsBuilderBase<IStateTransitionsBuilder>
    { }
}
