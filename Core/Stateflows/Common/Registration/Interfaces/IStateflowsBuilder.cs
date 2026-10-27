using System;
using Stateflows.Common.Interfaces;

namespace Stateflows.Common.Registration.Interfaces
{
    /// <summary>Configures Stateflows host behavior, type mapping, and resource groups.</summary>
    public interface IStateflowsBuilder : IStateflowsClientBuilder
    {
        /// <summary>Registers a custom type mapper for event and value conversion.</summary>
        /// <typeparam name="TTypeMapper">A constructible type mapper implementation.</typeparam>
        /// <returns>This builder for fluent configuration.</returns>
        IStateflowsBuilder AddTypeMapper<TTypeMapper>()
            where TTypeMapper : class, IStateflowsTypeMapper, new();

        /// <summary>Registers and configures a resource group.</summary>
        /// <param name="resourceName">The resource group name.</param>
        /// <param name="builderAction">Configures resource execution limits.</param>
        /// <returns>This builder for fluent configuration.</returns>
        IStateflowsBuilder AddResource(string resourceName, Action<IResourceBuilder> builderAction);
    }
}