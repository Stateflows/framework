using System;
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

        internal readonly ITypeMapper TypeMapper;
        
        internal readonly Dictionary<string, Resource> ResourceNames = [];
        
        internal readonly Dictionary<BehaviorClass, Resource> ResourcesByBehaviorClass = [];

        public IServiceCollection ServiceCollection { get; }

        public StateflowsBuilder(IServiceCollection services)
        {
            ServiceCollection = services;
            TypeMapper = new TypeMapper(TypeMappers);
            AddResource("", b => { });
        }

        IStateflowsBuilder IStateflowsBuilder.AddTypeMapper<TTypeMapper>()
        {
            TypeMappers.Add(new TTypeMapper());

            return this;
        }

        public IStateflowsBuilder AddResource(string resourceName, Action<IResourceBuilder> builderAction)
        {
            if (ResourceNames.ContainsKey(resourceName)) throw new ArgumentException($"Resource {resourceName} is already registered");
            
            var builder = new ResourceBuilder(resourceName);
            builderAction(builder);
            ResourceNames.Add(resourceName, builder.Build());

            return this;
        }
    }
}