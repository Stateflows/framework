using Microsoft.Extensions.DependencyInjection;
using Stateflows.StateMachines.Models;

namespace Stateflows.StateMachines.Registration.Interfaces.Internal
{
    internal interface ICompositeStateBuilderInternal : ICompositeStateBuilder
    {
        Vertex Vertex { get; }

        IServiceCollection Services { get; }
    }
}
