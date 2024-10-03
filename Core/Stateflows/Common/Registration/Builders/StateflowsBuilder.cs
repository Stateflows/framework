using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Stateflows.Common.Interfaces;
using Stateflows.Common.Registration.Interfaces;

namespace Stateflows.Common.Registration.Builders
{
    internal class StateflowsBuilder : IStateflowsBuilder
    {
        public readonly List<IStateflowsTypeMapper> TypeMappers = new List<IStateflowsTypeMapper>();

        public IServiceCollection ServiceCollection { get; private set; }

        public StateflowsBuilder(IServiceCollection services)
        {
            ServiceCollection = services;
        }

        IStateflowsBuilder IStateflowsBuilder.AddTypeMapper<TTypeMapper>()
        {
            TypeMappers.Add(new TTypeMapper());

            return this;
        }
    }
}