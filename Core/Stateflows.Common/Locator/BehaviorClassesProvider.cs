using System.Linq;
using System.Collections.Generic;
using Stateflows.Common.Interfaces;

namespace Stateflows.Common.Locator
{
    internal class BehaviorClassesProvider : IBehaviorClassesProvider
    {
        public BehaviorClassesProvider(IEnumerable<IBehaviorProvider> behaviorProviders)
        {
            LocalBehaviorClasses = behaviorProviders.Where(bp => bp.IsLocal).SelectMany(bp => bp.BehaviorClasses);
            RemoteBehaviorClasses = behaviorProviders.Where(bp => !bp.IsLocal).SelectMany(bp => bp.BehaviorClasses);
            AllBehaviorClasses = behaviorProviders.SelectMany(bp => bp.BehaviorClasses);
        }

        public IEnumerable<BehaviorClass> LocalBehaviorClasses { get; set; }

        public IEnumerable<BehaviorClass> RemoteBehaviorClasses { get; set; }

        public IEnumerable<BehaviorClass> AllBehaviorClasses { get; set; }
    }
}
