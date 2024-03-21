using Stateflows.StateMachines.Events;
using Stateflows.StateMachines.Registration.Interfaces.Base;

namespace Stateflows.StateMachines.Registration.Interfaces
{
    public interface IDefaultTransitionBuilder :
        IEffect<CompletionEvent, IDefaultTransitionBuilder>,
        IGuard<CompletionEvent, IDefaultTransitionBuilder>
    { }
}
