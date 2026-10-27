namespace Stateflows.Common
{
    /// <summary>Requests the current status and metadata of a behavior instance.</summary>
    [NoTracing, NoDeduplication]
    public sealed class BehaviorInfoRequest : IRequest<BehaviorInfo>
    { }
}
