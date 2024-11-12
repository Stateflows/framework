using Stateflows.StateMachines.Registration.Interfaces.Base;

namespace Stateflows.StateMachines.Registration.Interfaces
{
    public interface IChoiceBuilder :
        IPseudostateTransitions<IChoiceBuilder>
    { }
    
    public interface IOverridenChoiceBuilder :
        IPseudostateTransitions<IOverridenChoiceBuilder>,
        IPseudostateTransitionsOverrides<IOverridenChoiceBuilder>
    { }
}
