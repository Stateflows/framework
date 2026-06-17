using Stateflows.StateMachines.Registration.Interfaces.Base;

namespace Stateflows.StateMachines
{
    public interface IInitializedStateMachineBuilder :
        IStateMachineElements<IInitializedStateMachineBuilder>,
        IStateMachineFinal<IFinalizedStateMachineBuilder>,
        IStateMachineUtils<IInitializedStateMachineBuilder>,
        IStateMachineEvents<IInitializedStateMachineBuilder>;

    public interface IFinalizedStateMachineBuilder :
        IStateMachineUtils<IFinalizedStateMachineBuilder>,
        IStateMachineEvents<IFinalizedStateMachineBuilder>;

    public interface IStateMachineBuilder :
        IStateMachineOverride<IOverridenStateMachineBuilder>,
        IStateMachineInitial<IInitializedStateMachineBuilder>,
        IStateMachineUtils<IStateMachineBuilder>,
        IStateMachineEvents<IStateMachineBuilder>;
    
    public interface IOverridenStateMachineBuilder :
        IStateMachineElements<IOverridenStateMachineBuilder>,
        IStateMachineFinal<IFinalizedOverridenStateMachineBuilder>,
        IStateMachineOverrides<IOverridenStateMachineBuilder>,
        IStateMachineUtils<IOverridenStateMachineBuilder>,
        IStateMachineEvents<IOverridenStateMachineBuilder>;
    
    public interface IFinalizedOverridenStateMachineBuilder :
        IStateMachineElements<IFinalizedOverridenStateMachineBuilder>,
        IStateMachineUtils<IFinalizedOverridenStateMachineBuilder>,
        IStateMachineEvents<IFinalizedOverridenStateMachineBuilder>,
        IStateMachineOverrides<IFinalizedOverridenStateMachineBuilder>;
}
