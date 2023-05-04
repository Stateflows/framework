using Microsoft.Extensions.DependencyInjection;
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

