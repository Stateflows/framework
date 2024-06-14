using System;
using System.Collections.Generic;
using Stateflows.Common;
using Stateflows.Common.Classes;
using Stateflows.Common.Utilities;
using Stateflows.Common.Interfaces;

namespace Stateflows.System.Engine
{
    internal class Provider : IBehaviorProvider
    {
        public StateflowsEngine Engine { get; private set; }

        public IServiceProvider ServiceProvider { get; private set; }

        public bool IsLocal => true;

        public Provider(StateflowsEngine engine, IServiceProvider serviceProvider)
        {
            Engine = engine;
            ServiceProvider = serviceProvider;
        }

        public event ActionAsync<IBehaviorProvider> BehaviorClassesChanged;

        public bool TryProvideBehavior(BehaviorId id, out IBehavior behavior)
        {
            behavior = id == SystemBehavior.Id
                ? new Behavior(Engine, ServiceProvider, id)
                : null;

            return behavior != null;
        }

        public IEnumerable<BehaviorClass> BehaviorClasses
            => new BehaviorClass[] { new BehaviorClass("System", "Stateflows") };
    }
}
