using Microsoft.Extensions.DependencyInjection;
using Stateflows.Common.Registration.Interfaces;

namespace Stateflows.Common.Registration.Builders
{
    internal class StateflowsBuilder : IStateflowsBuilder
    {
        public IServiceCollection ServiceCollection { get; private set; }

        public StateflowsBuilder(IServiceCollection services)
        {
            ServiceCollection = services;
        }
    }
}