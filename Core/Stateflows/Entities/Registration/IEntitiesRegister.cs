using System;
using System.Threading.Tasks;
using Stateflows.Entities.Registration.Interfaces;

namespace Stateflows.Entities
{
    public interface IEntitiesRegister
    {
        void AddEntity<TTemplate>(string entityName, EntityBuildAction<TTemplate>? buildAction = null)
            where TTemplate : class
            => AddEntity(entityName, 1, buildAction);

        void AddEntity<TTemplate>(string entityName, int version, EntityBuildAction<TTemplate>? buildAction = null)
            where TTemplate : class;

        void AddEntity<TTemplate>(string entityName, Type entityType, EntityBuildAction<TTemplate>? buildAction = null)
            where TTemplate : class
            => AddEntity(entityName, 1, entityType, buildAction);

        void AddEntity<TTemplate>(string entityName, int version, Type entityType, EntityBuildAction<TTemplate>? buildAction = null)
            where TTemplate : class;

        void AddEntity<TTemplate, TEntity>(string? entityName = null, int version = 1, EntityBuildAction<TTemplate>? buildAction = null)
            where TTemplate : class
            where TEntity : class, IEntity<TTemplate>;

        Task VisitEntitiesAsync(IEntityVisitor visitor);
        Task VisitEntityAsync(string entityName, int version, IEntityVisitor visitor);
    }
}

