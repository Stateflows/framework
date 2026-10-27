using Microsoft.Extensions.DependencyInjection;

namespace Stateflows.Common.Registration.Interfaces
{
    /// <summary>Provides access to the service collection used for client-side registrations.</summary>
    public interface IStateflowsClientBuilder
    {
        /// <summary>Gets the dependency-injection service collection being configured.</summary>
        IServiceCollection ServiceCollection { get; }
    }
}