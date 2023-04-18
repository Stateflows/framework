using Microsoft.Extensions.DependencyInjection;
using Stateflows.Common.Registration.Interfaces;

namespace Stateflows.Common.Registration.Builders
{
    public class StateflowsClientBuilder : IStateflowsClientBuilder
    {
        public IServiceCollection Services { get; private set; }

        public StateflowsClientBuilder(IServiceCollection services)
        {
            Services = services;
        }
    }
}