using Stateflows.StateMachines.Registration.Interfaces.Base;

namespace Stateflows.StateMachines.Registration.Interfaces
{
    public interface IDefaultTransitionEffectBuilder :
        ITargetedTransitionUtils<IDefaultTransitionEffectBuilder>,
        IDefaultEffect<IDefaultTransitionEffectBuilder>
    { }
    
    public interface IDefaultTransitionBuilder :
        ITargetedTransitionUtils<IDefaultTransitionBuilder>,
        IDefaultEffect<IDefaultTransitionBuilder>,
        IDefaultGuard<IDefaultTransitionBuilder>
    { }
    
    public interface IOverridenDefaultTransitionEffectBuilder :
        ITargetedTransitionUtils<IOverridenDefaultTransitionEffectBuilder>,
        IDefaultEffect<IOverridenDefaultTransitionEffectBuilder>
    { }
    
    public interface IOverridenDefaultTransitionBuilder :
        ITargetedTransitionUtils<IOverridenDefaultTransitionBuilder>,
        IDefaultEffect<IOverridenDefaultTransitionBuilder>,
        IDefaultGuard<IOverridenDefaultTransitionBuilder>
    { }
}
