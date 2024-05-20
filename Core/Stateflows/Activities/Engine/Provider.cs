using System;
using System.Linq;
using System.Collections.Generic;
using Stateflows.Common;
using Stateflows.Common.Classes;
using Stateflows.Common.Utilities;
using Stateflows.Common.Interfaces;
using Stateflows.Activities.Registration;

namespace Stateflows.Activities.Engine
{
    internal class Provider : IBehaviorProvider
    {
        public ActivitiesRegister Register { get; }

        public StateflowsEngine Engine { get; }

        public IServiceProvider ServiceProvider { get; }

        public bool IsLocal => true;

        public Provider(ActivitiesRegister register, StateflowsEngine engine, IServiceProvider serviceProvider)
        {
            Register = register;
            Engine = engine;
            ServiceProvider = serviceProvider;
        }

        public event ActionAsync<IBehaviorProvider> BehaviorClassesChanged;

        public bool TryProvideBehavior(BehaviorId id, out IBehavior behavior)
        {
            behavior = id.Type == BehaviorType.Activity && Register.Activities.ContainsKey($"{id.Name}.current")
                ? new Behavior(Engine, ServiceProvider, id)
                : null;

            return behavior != null;
        }

        public IEnumerable<BehaviorClass> BehaviorClasses
            => Register.Activities.Values.Select(sm => new BehaviorClass(BehaviorType.Activity, sm.Name));
    }
}
