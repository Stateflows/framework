using Stateflows.Common;

namespace Stateflows.Activities
{
    /// <summary>Requests an <see cref="ActivityInfo"/> snapshot from an activity.</summary>
    [NoTracing, NoDeduplication]
    public sealed class ActivityInfoRequest : IRequest<ActivityInfo>
    { }
}
