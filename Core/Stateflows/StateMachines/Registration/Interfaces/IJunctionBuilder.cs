using Stateflows.StateMachines.Registration.Interfaces.Base;

namespace Stateflows.StateMachines.Registration.Interfaces
{
    /// <summary>Configures guarded paths through a junction, whose guards participate in selecting the compound
    /// transition.</summary>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/5.4.2.3.2.-Junction" />
    public interface IJunctionBuilder :
        IPseudostateElseTransitions<IJunctionBuilder>;
    
    /// <summary>Extends an inherited junction's guarded and fallback paths.</summary>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/5.4.2.3.2.-Junction" />
    /// <seealso href="https://github.com/Stateflows/framework/wiki/5.-Behaviors#definition-reuse-and-versions" />
    public interface IOverridenJunctionBuilder :
        IPseudostateElseTransitions<IOverridenJunctionBuilder>,
        IPseudostateElseTransitionsOverrides<IOverridenJunctionBuilder>;

}
