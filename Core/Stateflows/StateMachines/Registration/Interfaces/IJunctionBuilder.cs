using Stateflows.StateMachines.Registration.Interfaces.Base;

namespace Stateflows.StateMachines.Registration.Interfaces
{
    public interface IJunctionBuilder :
        IPseudostateTransitions<IJunctionBuilder>
    { }
    
    public interface IOverridenJunctionBuilder :
        IPseudostateTransitions<IOverridenJunctionBuilder>,
        IPseudostateTransitionsOverrides<IOverridenJunctionBuilder>
    { }

}
