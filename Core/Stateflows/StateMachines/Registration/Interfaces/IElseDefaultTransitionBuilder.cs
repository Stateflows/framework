using Stateflows.StateMachines.Registration.Interfaces.Base;

namespace Stateflows.StateMachines.Registration.Interfaces
{
    /// <summary>Configures effects for a fallback completion transition; no additional guard is needed.</summary>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/5.4.2.4.2.-Default-Transition" />
    /// <seealso href="https://github.com/Stateflows/framework/wiki/5.4.1.3.-Evaluation-of-Transitions#else-transitions" />
    public interface IElseDefaultTransitionBuilder :
        ITriggeredTransitionUtils<IElseDefaultTransitionBuilder>,
        ITargetedTransitionUtils<IElseDefaultTransitionBuilder>,
        IDefaultEffect<IElseDefaultTransitionBuilder>
    { }
    
    /// <summary>Extends effects on an inherited fallback completion transition.</summary>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/5.4.2.4.2.-Default-Transition" />
    /// <seealso href="https://github.com/Stateflows/framework/wiki/5.-Behaviors#definition-reuse-and-versions" />
    /// <seealso href="https://github.com/Stateflows/framework/wiki/5.4.1.3.-Evaluation-of-Transitions#else-transitions" />
    public interface IOverridenElseDefaultTransitionBuilder :
        ITriggeredTransitionUtils<IOverridenElseDefaultTransitionBuilder>,
        ITargetedTransitionUtils<IOverridenElseDefaultTransitionBuilder>,
        IDefaultEffect<IOverridenElseDefaultTransitionBuilder>
    { }
}
