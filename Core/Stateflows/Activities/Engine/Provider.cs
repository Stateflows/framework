using System.Linq;
using System.Collections.Generic;
using Stateflows.Common;
using Stateflows.Common.Utilities;
using Stateflows.Common.Interfaces;
using Stateflows.Activities.Registration;

namespace Stateflows.Activities.Service
{
    internal class Provider(ActivitiesRegister register, IBehaviorFactory behaviorFactory)
        : IBehaviorProvider, IVersionedBehaviorProvider
    {
        public IReadOnlyDictionary<BehaviorClass, int[]> BehaviorVersions
            => register.Activities.Values
                .GroupBy(a => new BehaviorClass(BehaviorType.Activity, a.Name))
                .ToDictionary(g => g.Key, g => g.Select(a => a.Version).Distinct().OrderBy(v => v).ToArray());

        public bool IsLocal => true;

        public event ActionAsync<IBehaviorProvider> BehaviorClassesChanged;

        public bool TryProvideBehavior(BehaviorId id, out IBehavior behavior)
        {
            behavior = id.Type == BehaviorType.Activity && register.Activities.ContainsKey($"{id.Name}.current")
                ? behaviorFactory.CreateBehavior(id)
                : null;

            return behavior != null;
        }

        public IEnumerable<BehaviorClass> BehaviorClasses
            => register.Activities.Values.Select(sm => new BehaviorClass(BehaviorType.Activity, sm.Name));
    }
}
