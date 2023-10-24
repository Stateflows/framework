using Microsoft.Extensions.DependencyInjection;
using Stateflows.Common.Registration.Interfaces;

namespace Stateflows.Common.Registration.Builders
{
    internal class StateflowsClientBuilder : IStateflowsClientBuilder
    {
        public IServiceCollection ServiceCollection { get; private set; }

        public StateflowsClientBuilder(IServiceCollection services)
        {
            ServiceCollection = services;
        }
    }
}