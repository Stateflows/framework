using Stateflows.StateMachines.Registration.Interfaces.Base;

namespace Stateflows.StateMachines.Registration.Interfaces
{
    public interface IOrthogonalStateBuilder :
        IOrthogonalStateExtension<IOrthogonalStateBuilder>,
        IStateEvents<IOrthogonalStateBuilder>,
        IOrthogonalStateTypedEvents<IOrthogonalStateBuilder>,
        IStateUtils<IOrthogonalStateBuilder>,
        IRegions<IOrthogonalStateBuilder>,
        IStateTransitions<IOrthogonalStateBuilder>,
        IStateDoActivity<IBehaviorOrthogonalStateBuilder>,
        IStateDoAction<IBehaviorOrthogonalStateBuilder>;

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

    public interface IBehaviorOrthogonalStateBuilder :
        IOrthogonalStateExtension<IBehaviorOrthogonalStateBuilder>,
        IStateEvents<IBehaviorOrthogonalStateBuilder>,
        IOrthogonalStateTypedEvents<IBehaviorOrthogonalStateBuilder>,
        IStateUtils<IBehaviorOrthogonalStateBuilder>,
        IRegions<IBehaviorOrthogonalStateBuilder>,
        IStateTransitions<IBehaviorOrthogonalStateBuilder>;

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
