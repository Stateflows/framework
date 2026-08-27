using System.Linq;
using System.Collections.Generic;
using Stateflows.Common;
using Stateflows.Common.Utilities;
using Stateflows.Common.Interfaces;
using Stateflows.Entities.Registration;

namespace Stateflows.Entities.Engine
{
    internal class Provider(EntitiesRegister register, IBehaviorFactory behaviorFactory) : IBehaviorProvider
    {
        public bool IsLocal => true;

        public event ActionAsync<IBehaviorProvider> BehaviorClassesChanged;

        public bool TryProvideBehavior(BehaviorId id, out IBehavior behavior)
        {
            behavior = id.Type == BehaviorType.Entity && register.Entities.ContainsKey($"{id.Name}.current")
                ? behaviorFactory.CreateBehavior(id)
                : null;

            return behavior != null;
        }

        public IEnumerable<BehaviorClass> BehaviorClasses
            => register.Entities.Values.Select(entity => new BehaviorClass(BehaviorType.Entity, entity.Name)).Distinct();
    }
}
