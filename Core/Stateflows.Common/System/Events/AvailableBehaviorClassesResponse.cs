using Stateflows.Common;
using System.Collections.Generic;

namespace Stateflows.System.Events
{
    public sealed class AvailableBehaviorClassesResponse : Response
    {
        public IEnumerable<BehaviorClass> AvailableBehaviorClasses { get; set; }
    }
}
