using Microsoft.Extensions.DependencyInjection;
using Stateflows.StateMachines.Models;

namespace Stateflows.StateMachines.Registration.Interfaces.Internal
{
    internal interface IInternal
    {
        //Graph Result { get; }

        IServiceCollection Services { get; }
    }
}