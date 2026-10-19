using Stateflows.Common;

namespace Stateflows.StateMachines
{
    [NoTracing, NoDeduplication]
    public sealed class StateMachineInfoRequest : IRequest<StateMachineInfo>
    { }
}
