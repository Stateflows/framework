using Stateflows.StateMachines.Registration.Interfaces.Base;

namespace Stateflows.StateMachines.Registration.Interfaces
{
    public interface IChoiceBuilder :
        IPseudostateElseTransitions<IChoiceBuilder>
    { }
    
    public interface IOverridenChoiceBuilder :
        IPseudostateElseTransitions<IOverridenChoiceBuilder>,
        IPseudostateElseTransitionsOverrides<IOverridenChoiceBuilder>
    { }
}
