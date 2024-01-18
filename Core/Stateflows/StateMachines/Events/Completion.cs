using Stateflows.Common;
using Stateflows.StateMachines.Registration;

namespace Stateflows.StateMachines.Events
{
    public sealed class Completion : Event
    {
        public override string Name => Constants.CompletionEvent;
    }
}
