using Stateflows.StateMachines.Registration.Interfaces.Base;

namespace Stateflows.StateMachines.Registration.Interfaces
{
    public interface IJoinBuilder :
        IPseudostateTransitionsEffects<IJoinBuilder>
    { }
    
    public interface IOverridenJoinBuilder :
        IPseudostateTransitionsEffects<IOverridenJoinBuilder>,
        IPseudostateTransitionsEffectsOverrides<IOverridenJoinBuilder>
    { }
}
