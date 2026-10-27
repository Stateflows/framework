using Stateflows.Common.Interfaces;
using Stateflows.StateMachines.Registration.Interfaces.Base;

namespace Stateflows.StateMachines.Registration.Interfaces
{
    /// <summary>Configures effects on an unguarded completion path.</summary>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-transitions#logic" />
    public interface IDefaultTransitionEffectBuilder :
        IElementMetadataBuilder<IDefaultTransitionEffectBuilder>,
        ITargetedTransitionUtils<IDefaultTransitionEffectBuilder>,
        IDefaultEffect<IDefaultTransitionEffectBuilder>;
    
    /// <summary>Configures guards and effects for a transition evaluated automatically on state completion.</summary>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-default-transition" />
    public interface IDefaultTransitionBuilder :
        IElementMetadataBuilder<IDefaultTransitionBuilder>,
        ITargetedTransitionUtils<IDefaultTransitionBuilder>,
        IDefaultEffect<IDefaultTransitionBuilder>,
        IDefaultGuard<IDefaultTransitionBuilder>;
    
    /// <summary>Extends the effects of an inherited unguarded completion path.</summary>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-transitions#logic" />
    /// <seealso href="https://github.com/Stateflows/framework/wiki/behaviors#definition-reuse-and-versions" />
    public interface IOverridenDefaultTransitionEffectBuilder :
        IElementMetadataBuilder<IOverridenDefaultTransitionEffectBuilder>,
        ITargetedTransitionUtils<IOverridenDefaultTransitionEffectBuilder>,
        IDefaultEffect<IOverridenDefaultTransitionEffectBuilder>;
    
    /// <summary>Extends the guards and effects of an inherited completion transition.</summary>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-default-transition" />
    /// <seealso href="https://github.com/Stateflows/framework/wiki/behaviors#definition-reuse-and-versions" />
    public interface IOverridenDefaultTransitionBuilder :
        IElementMetadataBuilder<IOverridenDefaultTransitionBuilder>,
        ITargetedTransitionUtils<IOverridenDefaultTransitionBuilder>,
        IDefaultEffect<IOverridenDefaultTransitionBuilder>,
        IDefaultGuard<IOverridenDefaultTransitionBuilder>;
}
