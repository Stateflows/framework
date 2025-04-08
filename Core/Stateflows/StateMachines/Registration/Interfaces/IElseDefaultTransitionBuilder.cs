using Stateflows.StateMachines.Registration.Interfaces.Base;

namespace Stateflows.StateMachines.Registration.Interfaces
{
    public interface IElseDefaultTransitionBuilder :
        ITransitionUtils<IElseDefaultTransitionBuilder>,
        IDefaultEffect<IElseDefaultTransitionBuilder>
    { }
    
    public interface IOverridenElseDefaultTransitionBuilder :
        ITransitionUtils<IOverridenElseDefaultTransitionBuilder>,
        IDefaultEffect<IOverridenElseDefaultTransitionBuilder>
    { }
}
