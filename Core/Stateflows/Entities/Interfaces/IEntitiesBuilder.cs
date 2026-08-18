using System.Reflection;
using System.Collections.Generic;
using Stateflows.Entities.Registration.Interfaces;

namespace Stateflows.Entities
{
    public interface IEntitiesBuilder
    {
        IEntitiesBuilder AddFromAssembly(Assembly assembly);
        IEntitiesBuilder AddFromAssemblies(IEnumerable<Assembly> assemblies);
        IEntitiesBuilder AddEntity<TTemplate>(string entityName, EntityBuildAction<TTemplate>? buildAction = null)
            where TTemplate : class;
        IEntitiesBuilder AddEntity<TTemplate>(string entityName, int version, EntityBuildAction<TTemplate>? buildAction = null)
            where TTemplate : class;
        IEntitiesBuilder AddEntity<TTemplate, TEntity>(string? entityName = null, int version = 1, EntityBuildAction<TTemplate>? buildAction = null)
            where TTemplate : class
            where TEntity : class, IEntity<TTemplate>;
        IEntitiesBuilder AddEntity<TTemplate, TEntity>(int version, EntityBuildAction<TTemplate>? buildAction = null)
            where TTemplate : class
            where TEntity : class, IEntity<TTemplate>;
    }
}

