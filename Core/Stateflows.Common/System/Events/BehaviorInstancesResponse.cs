using Stateflows.Common;
using System.Collections.Generic;

namespace Stateflows.System.Events
{
    public class BehaviorDescriptor
    {
        public BehaviorId Id { get; set; }

        public BehaviorStatus Status { get; set; }
    }

    public sealed class BehaviorInstancesResponse : Response
    {
        public IEnumerable<BehaviorDescriptor> Behaviors { get; set; }
    }
}
