using Stateflows.StateMachines.Registration.Interfaces.Base;

namespace Stateflows.StateMachines.Registration.Interfaces
{
    /// <summary>Configures effects for a fallback completion transition; no additional guard is needed.</summary>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-default-transition" />
    /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-evaluation-of-transitions#else-transitions" />
    public interface IElseDefaultTransitionBuilder :
        ITriggeredTransitionUtils<IElseDefaultTransitionBuilder>,
        ITargetedTransitionUtils<IElseDefaultTransitionBuilder>,
        IDefaultEffect<IElseDefaultTransitionBuilder>
    { }
    
    /// <summary>Extends effects on an inherited fallback completion transition.</summary>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-default-transition" />
    /// <seealso href="https://github.com/Stateflows/framework/wiki/behaviors#definition-reuse-and-versions" />
    /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-evaluation-of-transitions#else-transitions" />
    public interface IOverridenElseDefaultTransitionBuilder :
        ITriggeredTransitionUtils<IOverridenElseDefaultTransitionBuilder>,
        ITargetedTransitionUtils<IOverridenElseDefaultTransitionBuilder>,
        IDefaultEffect<IOverridenElseDefaultTransitionBuilder>
    { }
}
