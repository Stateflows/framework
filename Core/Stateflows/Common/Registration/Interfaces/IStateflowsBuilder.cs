using System;
using Stateflows.Common.Interfaces;

namespace Stateflows.Common.Registration.Interfaces
{
    public interface IStateflowsBuilder : IStateflowsClientBuilder
    {
        IStateflowsBuilder AddTypeMapper<TTypeMapper>()
            where TTypeMapper : class, IStateflowsTypeMapper, new();

        IStateflowsBuilder AddResource(string resourceName, Action<IResourceBuilder> builderAction);
    }
}