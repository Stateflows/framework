using Stateflows.Common;

namespace Stateflows.Activities.Events
{
    public sealed class CancelResponse : Response
    {
        public bool Cancelled { get; set; }
    }
}
