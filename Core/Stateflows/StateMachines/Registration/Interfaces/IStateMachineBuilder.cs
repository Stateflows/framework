using Stateflows.Common.Interfaces;
using Stateflows.Common.Registration.Interfaces;
using Stateflows.StateMachines.Registration.Interfaces.Base;

namespace Stateflows.StateMachines
{
    /// <summary>Continues a machine definition after its initial vertex is selected, allowing further elements and one
    /// final state.</summary>
    /// <remarks>The name describes the fluent definition stage, not a running machine's status.</remarks>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machines" />
    public interface IInitializedStateMachineBuilder :
        IStateMachineElements<IInitializedStateMachineBuilder>,
        IStateMachineFinal<IFinalizedStateMachineBuilder>,
        IStateMachineUtils<IInitializedStateMachineBuilder>,
        IStateMachineEvents<IInitializedStateMachineBuilder>,
        IEntityOwnerBase<IInitializedStateMachineBuilder>;

    /// <summary>Continues configuration of lifecycle, middleware, and entities after a final state closes structural
    /// additions.</summary>
    /// <remarks>This is a registration-time builder stage, not a finalized runtime instance.</remarks>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machines" />
    public interface IFinalizedStateMachineBuilder :
        IStateMachineUtils<IFinalizedStateMachineBuilder>,
        IStateMachineEvents<IFinalizedStateMachineBuilder>,
        IEntityOwnerBase<IFinalizedStateMachineBuilder>;

    /// <summary>Starts a fluent machine definition by selecting an initial vertex or reusing a typed definition.
    /// </summary>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machines" />
    public interface IStateMachineBuilder :
        IStateMachineOverride<IOverridenStateMachineBuilder>,
        IStateMachineInitial<IInitializedStateMachineBuilder>,
        IStateMachineUtils<IStateMachineBuilder>,
        IStateMachineEvents<IStateMachineBuilder>,
        IEntityOwnerBase<IStateMachineBuilder>;
    
    /// <summary>Extends a reused machine definition with additional elements or overrides of inherited elements.
    /// </summary>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machines" />
    /// <seealso href="https://github.com/Stateflows/framework/wiki/behaviors#definition-reuse-and-versions" />
    public interface IOverridenStateMachineBuilder :
        IStateMachineElements<IOverridenStateMachineBuilder>,
        IStateMachineFinal<IFinalizedOverridenStateMachineBuilder>,
        IStateMachineOverrides<IOverridenStateMachineBuilder>,
        IStateMachineUtils<IOverridenStateMachineBuilder>,
        IStateMachineEvents<IOverridenStateMachineBuilder>,
        IEntityOwnerBase<IOverridenStateMachineBuilder>;
    
    /// <summary>Configures inherited elements, lifecycle, and utilities after adding a final state to a reused
    /// definition.</summary>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machines" />
    public interface IFinalizedOverridenStateMachineBuilder :
        IStateMachineUtils<IFinalizedOverridenStateMachineBuilder>,
        IStateMachineEvents<IFinalizedOverridenStateMachineBuilder>,
        IStateMachineOverrides<IFinalizedOverridenStateMachineBuilder>,
        IEntityOwnerBase<IFinalizedOverridenStateMachineBuilder>;
}
