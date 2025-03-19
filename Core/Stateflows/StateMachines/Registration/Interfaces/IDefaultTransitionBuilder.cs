using Stateflows.StateMachines.Registration.Interfaces.Base;

namespace Stateflows.StateMachines.Registration.Interfaces
{
    public interface IDefaultTransitionEffectBuilder :
        ITransitionUtils<IDefaultTransitionEffectBuilder>,
        IDefaultEffect<IDefaultTransitionEffectBuilder>
    { }
    
    public interface IDefaultTransitionBuilder :
        ITransitionUtils<IDefaultTransitionBuilder>,
        IDefaultEffect<IDefaultTransitionBuilder>,
        IDefaultGuard<IDefaultTransitionBuilder>
    { }
}
