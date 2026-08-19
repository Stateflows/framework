using Stateflows.Common.Interfaces;
using Stateflows.StateMachines.Registration.Interfaces.Base;

namespace Stateflows.StateMachines.Registration.Interfaces
{
    public interface IDefaultTransitionEffectBuilder :
        IElementMetadataBuilder<IDefaultTransitionEffectBuilder>,
        ITargetedTransitionUtils<IDefaultTransitionEffectBuilder>,
        IDefaultEffect<IDefaultTransitionEffectBuilder>;
    
    public interface IDefaultTransitionBuilder :
        IElementMetadataBuilder<IDefaultTransitionBuilder>,
        ITargetedTransitionUtils<IDefaultTransitionBuilder>,
        IDefaultEffect<IDefaultTransitionBuilder>,
        IDefaultGuard<IDefaultTransitionBuilder>;
    
    public interface IOverridenDefaultTransitionEffectBuilder :
        IElementMetadataBuilder<IOverridenDefaultTransitionEffectBuilder>,
        ITargetedTransitionUtils<IOverridenDefaultTransitionEffectBuilder>,
        IDefaultEffect<IOverridenDefaultTransitionEffectBuilder>;
    
    public interface IOverridenDefaultTransitionBuilder :
        IElementMetadataBuilder<IOverridenDefaultTransitionBuilder>,
        ITargetedTransitionUtils<IOverridenDefaultTransitionBuilder>,
        IDefaultEffect<IOverridenDefaultTransitionBuilder>,
        IDefaultGuard<IOverridenDefaultTransitionBuilder>;
}
