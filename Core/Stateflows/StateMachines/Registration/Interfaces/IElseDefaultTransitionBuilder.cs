using Stateflows.StateMachines.Registration.Interfaces.Base;

namespace Stateflows.StateMachines.Registration.Interfaces
{
    public interface IElseDefaultTransitionBuilder :
        ITransitionUtils<IElseDefaultTransitionBuilder>,
        IDefaultEffect<IElseDefaultTransitionBuilder>
    { }
}
