using Stateflows.StateMachines.Registration.Interfaces.Base;

namespace Stateflows.StateMachines.Registration.Interfaces
{
    public interface IForkBuilder :
        IForkTransitions<IForkBuilder>
    { }
    
    public interface IOverridenForkBuilder :
        IForkTransitions<IOverridenForkBuilder>
    { }
}
