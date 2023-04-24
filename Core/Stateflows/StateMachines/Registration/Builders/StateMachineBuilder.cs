using System;
using Microsoft.Extensions.DependencyInjection;
using Stateflows.StateMachines.Models;
using Stateflows.StateMachines.Extensions;
using Stateflows.StateMachines.Interfaces;
using Stateflows.StateMachines.Context.Classes;
using Stateflows.StateMachines.Registration.Interfaces;
using Stateflows.StateMachines.Registration.Interfaces.Base;
using Stateflows.StateMachines.Registration.Interfaces.Internal;

namespace Stateflows.StateMachines.Registration.Builders
{
    internal class StateMachineBuilder : StateMachineBuilderBase, IStateMachineBuilder, IStateMachineInitialBuilder, IStateMachineBuilderInternal
    {
        public StateMachineBuilder(string name, IServiceCollection services)
            : base(name, services)
        { }
    }
}

