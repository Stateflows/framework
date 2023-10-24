using Microsoft.Extensions.DependencyInjection;

namespace Stateflows.Common.Registration.Interfaces
{
    public interface IStateflowsClientBuilder
    {
        IServiceCollection ServiceCollection { get; }
    }
}