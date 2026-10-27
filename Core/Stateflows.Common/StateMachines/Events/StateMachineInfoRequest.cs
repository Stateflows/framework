using Stateflows.Common;

namespace Stateflows.StateMachines
{
    /// <summary>Requests a <see cref="StateMachineInfo"/> snapshot from a state machine.</summary>
    [NoTracing, NoDeduplication]
    public sealed class StateMachineInfoRequest : IRequest<StateMachineInfo>
    { }
}
