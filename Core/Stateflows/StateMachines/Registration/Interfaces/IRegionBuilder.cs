using Stateflows.StateMachines.Registration.Interfaces.Base;

namespace Stateflows.StateMachines.Registration.Interfaces
{
    public interface IInitializedRegionBuilder :
        IStateMachine<IInitializedRegionBuilder>,
        IStateMachineFinal<IFinalizedRegionBuilder>
    { }

    public interface IFinalizedRegionBuilder
    { }

    public interface IRegionBuilder :
        IStateMachineInitial<IInitializedRegionBuilder>,
        IStateMachine<IInitializedRegionBuilder>
    { }

    public interface IFinalizedOverridenRegionBuilder
    { }

    public interface IOverridenRegionBuilder :
        IStateMachine<IOverridenRegionBuilder>,
        IStateMachineOverrides<IOverridenRegionBuilder>,
        IStateMachineFinal<IFinalizedOverridenRegionBuilder>
    { }
}
