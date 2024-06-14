using System.Threading;
using Stateflows.Activities.Context.Interfaces;

namespace Stateflows.Activities.Context.Classes
{
    public static class ActivityNodeContextAccessor
    {
        public static readonly AsyncLocal<IActivityNodeContext> Context = new AsyncLocal<IActivityNodeContext>();
    }
}
