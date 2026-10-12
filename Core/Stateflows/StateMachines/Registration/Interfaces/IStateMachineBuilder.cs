using Stateflows.Common.Interfaces;
using Stateflows.Common.Registration.Interfaces;
using Stateflows.StateMachines.Registration.Interfaces.Base;

namespace Stateflows.StateMachines
{
    public interface IInitializedStateMachineBuilder :
        IStateMachineElements<IInitializedStateMachineBuilder>,
        IStateMachineFinal<IFinalizedStateMachineBuilder>,
        IStateMachineUtils<IInitializedStateMachineBuilder>,
        IStateMachineEvents<IInitializedStateMachineBuilder>,
        IEntityOwnerBase<IInitializedStateMachineBuilder>;

    public interface IFinalizedStateMachineBuilder :
        IStateMachineUtils<IFinalizedStateMachineBuilder>,
        IStateMachineEvents<IFinalizedStateMachineBuilder>,
        IEntityOwnerBase<IFinalizedStateMachineBuilder>;

    public interface IStateMachineBuilder :
        IStateMachineOverride<IOverridenStateMachineBuilder>,
        IStateMachineInitial<IInitializedStateMachineBuilder>,
        IStateMachineUtils<IStateMachineBuilder>,
        IStateMachineEvents<IStateMachineBuilder>,
        IEntityOwnerBase<IStateMachineBuilder>;
    
    public interface IOverridenStateMachineBuilder :
        IStateMachineElements<IOverridenStateMachineBuilder>,
        IStateMachineFinal<IFinalizedOverridenStateMachineBuilder>,
        IStateMachineOverrides<IOverridenStateMachineBuilder>,
        IStateMachineUtils<IOverridenStateMachineBuilder>,
        IStateMachineEvents<IOverridenStateMachineBuilder>,
        IEntityOwnerBase<IOverridenStateMachineBuilder>;
    
    public interface IFinalizedOverridenStateMachineBuilder :
        IStateMachineElements<IFinalizedOverridenStateMachineBuilder>,
        IStateMachineUtils<IFinalizedOverridenStateMachineBuilder>,
        IStateMachineEvents<IFinalizedOverridenStateMachineBuilder>,
        IStateMachineOverrides<IFinalizedOverridenStateMachineBuilder>,
        IEntityOwnerBase<IFinalizedOverridenStateMachineBuilder>;
}
