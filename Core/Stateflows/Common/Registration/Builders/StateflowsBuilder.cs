using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Stateflows.Common.Classes;
using Stateflows.Common.Interfaces;
using Stateflows.Common.Registration.Interfaces;

namespace Stateflows.Common.Registration.Builders
{
    internal class StateflowsBuilder : IStateflowsBuilder
    {
        private readonly List<IStateflowsTypeMapper> TypeMappers = [];

        internal static int MaxConcurrentBehaviorExecutions = 0;

        internal readonly ITypeMapper TypeMapper;

        public IServiceCollection ServiceCollection { get; }

        public StateflowsBuilder(IServiceCollection services)
        {
            ServiceCollection = services;
            TypeMapper = new TypeMapper(TypeMappers);
        }

        IStateflowsBuilder IStateflowsBuilder.AddTypeMapper<TTypeMapper>()
        {
            TypeMappers.Add(new TTypeMapper());

            return this;
        }

        public IStateflowsBuilder SetMaxConcurrentBehaviorExecutions(int maxConcurrentBehaviorExecutions)
        {
            MaxConcurrentBehaviorExecutions = maxConcurrentBehaviorExecutions;
            
            return this;
        }
    }
}