using Stateflows.StateMachines.Registration.Interfaces.Base;

namespace Stateflows.StateMachines.Registration.Interfaces
{
    /// <summary>Configures guarded completion branches and an optional fallback from a choice evaluated after incoming
    /// effects.</summary>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/5.4.2.3.1.-Choice" />
    public interface IChoiceBuilder :
        IPseudostateElseTransitions<IChoiceBuilder>;
    
    /// <summary>Extends an inherited choice, including its existing guarded and fallback branches.</summary>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/5.4.2.3.1.-Choice" />
    /// <seealso href="https://github.com/Stateflows/framework/wiki/5.-Behaviors#definition-reuse-and-versions" />
    public interface IOverridenChoiceBuilder :
        IPseudostateElseTransitions<IOverridenChoiceBuilder>,
        IPseudostateElseTransitionsOverrides<IOverridenChoiceBuilder>;
}
