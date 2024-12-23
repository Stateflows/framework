using Stateflows.StateMachines.Registration.Interfaces.Base;

namespace Stateflows.StateMachines.Registration.Interfaces
{
    public interface IJoinBuilder :
        IStateTransitions<IJoinBuilder>
    { }
    
    public interface IOverridenJoinBuilder :
        IStateTransitions<IOverridenJoinBuilder>,
        IStateTransitionsOverrides<IOverridenJoinBuilder>
    { }
}
