using System.Linq;
using System.Collections.Generic;
using Stateflows.Common;
using Stateflows.Common.Utilities;
using Stateflows.Common.Interfaces;
using Stateflows.StateMachines.Registration;

namespace Stateflows.StateMachines.Engine
{
    internal class Provider(StateMachinesRegister register, IBehaviorFactory behaviorFactory) : IBehaviorProvider, IVersionedBehaviorProvider
    {
        public IReadOnlyDictionary<BehaviorClass, int[]> BehaviorVersions
            => register.StateMachines.Values
                .GroupBy(sm => new BehaviorClass(Constants.StateMachine, sm.Name))
                .ToDictionary(g => g.Key, g => g.Select(sm => sm.Version).Distinct().OrderBy(v => v).ToArray());

        public bool IsLocal => true;

        public event ActionAsync<IBehaviorProvider> BehaviorClassesChanged;

        public bool TryProvideBehavior(BehaviorId id, out IBehavior behavior)
        {
            behavior = id.Type == Constants.StateMachine && register.StateMachines.ContainsKey($"{id.Name}.current")
                ? behaviorFactory.CreateBehavior(id)
                : null;

            return behavior != null;
        }

        public IEnumerable<BehaviorClass> BehaviorClasses
            => register.StateMachines.Values.Select(sm => new BehaviorClass(Constants.StateMachine, sm.Name)).Distinct();
    }
}
