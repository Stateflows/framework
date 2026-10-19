using System.Collections.Generic;
using System.Linq;
using Stateflows.Actions.Registration;
using Stateflows.Common;
using Stateflows.Common.Interfaces;
using Stateflows.Common.Utilities;

namespace Stateflows.Actions.Service
{
    internal class Provider(ActionsRegister register, IBehaviorFactory behaviorFactory) : IBehaviorProvider, IVersionedBehaviorProvider
    {
        public IReadOnlyDictionary<BehaviorClass, int[]> BehaviorVersions
            => register.Actions.Values
                .GroupBy(a => new BehaviorClass(a.BehaviorClassType, a.Name))
                .ToDictionary(g => g.Key, g => g.Select(a => a.Version).Distinct().OrderBy(v => v).ToArray());

        public bool IsLocal => true;

        public event ActionAsync<IBehaviorProvider> BehaviorClassesChanged;

        public bool TryProvideBehavior(BehaviorId id, out IBehavior behavior)
        {
            behavior = register.SupportedClassTypes.Contains(id.Type) && register.Actions.ContainsKey($"{id.Name}.current")
                ? behaviorFactory.CreateBehavior(id)
                : null;

            return behavior != null;
        }

        public IEnumerable<BehaviorClass> BehaviorClasses
            => register.Actions.Values.Select(a => new BehaviorClass(a.BehaviorClassType, a.Name));
    }
}
