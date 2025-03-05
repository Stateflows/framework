using Microsoft.Extensions.DependencyInjection;
using Stateflows.Activities.Models;

namespace Stateflows.Activities.Registration.Interfaces
{
    internal interface IInternal
    {
        Graph Graph { get; }

        IServiceCollection Services { get; }
    }
}
