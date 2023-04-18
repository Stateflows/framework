using Microsoft.Extensions.DependencyInjection;
using Stateflows.Common.Registration.Interfaces;

namespace Stateflows.Common.Registration.Builders
{
    public class StateflowsBuilder : IStateflowsBuilder, IStateflowsClientBuilder
    {
        public IServiceCollection Services { get; private set; }

        public StateflowsBuilder(IServiceCollection services)
        {
            Services = services;
        }
    }
}