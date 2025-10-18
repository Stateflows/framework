using Stateflows.StateMachines.Registration.Interfaces.Base;

namespace Stateflows.StateMachines
{
    public interface IInitializedStateMachineElementsBuilder :
        IStateMachineElements<IInitializedStateMachineElementsBuilder>,
        IStateMachineFinal<IFinalizedStateMachineBuilder>,
        IStateMachineUtils<IInitializedStateMachineElementsBuilder>,
        IStateMachineEvents<IInitializedStateMachineElementsBuilder>
    { }

    public interface IFinalizedStateMachineBuilder :
        IStateMachineUtils<IFinalizedStateMachineBuilder>,
        IStateMachineEvents<IFinalizedStateMachineBuilder>
    { }

    public interface IStateMachineBuilder :
        IStateMachineOverride<IOverridenStateMachineElementsBuilder>,
        IStateMachineInitial<IInitializedStateMachineElementsBuilder>,
        IStateMachineUtils<IStateMachineBuilder>,
        IStateMachineEvents<IStateMachineBuilder>
    { }
    
    public interface IOverridenStateMachineElementsBuilder :
        IStateMachineElements<IOverridenStateMachineElementsBuilder>,
        IStateMachineFinal<IFinalizedOverridenStateMachineElementsBuilder>,
        IStateMachineOverrides<IOverridenStateMachineElementsBuilder>,
        IStateMachineUtils<IOverridenStateMachineElementsBuilder>,
        IStateMachineEvents<IOverridenStateMachineElementsBuilder>
    { }
    
    public interface IFinalizedOverridenStateMachineElementsBuilder :
        IStateMachineElements<IFinalizedOverridenStateMachineElementsBuilder>,
        IStateMachineUtils<IFinalizedOverridenStateMachineElementsBuilder>,
        IStateMachineEvents<IFinalizedOverridenStateMachineElementsBuilder>,
        IStateMachineOverrides<IFinalizedOverridenStateMachineElementsBuilder>
    { }
}
