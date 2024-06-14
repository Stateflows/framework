using System.Threading;
using Stateflows.Activities.Context.Interfaces;

namespace Stateflows.Activities.Context.Classes
{
    public static class ActivityFlowContextAccessor
    {
        public static readonly AsyncLocal<IActivityFlowContext> Context = new AsyncLocal<IActivityFlowContext>();
    }
}
