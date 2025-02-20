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
        private readonly StateMachinesRegister Register;

        private readonly StateflowsService Service;

        private readonly IServiceProvider ServiceProvider;

        public bool IsLocal => true;

        public Provider(StateMachinesRegister register, StateflowsService service, IServiceProvider serviceProvider)
        {
            Register = register;
            Service = service;
            ServiceProvider = serviceProvider;
        }

        public event ActionAsync<IBehaviorProvider> BehaviorClassesChanged;

        public bool TryProvideBehavior(BehaviorId id, out IBehavior behavior)
        {
            behavior = id.Type == Constants.StateMachine && Register.StateMachines.ContainsKey($"{id.Name}.current")
                ? new Behavior(Service, ServiceProvider, id)
                : null;

            return behavior != null;
        }

        public IEnumerable<BehaviorClass> BehaviorClasses
            => Register.StateMachines.Values.Select(sm => new BehaviorClass(Constants.StateMachine, sm.Name)).Distinct();
    }
}
