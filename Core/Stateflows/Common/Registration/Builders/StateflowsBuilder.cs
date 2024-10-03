using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Stateflows.Common.Interfaces;
using Stateflows.Common.Registration.Interfaces;

namespace Stateflows.Common.Registration.Builders
{
    internal class StateflowsBuilder : IStateflowsBuilder
    {
        private readonly List<IStateflowsTypeMapper> typeMappers = new List<IStateflowsTypeMapper>();

        public IServiceCollection ServiceCollection { get; private set; }

        public StateflowsBuilder(IServiceCollection services)
        {
            ServiceCollection = services;
        }

        IStateflowsBuilder IStateflowsBuilder.AddTypeMapper<TTypeMapper>()
        {
            typeMappers.Add(new TTypeMapper());

            return this;
        }

        internal IEnumerable<Type> GetMappedTypes(Type type)
        {
            foreach (var typeMapper in typeMappers)
            {
                if (typeMapper.TryMapType(type, out var triggerTypes))
                {
                    return triggerTypes;
                }
            }

            return new Type[] { type };
        }
    }
}