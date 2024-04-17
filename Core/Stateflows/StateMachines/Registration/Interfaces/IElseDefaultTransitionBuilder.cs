using Stateflows.StateMachines.Events;
using Stateflows.StateMachines.Registration.Interfaces.Base;

namespace Stateflows.StateMachines.Registration.Interfaces
{
    public interface IElseDefaultTransitionBuilder :
        IEffect<CompletionEvent, IElseDefaultTransitionBuilder>
    { }
}
