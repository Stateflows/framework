using Stateflows.Common;

namespace Stateflows.StateMachines.Events
{
    [DoNotTrace]
    public sealed class CurrentStateRequest : IRequest<CurrentState>
    { }
}
