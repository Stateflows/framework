using Microsoft.Extensions.DependencyInjection;
using Stateflows.StateMachines.Models;

namespace Stateflows.StateMachines.Registration.Interfaces.Internal
{
    internal interface IStateMachineBuilderInternal : IStateMachineBuilder
    {
        Graph Result { get; }

        IServiceCollection Services { get; }
    }
}