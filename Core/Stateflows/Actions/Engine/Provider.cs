using System;
using System.Linq;
using System.Collections.Generic;
using Stateflows.Common;
using Stateflows.Common.Classes;
using Stateflows.Common.Utilities;
using Stateflows.Common.Interfaces;
using Stateflows.Actions.Registration;

namespace Stateflows.Actions.Service
{
    internal class Provider : IBehaviorProvider
    {
        private readonly ActionsRegister Register;

        private readonly StateflowsService Service;

        private readonly IServiceProvider ServiceProvider;

        public bool IsLocal => true;

        public Provider(ActionsRegister register, StateflowsService service, IServiceProvider serviceProvider)
        {
            Register = register;
            Service = service;
            ServiceProvider = serviceProvider;
        }

        public event ActionAsync<IBehaviorProvider> BehaviorClassesChanged;

        public bool TryProvideBehavior(BehaviorId id, out IBehavior behavior)
        {
            behavior = id.Type == BehaviorType.Action && Register.Actions.ContainsKey($"{id.Name}.current")
                ? new Behavior(Service, ServiceProvider, id)
                : null;

            return behavior != null;
        }

        public IEnumerable<BehaviorClass> BehaviorClasses
            => Register.Actions.Values.Select(a => new BehaviorClass(BehaviorType.Action, a.Name));
    }
}
