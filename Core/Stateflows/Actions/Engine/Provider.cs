using System;
using System.Collections.Generic;
using System.Linq;
using Stateflows.Actions.Registration;
using Stateflows.Common;
using Stateflows.Common.Classes;
using Stateflows.Common.Interfaces;
using Stateflows.Common.Utilities;

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
            behavior = Register.SupportedClassTypes.Contains(id.Type) && Register.Actions.ContainsKey($"{id.Name}.current")
                ? new Behavior(Service, ServiceProvider, id)
                : null;

            return behavior != null;
        }

        public IEnumerable<BehaviorClass> BehaviorClasses
            => Register.Actions.Values.Select(a => new BehaviorClass(a.BehaviorClassType, a.Name));
    }
}
