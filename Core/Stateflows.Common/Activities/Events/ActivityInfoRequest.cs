using Stateflows.Common;

namespace Stateflows.Activities
{
    [NoTracing, NoDeduplication]
    public sealed class ActivityInfoRequest : IRequest<ActivityInfo>
    { }
}
