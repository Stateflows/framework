using Stateflows.StateMachines.Registration;
using Stateflows.Common;

namespace Stateflows.StateMachines.Events
{
    public sealed class Completion : Event
    {
        public override string Name => Constants.CompletionEvent;
    }
}
