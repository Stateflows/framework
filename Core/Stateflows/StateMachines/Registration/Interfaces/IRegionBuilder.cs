using Stateflows.StateMachines.Registration.Interfaces.Base;

namespace Stateflows.StateMachines.Registration.Interfaces
{
    /// <summary>Continues defining a region after selecting its initial vertex.</summary>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-regions" />
    public interface IInitializedRegionBuilder :
        IStateMachineElements<IInitializedRegionBuilder>,
        IStateHistory<IInitializedRegionBuilder>,
        IStateMachineFinal<IFinalizedRegionBuilder>;

    /// <summary>Marks completion of a region's structural definition after adding its final state.</summary>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-regions" />
    public interface IFinalizedRegionBuilder;

    /// <summary>Starts one orthogonal region's definition by selecting its initial vertex.</summary>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-regions" />
    public interface IRegionBuilder :
        IStateMachineInitial<IInitializedRegionBuilder>,
        IStateMachineElements<IInitializedRegionBuilder>,
        IStateHistory<IRegionBuilder>;

    /// <summary>Allows inherited-element overrides after adding a final state to an inherited region.</summary>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-regions" />
    public interface IFinalizedOverridenRegionBuilder :
        IStateMachineOverrides<IFinalizedOverridenRegionBuilder>;

    /// <summary>Extends an inherited region with new elements and overrides of its existing elements.</summary>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-regions" />
    /// <seealso href="https://github.com/Stateflows/framework/wiki/behaviors#definition-reuse-and-versions" />
    public interface IOverridenRegionBuilder :
        IStateMachineElements<IOverridenRegionBuilder>,
        IStateHistory<IOverridenRegionBuilder>,
        IStateMachineOverrides<IOverridenRegionBuilder>,
        IStateMachineFinal<IFinalizedOverridenRegionBuilder>;
    
    
    
    
    /// <summary>Continues an initialized region definition in an embedded-behavior builder chain.</summary>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-regions" />
    public interface IBehaviorInitializedRegionBuilder :
        IStateMachineElements<IBehaviorInitializedRegionBuilder>,
        IStateHistory<IBehaviorInitializedRegionBuilder>,
        IStateMachineFinal<IBehaviorFinalizedRegionBuilder>;

    /// <summary>Marks completion of a region definition in an embedded-behavior builder chain.</summary>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-regions" />
    public interface IBehaviorFinalizedRegionBuilder;

    /// <summary>Starts a region definition in an embedded-behavior builder chain.</summary>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-regions" />
    public interface IBehaviorRegionBuilder :
        IStateMachineInitial<IBehaviorInitializedRegionBuilder>,
        IStateMachineElements<IBehaviorInitializedRegionBuilder>,
        IStateHistory<IBehaviorRegionBuilder>;

    /// <summary>Allows inherited-element overrides after finalizing a region definition in an embedded-behavior chain.
    /// </summary>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-regions" />
    public interface IBehaviorFinalizedOverridenRegionBuilder :
        IStateMachineOverrides<IBehaviorFinalizedOverridenRegionBuilder>;

    /// <summary>Extends an inherited region in an embedded-behavior builder chain.</summary>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-regions" />
    /// <seealso href="https://github.com/Stateflows/framework/wiki/behaviors#definition-reuse-and-versions" />
    public interface IBehaviorOverridenRegionBuilder :
        IStateMachineElements<IBehaviorOverridenRegionBuilder>,
        IStateHistory<IBehaviorOverridenRegionBuilder>,
        IStateMachineOverrides<IBehaviorOverridenRegionBuilder>,
        IStateMachineFinal<IBehaviorFinalizedOverridenRegionBuilder>;
}
