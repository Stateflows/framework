using Stateflows.Common;
using System.Collections.Generic;

namespace Stateflows.System
{
    public sealed class AvailableBehaviorClassesResponse : Response
    {
        public IEnumerable<BehaviorClass> AvailableBehaviorClasses { get; set; }
    }
}
