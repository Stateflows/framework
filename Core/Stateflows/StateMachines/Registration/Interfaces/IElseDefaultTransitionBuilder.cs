using Stateflows.StateMachines.Registration.Interfaces.Base;

namespace Stateflows.StateMachines.Registration.Interfaces
{
    public interface IElseDefaultTransitionBuilder :
        ITriggeredTransitionUtils<IElseDefaultTransitionBuilder>,
        ITargetedTransitionUtils<IElseDefaultTransitionBuilder>,
        IDefaultEffect<IElseDefaultTransitionBuilder>
    { }
    
    public interface IOverridenElseDefaultTransitionBuilder :
        ITriggeredTransitionUtils<IOverridenElseDefaultTransitionBuilder>,
        ITargetedTransitionUtils<IOverridenElseDefaultTransitionBuilder>,
        IDefaultEffect<IOverridenElseDefaultTransitionBuilder>
    { }
}
