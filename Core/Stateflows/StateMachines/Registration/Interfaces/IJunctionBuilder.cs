using Stateflows.StateMachines.Registration.Interfaces.Base;

namespace Stateflows.StateMachines.Registration.Interfaces
{
    public interface IJunctionBuilder :
        IPseudostateElseTransitions<IJunctionBuilder>
    { }
    
    public interface IOverridenJunctionBuilder :
        IPseudostateElseTransitions<IOverridenJunctionBuilder>,
        IPseudostateElseTransitionsOverrides<IOverridenJunctionBuilder>
    { }

}
