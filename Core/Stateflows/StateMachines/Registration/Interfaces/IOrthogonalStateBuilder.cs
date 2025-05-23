using Stateflows.StateMachines.Registration.Interfaces.Base;

namespace Stateflows.StateMachines.Registration.Interfaces
{
    public interface IOrthogonalStateBuilder :
        IOrthogonalStateExtension<IOrthogonalStateBuilder>,
        IStateEvents<IOrthogonalStateBuilder>,
        IOrthogonalStateTypedEvents<IOrthogonalStateBuilder>,
        IStateUtils<IOrthogonalStateBuilder>,
        IRegions<IOrthogonalStateBuilder>,
        IStateTransitions<IOrthogonalStateBuilder>
    { }

    public interface IOverridenOrthogonalStateBuilder :
        IOrthogonalStateExtension<IOverridenOrthogonalStateBuilder>,
        IStateEvents<IOverridenOrthogonalStateBuilder>,
        IOrthogonalStateTypedEvents<IOverridenOrthogonalStateBuilder>,
        IStateUtils<IOverridenOrthogonalStateBuilder>,
        IRegions<IOverridenOrthogonalStateBuilder>,
        IRegionsOverrides<IOverridenOrthogonalStateBuilder>,
        IStateTransitions<IOverridenOrthogonalStateBuilder>,
        IStateTransitionsOverrides<IOverridenOrthogonalStateBuilder>
    { }
}
