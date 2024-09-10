using Stateflows.Common;

namespace Stateflows.StateMachines.Events
{
    [NoTracing]
    public sealed class CurrentStateRequest : IRequest<CurrentState>
    { }
}
