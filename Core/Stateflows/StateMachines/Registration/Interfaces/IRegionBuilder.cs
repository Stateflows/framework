using Stateflows.StateMachines.Registration.Interfaces.Base;

namespace Stateflows.StateMachines.Registration.Interfaces
{
    public interface IInitializedRegionBuilder :
        IStateMachineElements<IInitializedRegionBuilder>,
        IStateHistory<IInitializedRegionBuilder>,
        IStateMachineFinal<IFinalizedRegionBuilder>,
        IStateDoActivity<IBehaviorInitializedRegionBuilder>,
        IStateDoAction<IBehaviorInitializedRegionBuilder>;

    public interface IFinalizedRegionBuilder :
        IStateDoActivity<IBehaviorFinalizedRegionBuilder>,
        IStateDoAction<IBehaviorFinalizedRegionBuilder>;

    public interface IRegionBuilder :
        IStateMachineInitial<IInitializedRegionBuilder>,
        IStateMachineElements<IInitializedRegionBuilder>,
        IStateHistory<IRegionBuilder>,
        IStateDoActivity<IBehaviorRegionBuilder>,
        IStateDoAction<IBehaviorRegionBuilder>;

    public interface IFinalizedOverridenRegionBuilder :
        IStateMachineOverrides<IFinalizedOverridenRegionBuilder>,
        IStateDoActivity<IBehaviorFinalizedOverridenRegionBuilder>,
        IStateDoAction<IBehaviorFinalizedOverridenRegionBuilder>;

    public interface IOverridenRegionBuilder :
        IStateMachineElements<IOverridenRegionBuilder>,
        IStateHistory<IOverridenRegionBuilder>,
        IStateMachineOverrides<IOverridenRegionBuilder>,
        IStateMachineFinal<IFinalizedOverridenRegionBuilder>,
        IStateDoActivity<IBehaviorOverridenRegionBuilder>,
        IStateDoAction<IBehaviorOverridenRegionBuilder>;
    
    
    
    
    public interface IBehaviorInitializedRegionBuilder :
        IStateMachineElements<IBehaviorInitializedRegionBuilder>,
        IStateHistory<IBehaviorInitializedRegionBuilder>,
        IStateMachineFinal<IBehaviorFinalizedRegionBuilder>;

    public interface IBehaviorFinalizedRegionBuilder;

    public interface IBehaviorRegionBuilder :
        IStateMachineInitial<IBehaviorInitializedRegionBuilder>,
        IStateMachineElements<IBehaviorInitializedRegionBuilder>,
        IStateHistory<IBehaviorRegionBuilder>;

    public interface IBehaviorFinalizedOverridenRegionBuilder :
        IStateMachineOverrides<IBehaviorFinalizedOverridenRegionBuilder>;

    public interface IBehaviorOverridenRegionBuilder :
        IStateMachineElements<IBehaviorOverridenRegionBuilder>,
        IStateHistory<IBehaviorOverridenRegionBuilder>,
        IStateMachineOverrides<IBehaviorOverridenRegionBuilder>,
        IStateMachineFinal<IBehaviorFinalizedOverridenRegionBuilder>;
}
