using Stateflows.StateMachines.Registration.Interfaces.Base;

namespace Stateflows.StateMachines.Registration.Interfaces
{
    public interface IInitializedRegionBuilder :
        IStateMachineElements<IInitializedRegionBuilder>,
        IStateHistory<IInitializedRegionBuilder>,
        IStateMachineFinal<IFinalizedRegionBuilder>
    { }

    public interface IFinalizedRegionBuilder
    { }

    public interface IRegionBuilder :
        IStateMachineInitial<IInitializedRegionBuilder>,
        IStateMachineElements<IInitializedRegionBuilder>,
        IStateHistory<IRegionBuilder>
    { }

    public interface IFinalizedOverridenRegionBuilder :
        IStateMachineOverrides<IFinalizedOverridenRegionBuilder>
    { }

    public interface IOverridenRegionBuilder :
        IStateMachineElements<IOverridenRegionBuilder>,
        IStateHistory<IOverridenRegionBuilder>,
        IStateMachineOverrides<IOverridenRegionBuilder>,
        IStateMachineFinal<IFinalizedOverridenRegionBuilder>
    { }
}
