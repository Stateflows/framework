using Stateflows.StateMachines.Registration.Interfaces.Base;

namespace Stateflows.StateMachines.Registration.Interfaces
{
    /// <summary>Configures a regular state's actions, outgoing transitions, deferrals, and optional nested structure or
    /// do behavior.</summary>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-state" />
    public interface IStateBuilder :
        IStateEvents<IStateBuilder>,
        IStateUtils<IStateBuilder>,
        IStateTransitions<IStateBuilder>,
        IStateSubmachine<IBehaviorStateBuilder>,
        IStateDoActivity<IBehaviorStateBuilder>,
        IStateDoAction<IBehaviorStateBuilder>;
    
    /// <summary>Extends an inherited regular state, including existing transitions and deferrals.</summary>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-state" />
    /// <seealso href="https://github.com/Stateflows/framework/wiki/behaviors#definition-reuse-and-versions" />
    public interface IOverridenStateBuilder :
        IStateExtension<IOverridenStateBuilder>,
        IStateEvents<IOverridenStateBuilder>,
        IStateUtils<IOverridenStateBuilder>,
        IStateUtilsOverrides<IOverridenStateBuilder>,
        IStateTransitions<IOverridenStateBuilder>,
        IStateTransitionsOverrides<IOverridenStateBuilder>,
        IStateComposition<IOverridenRegionalizedStateBuilder>,
        IStateOrthogonalization<IOverridenRegionalizedStateBuilder>,
        IStateSubmachine<IBehaviorOverridenStateBuilder>,
        IStateDoActivity<IBehaviorOverridenStateBuilder>,
        IStateDoAction<IBehaviorOverridenStateBuilder>;
    
    /// <summary>Continues configuring an inherited state after its region structure has been established.</summary>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-regions" />
    /// <seealso href="https://github.com/Stateflows/framework/wiki/behaviors#definition-reuse-and-versions" />
    public interface IOverridenRegionalizedStateBuilder :
        IStateExtension<IOverridenRegionalizedStateBuilder>,
        IStateEvents<IOverridenRegionalizedStateBuilder>,
        IStateUtils<IOverridenRegionalizedStateBuilder>,
        IStateUtilsOverrides<IOverridenRegionalizedStateBuilder>,
        IStateTransitions<IOverridenRegionalizedStateBuilder>,
        IStateTransitionsOverrides<IOverridenRegionalizedStateBuilder>,
        IStateSubmachine<IBehaviorOverridenRegionalizedStateBuilder>,
        IStateDoActivity<IBehaviorOverridenRegionalizedStateBuilder>,
        IStateDoAction<IBehaviorOverridenRegionalizedStateBuilder>;

    /// <summary>Continues configuring a state after its single embedded do behavior has been assigned.</summary>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/behavioral-groups" />
    public interface IBehaviorStateBuilder :
        IStateEvents<IBehaviorStateBuilder>,
        IStateUtils<IBehaviorStateBuilder>,
        IStateTransitions<IBehaviorStateBuilder>;

    /// <summary>Extends an inherited regular state whose embedded do behavior is already assigned.</summary>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-state" />
    /// <seealso href="https://github.com/Stateflows/framework/wiki/behaviors#definition-reuse-and-versions" />
    public interface IBehaviorOverridenStateBuilder :
        IStateExtension<IBehaviorOverridenStateBuilder>,
        IStateEvents<IBehaviorOverridenStateBuilder>,
        IStateUtils<IBehaviorOverridenStateBuilder>,
        IStateUtilsOverrides<IBehaviorOverridenStateBuilder>,
        IStateTransitions<IBehaviorOverridenStateBuilder>,
        IStateTransitionsOverrides<IBehaviorOverridenStateBuilder>,
        IStateComposition<IBehaviorOverridenRegionalizedStateBuilder>,
        IStateOrthogonalization<IBehaviorOverridenRegionalizedStateBuilder>;

    /// <summary>Continues configuration of an inherited state with established regions and an assigned do behavior.
    /// </summary>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-regions" />
    /// <seealso href="https://github.com/Stateflows/framework/wiki/behaviors#definition-reuse-and-versions" />
    public interface IBehaviorOverridenRegionalizedStateBuilder :
        IStateExtension<IBehaviorOverridenRegionalizedStateBuilder>,
        IStateEvents<IBehaviorOverridenRegionalizedStateBuilder>,
        IStateUtils<IBehaviorOverridenRegionalizedStateBuilder>,
        IStateUtilsOverrides<IBehaviorOverridenRegionalizedStateBuilder>,
        IStateTransitions<IBehaviorOverridenRegionalizedStateBuilder>,
        IStateTransitionsOverrides<IBehaviorOverridenRegionalizedStateBuilder>;
}
