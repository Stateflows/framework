using Stateflows.StateMachines.Registration.Interfaces.Base;

namespace Stateflows.StateMachines.Registration.Interfaces
{
    /// <summary>Defines a composite state's lifecycle, outgoing transitions, and nested region before selecting its
    /// initial vertex.</summary>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-composite-state" />
    public interface ICompositeStateBuilder :
        ICompositeStateExtension<ICompositeStateBuilder>,
        IStateEvents<ICompositeStateBuilder>,
        IStateUtils<ICompositeStateBuilder>,
        ICompositeStateTypedEvents<ICompositeStateBuilder>,
        IStateTransitions<ICompositeStateBuilder>,
        IStateMachineInitial<IInitializedCompositeStateBuilder>,
        IStateMachineElements<IInitializedCompositeStateBuilder>,
        IStateHistory<ICompositeStateBuilder>,
        IStateDoActivity<IBehaviorCompositeStateBuilder>,
        IStateDoAction<IBehaviorCompositeStateBuilder>;
    
    /// <summary>Continues a composite definition after its initial vertex is selected, allowing nested vertices and a
    /// final state.</summary>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-composite-state" />
    public interface IInitializedCompositeStateBuilder :
        ICompositeStateExtension<IInitializedCompositeStateBuilder>,
        IStateEvents<IInitializedCompositeStateBuilder>,
        IStateUtils<IInitializedCompositeStateBuilder>,
        ICompositeStateTypedEvents<IInitializedCompositeStateBuilder>,
        IStateTransitions<IInitializedCompositeStateBuilder>,
        IStateMachineElements<IInitializedCompositeStateBuilder>,
        IStateHistory<IInitializedCompositeStateBuilder>,
        IStateMachineFinal<IFinalizedCompositeStateBuilder>,
        IStateDoActivity<IBehaviorInitializedCompositeStateBuilder>,
        IStateDoAction<IBehaviorInitializedCompositeStateBuilder>;

    /// <summary>Configures the enclosing composite state's behavior after its final state closes nested structural
    /// additions.</summary>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-composite-state" />
    public interface IFinalizedCompositeStateBuilder :
        ICompositeStateExtension<IFinalizedCompositeStateBuilder>,
        IStateEvents<IFinalizedCompositeStateBuilder>,
        IStateUtils<IFinalizedCompositeStateBuilder>,
        ICompositeStateTypedEvents<IFinalizedCompositeStateBuilder>,
        IStateTransitions<IFinalizedCompositeStateBuilder>,
        IStateDoActivity<IBehaviorFinalizedCompositeStateBuilder>,
        IStateDoAction<IBehaviorFinalizedCompositeStateBuilder>;

    /// <summary>Extends an inherited composite state, its nested elements, transitions, and deferrals.</summary>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-composite-state" />
    /// <seealso href="https://github.com/Stateflows/framework/wiki/behaviors#definition-reuse-and-versions" />
    public interface IOverridenCompositeStateBuilder :
        ICompositeStateExtension<IOverridenCompositeStateBuilder>,
        IStateEvents<IOverridenCompositeStateBuilder>,
        IStateUtils<IOverridenCompositeStateBuilder>,
        IStateUtilsOverrides<IOverridenCompositeStateBuilder>,
        ICompositeStateTypedEvents<IOverridenCompositeStateBuilder>,
        IStateTransitions<IOverridenCompositeStateBuilder>,
        IStateTransitionsOverrides<IOverridenCompositeStateBuilder>,
        IStateMachineElements<IOverridenCompositeStateBuilder>,
        IStateHistory<IOverridenCompositeStateBuilder>,
        IStateMachineOverrides<IOverridenCompositeStateBuilder>,
        IStateOrthogonalization<IOverridenRegionalizedCompositeStateBuilder>,
        IStateMachineFinal<IFinalizedOverridenCompositeStateBuilder>,
        IStateDoActivity<IBehaviorOverridenCompositeStateBuilder>,
        IStateDoAction<IBehaviorOverridenCompositeStateBuilder>;

    /// <summary>Continues an inherited composite definition after its regionalization operation has been used.
    /// </summary>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-regions" />
    /// <seealso href="https://github.com/Stateflows/framework/wiki/behaviors#definition-reuse-and-versions" />
    public interface IOverridenRegionalizedCompositeStateBuilder :
        ICompositeStateExtension<IOverridenRegionalizedCompositeStateBuilder>,
        IStateEvents<IOverridenRegionalizedCompositeStateBuilder>,
        IStateUtils<IOverridenRegionalizedCompositeStateBuilder>,
        IStateUtilsOverrides<IOverridenRegionalizedCompositeStateBuilder>,
        ICompositeStateTypedEvents<IOverridenRegionalizedCompositeStateBuilder>,
        IStateTransitions<IOverridenRegionalizedCompositeStateBuilder>,
        IStateTransitionsOverrides<IOverridenRegionalizedCompositeStateBuilder>,
        IStateMachineElements<IOverridenRegionalizedCompositeStateBuilder>,
        IStateHistory<IOverridenRegionalizedCompositeStateBuilder>,
        IStateMachineOverrides<IOverridenRegionalizedCompositeStateBuilder>,
        IStateMachineFinal<IFinalizedOverridenRegionalizedCompositeStateBuilder>,
        IStateDoActivity<IBehaviorOverridenRegionalizedCompositeStateBuilder>,
        IStateDoAction<IBehaviorOverridenRegionalizedCompositeStateBuilder>;

    /// <summary>Configures an inherited composite state after adding its final state, retaining inherited-element
    /// overrides.</summary>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-composite-state" />
    public interface IFinalizedOverridenCompositeStateBuilder :
        ICompositeStateExtension<IFinalizedOverridenCompositeStateBuilder>,
        IStateEvents<IFinalizedOverridenCompositeStateBuilder>,
        IStateUtils<IFinalizedOverridenCompositeStateBuilder>,
        IStateUtilsOverrides<IFinalizedOverridenCompositeStateBuilder>,
        ICompositeStateTypedEvents<IFinalizedOverridenCompositeStateBuilder>,
        IStateTransitions<IFinalizedOverridenCompositeStateBuilder>,
        IStateTransitionsOverrides<IFinalizedOverridenCompositeStateBuilder>,
        IStateMachineOverrides<IFinalizedOverridenCompositeStateBuilder>,
        IStateOrthogonalization<IFinalizedOverridenRegionalizedCompositeStateBuilder>,
        IStateDoActivity<IBehaviorFinalizedOverridenCompositeStateBuilder>,
        IStateDoAction<IBehaviorFinalizedOverridenCompositeStateBuilder>;

    /// <summary>Configures an inherited composite state after final-state selection and regionalization.</summary>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-regions" />
    public interface IFinalizedOverridenRegionalizedCompositeStateBuilder :
        ICompositeStateExtension<IFinalizedOverridenRegionalizedCompositeStateBuilder>,
        IStateEvents<IFinalizedOverridenRegionalizedCompositeStateBuilder>,
        IStateUtils<IFinalizedOverridenRegionalizedCompositeStateBuilder>,
        IStateUtilsOverrides<IFinalizedOverridenRegionalizedCompositeStateBuilder>,
        ICompositeStateTypedEvents<IFinalizedOverridenRegionalizedCompositeStateBuilder>,
        IStateMachineOverrides<IFinalizedOverridenRegionalizedCompositeStateBuilder>,
        IStateTransitions<IFinalizedOverridenRegionalizedCompositeStateBuilder>,
        IStateTransitionsOverrides<IFinalizedOverridenRegionalizedCompositeStateBuilder>,
        IStateDoActivity<IBehaviorFinalizedOverridenRegionalizedCompositeStateBuilder>,
        IStateDoAction<IBehaviorFinalizedOverridenRegionalizedCompositeStateBuilder>;
    

    /// <summary>Continues a composite-state definition after assigning its embedded do behavior, before selecting the
    /// nested initial vertex.</summary>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-composite-state" />
    public interface IBehaviorCompositeStateBuilder :
        ICompositeStateExtension<IBehaviorCompositeStateBuilder>,
        IStateEvents<IBehaviorCompositeStateBuilder>,
        IStateUtils<IBehaviorCompositeStateBuilder>,
        ICompositeStateTypedEvents<IBehaviorCompositeStateBuilder>,
        IStateTransitions<IBehaviorCompositeStateBuilder>,
        IStateMachineInitial<IBehaviorInitializedCompositeStateBuilder>,
        IStateMachineElements<IBehaviorInitializedCompositeStateBuilder>,
        IStateHistory<IBehaviorCompositeStateBuilder>;
    
    /// <summary>Continues a composite definition with an assigned do behavior and a selected nested initial vertex.
    /// </summary>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-composite-state" />
    public interface IBehaviorInitializedCompositeStateBuilder :
        ICompositeStateExtension<IBehaviorInitializedCompositeStateBuilder>,
        IStateEvents<IBehaviorInitializedCompositeStateBuilder>,
        IStateUtils<IBehaviorInitializedCompositeStateBuilder>,
        ICompositeStateTypedEvents<IBehaviorInitializedCompositeStateBuilder>,
        IStateTransitions<IBehaviorInitializedCompositeStateBuilder>,
        IStateMachineElements<IBehaviorInitializedCompositeStateBuilder>,
        IStateHistory<IBehaviorInitializedCompositeStateBuilder>,
        IStateMachineFinal<IBehaviorFinalizedCompositeStateBuilder>;

    /// <summary>Configures a composite state's behavior after its do behavior and nested final state have been
    /// assigned.</summary>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-composite-state" />
    public interface IBehaviorFinalizedCompositeStateBuilder :
        ICompositeStateExtension<IBehaviorFinalizedCompositeStateBuilder>,
        IStateEvents<IBehaviorFinalizedCompositeStateBuilder>,
        IStateUtils<IBehaviorFinalizedCompositeStateBuilder>,
        ICompositeStateTypedEvents<IBehaviorFinalizedCompositeStateBuilder>,
        IStateTransitions<IBehaviorFinalizedCompositeStateBuilder>;

    /// <summary>Extends an inherited composite state whose embedded do behavior has already been assigned.</summary>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-composite-state" />
    /// <seealso href="https://github.com/Stateflows/framework/wiki/behaviors#definition-reuse-and-versions" />
    public interface IBehaviorOverridenCompositeStateBuilder :
        ICompositeStateExtension<IBehaviorOverridenCompositeStateBuilder>,
        IStateEvents<IBehaviorOverridenCompositeStateBuilder>,
        IStateUtils<IBehaviorOverridenCompositeStateBuilder>,
        IStateUtilsOverrides<IBehaviorOverridenCompositeStateBuilder>,
        ICompositeStateTypedEvents<IBehaviorOverridenCompositeStateBuilder>,
        IStateTransitions<IBehaviorOverridenCompositeStateBuilder>,
        IStateTransitionsOverrides<IBehaviorOverridenCompositeStateBuilder>,
        IStateMachineElements<IBehaviorOverridenCompositeStateBuilder>,
        IStateHistory<IBehaviorOverridenCompositeStateBuilder>,
        IStateMachineOverrides<IBehaviorOverridenCompositeStateBuilder>,
        IStateOrthogonalization<IBehaviorOverridenRegionalizedCompositeStateBuilder>,
        IStateMachineFinal<IBehaviorFinalizedOverridenCompositeStateBuilder>;

    /// <summary>Continues an inherited composite definition after assigning a do behavior and establishing its region
    /// structure.</summary>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-regions" />
    /// <seealso href="https://github.com/Stateflows/framework/wiki/behaviors#definition-reuse-and-versions" />
    public interface IBehaviorOverridenRegionalizedCompositeStateBuilder :
        ICompositeStateExtension<IBehaviorOverridenRegionalizedCompositeStateBuilder>,
        IStateEvents<IBehaviorOverridenRegionalizedCompositeStateBuilder>,
        IStateUtils<IBehaviorOverridenRegionalizedCompositeStateBuilder>,
        IStateUtilsOverrides<IBehaviorOverridenRegionalizedCompositeStateBuilder>,
        ICompositeStateTypedEvents<IBehaviorOverridenRegionalizedCompositeStateBuilder>,
        IStateTransitions<IBehaviorOverridenRegionalizedCompositeStateBuilder>,
        IStateTransitionsOverrides<IBehaviorOverridenRegionalizedCompositeStateBuilder>,
        IStateMachineElements<IBehaviorOverridenRegionalizedCompositeStateBuilder>,
        IStateHistory<IBehaviorOverridenRegionalizedCompositeStateBuilder>,
        IStateMachineOverrides<IBehaviorOverridenRegionalizedCompositeStateBuilder>,
        IStateMachineFinal<IBehaviorFinalizedOverridenRegionalizedCompositeStateBuilder>;

    /// <summary>Configures an inherited composite state with an assigned do behavior and a selected nested final state.
    /// </summary>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-composite-state" />
    public interface IBehaviorFinalizedOverridenCompositeStateBuilder :
        ICompositeStateExtension<IBehaviorFinalizedOverridenCompositeStateBuilder>,
        IStateEvents<IBehaviorFinalizedOverridenCompositeStateBuilder>,
        IStateUtils<IBehaviorFinalizedOverridenCompositeStateBuilder>,
        IStateUtilsOverrides<IBehaviorFinalizedOverridenCompositeStateBuilder>,
        ICompositeStateTypedEvents<IBehaviorFinalizedOverridenCompositeStateBuilder>,
        IStateTransitions<IBehaviorFinalizedOverridenCompositeStateBuilder>,
        IStateTransitionsOverrides<IBehaviorFinalizedOverridenCompositeStateBuilder>,
        IStateMachineOverrides<IBehaviorFinalizedOverridenCompositeStateBuilder>,
        IStateOrthogonalization<IBehaviorFinalizedOverridenRegionalizedCompositeStateBuilder>;

    /// <summary>Configures inherited composite behavior after do-behavior assignment, final-state selection, and
    /// regionalization.</summary>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-regions" />
    public interface IBehaviorFinalizedOverridenRegionalizedCompositeStateBuilder :
        ICompositeStateExtension<IBehaviorFinalizedOverridenRegionalizedCompositeStateBuilder>,
        IStateEvents<IBehaviorFinalizedOverridenRegionalizedCompositeStateBuilder>,
        IStateUtils<IBehaviorFinalizedOverridenRegionalizedCompositeStateBuilder>,
        IStateUtilsOverrides<IBehaviorFinalizedOverridenRegionalizedCompositeStateBuilder>,
        ICompositeStateTypedEvents<IBehaviorFinalizedOverridenRegionalizedCompositeStateBuilder>,
        IStateMachineOverrides<IBehaviorFinalizedOverridenRegionalizedCompositeStateBuilder>,
        IStateTransitions<IBehaviorFinalizedOverridenRegionalizedCompositeStateBuilder>,
        IStateTransitionsOverrides<IBehaviorFinalizedOverridenRegionalizedCompositeStateBuilder>;
}
