using Stateflows.StateMachines.Registration.Interfaces.Base;

namespace Stateflows.StateMachines
{
    public interface IInitializedStateMachineBuilder :
        IStateMachineElements<IInitializedStateMachineBuilder>,
        IStateMachineFinal<IFinalizedStateMachineBuilder>,
        IStateMachineUtils<IInitializedStateMachineBuilder>,
        IStateMachineEvents<IInitializedStateMachineBuilder>,
        IStateMachineEntity<IInitializedStateMachineWithEntityBuilder>;

    public interface IFinalizedStateMachineBuilder :
        IStateMachineUtils<IFinalizedStateMachineBuilder>,
        IStateMachineEvents<IFinalizedStateMachineBuilder>,
        IStateMachineEntity<IFinalizedStateMachineWithEntityBuilder>;

    public interface IStateMachineBuilder :
        IStateMachineOverride<IOverridenStateMachineBuilder>,
        IStateMachineInitial<IInitializedStateMachineBuilder>,
        IStateMachineUtils<IStateMachineBuilder>,
        IStateMachineEvents<IStateMachineBuilder>,
        IStateMachineEntity<IStateMachineWithEntityBuilder>;
    
    public interface IOverridenStateMachineBuilder :
        IStateMachineElements<IOverridenStateMachineBuilder>,
        IStateMachineFinal<IFinalizedOverridenStateMachineBuilder>,
        IStateMachineOverrides<IOverridenStateMachineBuilder>,
        IStateMachineUtils<IOverridenStateMachineBuilder>,
        IStateMachineEvents<IOverridenStateMachineBuilder>,
        IStateMachineEntity<IOverridenStateMachineWithEntityBuilder>;
    
    public interface IFinalizedOverridenStateMachineBuilder :
        IStateMachineElements<IFinalizedOverridenStateMachineBuilder>,
        IStateMachineUtils<IFinalizedOverridenStateMachineBuilder>,
        IStateMachineEvents<IFinalizedOverridenStateMachineBuilder>,
        IStateMachineOverrides<IFinalizedOverridenStateMachineBuilder>,
        IStateMachineEntity<IFinalizedOverridenStateMachineWithEntityBuilder>;
    
    public interface IInitializedStateMachineWithEntityBuilder :
        IStateMachineElements<IInitializedStateMachineWithEntityBuilder>,
        IStateMachineFinal<IFinalizedStateMachineWithEntityBuilder>,
        IStateMachineUtils<IInitializedStateMachineWithEntityBuilder>,
        IStateMachineEvents<IInitializedStateMachineWithEntityBuilder>;

    public interface IFinalizedStateMachineWithEntityBuilder :
        IStateMachineUtils<IFinalizedStateMachineWithEntityBuilder>,
        IStateMachineEvents<IFinalizedStateMachineWithEntityBuilder>;

    public interface IStateMachineWithEntityBuilder :
        IStateMachineOverride<IOverridenStateMachineWithEntityBuilder>,
        IStateMachineInitial<IInitializedStateMachineBuilder>,
        IStateMachineUtils<IStateMachineWithEntityBuilder>,
        IStateMachineEvents<IStateMachineWithEntityBuilder>;
    
    public interface IOverridenStateMachineWithEntityBuilder :
        IStateMachineElements<IOverridenStateMachineWithEntityBuilder>,
        IStateMachineFinal<IFinalizedOverridenStateMachineWithEntityBuilder>,
        IStateMachineOverrides<IOverridenStateMachineWithEntityBuilder>,
        IStateMachineUtils<IOverridenStateMachineWithEntityBuilder>,
        IStateMachineEvents<IOverridenStateMachineWithEntityBuilder>;
    
    public interface IFinalizedOverridenStateMachineWithEntityBuilder :
        IStateMachineElements<IFinalizedOverridenStateMachineWithEntityBuilder>,
        IStateMachineUtils<IFinalizedOverridenStateMachineWithEntityBuilder>,
        IStateMachineEvents<IFinalizedOverridenStateMachineWithEntityBuilder>,
        IStateMachineOverrides<IFinalizedOverridenStateMachineWithEntityBuilder>;
}
