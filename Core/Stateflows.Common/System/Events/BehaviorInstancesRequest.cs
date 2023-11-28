using Stateflows.Common;
using System.Collections.Generic;

namespace Stateflows.System.Events
{
    public sealed class BehaviorInstancesRequest : Request<BehaviorInstancesResponse>
    {
        public IEnumerable<BehaviorClass> BehaviorClasses { get; set; }
    }
}