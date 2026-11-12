using Stateflows.StateMachines.Registration.Interfaces.Base;

namespace Stateflows.StateMachines.Registration.Interfaces
{
    /// <summary>Defines the effect-only continuation after incoming orthogonal-region paths synchronize at a join.
    /// </summary>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/5.4.2.3.4.-Join" />
    public interface IJoinBuilder :
        IPseudostateElseTransitions<IJoinBuilder>
    { }
    
    /// <summary>Extends the continuation of an inherited join.</summary>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/5.4.2.3.4.-Join" />
    /// <seealso href="https://github.com/Stateflows/framework/wiki/5.-Behaviors#definition-reuse-and-versions" />
    public interface IOverridenJoinBuilder :
        IPseudostateElseTransitions<IOverridenJoinBuilder>,
        IPseudostateElseTransitionsOverrides<IOverridenJoinBuilder>
    { }
}
