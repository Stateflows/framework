using Microsoft.Extensions.DependencyInjection;

namespace Stateflows.StateMachines.Registration.Interfaces.Internal
{
    internal interface IInternal
    {
        IServiceCollection Services { get; }
    }
}