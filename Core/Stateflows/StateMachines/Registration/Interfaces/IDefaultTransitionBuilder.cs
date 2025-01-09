using Stateflows.StateMachines.Events;
using Stateflows.StateMachines.Registration.Interfaces.Base;

namespace Stateflows.StateMachines.Registration.Interfaces
{
    public interface IDefaultTransitionEffectBuilder :
        IDefaultEffect<IDefaultTransitionEffectBuilder>
    { }
    
    public interface IDefaultTransitionBuilder :
        IDefaultEffect<IDefaultTransitionBuilder>,
        IDefaultGuard<IDefaultTransitionBuilder>
    { }
}
