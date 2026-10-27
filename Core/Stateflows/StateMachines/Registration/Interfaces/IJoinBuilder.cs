using Stateflows.StateMachines.Registration.Interfaces.Base;

namespace Stateflows.StateMachines.Registration.Interfaces
{
    /// <summary>Defines the effect-only continuation after incoming orthogonal-region paths synchronize at a join.
    /// </summary>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-join" />
    public interface IJoinBuilder :
        IPseudostateElseTransitions<IJoinBuilder>
    { }
    
    /// <summary>Extends the continuation of an inherited join.</summary>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-join" />
    /// <seealso href="https://github.com/Stateflows/framework/wiki/behaviors#definition-reuse-and-versions" />
    public interface IOverridenJoinBuilder :
        IPseudostateElseTransitions<IOverridenJoinBuilder>,
        IPseudostateElseTransitionsOverrides<IOverridenJoinBuilder>
    { }
}
