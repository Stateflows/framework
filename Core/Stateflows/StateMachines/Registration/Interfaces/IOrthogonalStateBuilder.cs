using Stateflows.StateMachines.Registration.Interfaces.Base;

namespace Stateflows.StateMachines.Registration.Interfaces
{
    /// <summary>Defines an orthogonal state's independently active regions, lifecycle actions, transitions, and
    /// optional do behavior.</summary>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-orthogonal-state" />
    public interface IOrthogonalStateBuilder :
        IOrthogonalStateExtension<IOrthogonalStateBuilder>,
        IStateEvents<IOrthogonalStateBuilder>,
        IOrthogonalStateTypedEvents<IOrthogonalStateBuilder>,
        IStateUtils<IOrthogonalStateBuilder>,
        IRegions<IOrthogonalStateBuilder>,
        IStateTransitions<IOrthogonalStateBuilder>,
        IStateDoActivity<IBehaviorOrthogonalStateBuilder>,
        IStateDoAction<IBehaviorOrthogonalStateBuilder>;

    /// <summary>Extends an inherited orthogonal state and its existing or additional regions.</summary>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-orthogonal-state" />
    /// <seealso href="https://github.com/Stateflows/framework/wiki/behaviors#definition-reuse-and-versions" />
    public interface IOverridenOrthogonalStateBuilder :
        IOrthogonalStateExtension<IOverridenOrthogonalStateBuilder>,
        IStateEvents<IOverridenOrthogonalStateBuilder>,
        IOrthogonalStateTypedEvents<IOverridenOrthogonalStateBuilder>,
        IStateUtils<IOverridenOrthogonalStateBuilder>,
        IStateUtilsOverrides<IOverridenOrthogonalStateBuilder>,
        IRegions<IOverridenOrthogonalStateBuilder>,
        IRegionsOverrides<IOverridenOrthogonalStateBuilder>,
        IStateTransitions<IOverridenOrthogonalStateBuilder>,
        IStateTransitionsOverrides<IOverridenOrthogonalStateBuilder>,
        IStateDoActivity<IBehaviorOverridenOrthogonalStateBuilder>,
        IStateDoAction<IBehaviorOverridenOrthogonalStateBuilder>;

    /// <summary>Continues configuring an orthogonal state after assigning its single embedded do behavior.</summary>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-orthogonal-state" />
    public interface IBehaviorOrthogonalStateBuilder :
        IOrthogonalStateExtension<IBehaviorOrthogonalStateBuilder>,
        IStateEvents<IBehaviorOrthogonalStateBuilder>,
        IOrthogonalStateTypedEvents<IBehaviorOrthogonalStateBuilder>,
        IStateUtils<IBehaviorOrthogonalStateBuilder>,
        IRegions<IBehaviorOrthogonalStateBuilder>,
        IStateTransitions<IBehaviorOrthogonalStateBuilder>;

    /// <summary>Extends an inherited orthogonal state whose embedded do behavior is assigned.</summary>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-orthogonal-state" />
    /// <seealso href="https://github.com/Stateflows/framework/wiki/behaviors#definition-reuse-and-versions" />
    public interface IBehaviorOverridenOrthogonalStateBuilder :
        IOrthogonalStateExtension<IBehaviorOverridenOrthogonalStateBuilder>,
        IStateEvents<IBehaviorOverridenOrthogonalStateBuilder>,
        IOrthogonalStateTypedEvents<IBehaviorOverridenOrthogonalStateBuilder>,
        IStateUtils<IBehaviorOverridenOrthogonalStateBuilder>,
        IStateUtilsOverrides<IBehaviorOverridenOrthogonalStateBuilder>,
        IRegions<IBehaviorOverridenOrthogonalStateBuilder>,
        IRegionsOverrides<IBehaviorOverridenOrthogonalStateBuilder>,
        IStateTransitions<IBehaviorOverridenOrthogonalStateBuilder>,
        IStateTransitionsOverrides<IBehaviorOverridenOrthogonalStateBuilder>;
}
