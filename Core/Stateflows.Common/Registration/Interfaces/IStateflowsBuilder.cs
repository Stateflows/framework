using Microsoft.Extensions.DependencyInjection;

namespace Stateflows.Common.Registration.Interfaces
{
    public interface IStateflowsBuilder
    {
        IServiceCollection Services { get; }
    }
}