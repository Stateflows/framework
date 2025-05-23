using Stateflows.StateMachines.Registration.Interfaces.Base;

namespace Stateflows.StateMachines.Registration.Interfaces
{
    public interface IJoinBuilder :
        IPseudostateElseTransitions<IJoinBuilder>
    { }
    
    public interface IOverridenJoinBuilder :
        IPseudostateElseTransitions<IOverridenJoinBuilder>,
        IPseudostateElseTransitionsOverrides<IOverridenJoinBuilder>
    { }
}
