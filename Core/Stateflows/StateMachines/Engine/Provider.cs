using System;
using System.Linq;
using System.Collections.Generic;
using Stateflows.Common;
using Stateflows.Common.Classes;
using Stateflows.Common.Utilities;
using Stateflows.Common.Interfaces;
using Stateflows.StateMachines.Registration;

namespace Stateflows.StateMachines.Engine
{
    internal class Provider : IBehaviorProvider
    {
        public StateMachinesRegister Register { get; private set; }

        public StateflowsEngine Engine { get; private set; }

        public IServiceProvider ServiceProvider { get; private set; }

        public bool IsLocal => true;

        public Provider(StateMachinesRegister register, StateflowsEngine engine, IServiceProvider serviceProvider)
        {
            Register = register;
            Engine = engine;
            ServiceProvider = serviceProvider;
        }

        public event ActionAsync<IBehaviorProvider> BehaviorClassesChanged;

        public bool TryProvideBehavior(BehaviorId id, out IBehavior behavior)
        {
            behavior = id.Type == nameof(StateMachine) && Register.StateMachines.ContainsKey(id.Name)
                ? new Behavior(Engine, ServiceProvider, id)
                : null;

            return behavior != null;
        }

        public IEnumerable<BehaviorClass> BehaviorClasses
            => Register.StateMachines.Values.Select(sm => new BehaviorClass(nameof(StateMachine), sm.Name));
    }
}
