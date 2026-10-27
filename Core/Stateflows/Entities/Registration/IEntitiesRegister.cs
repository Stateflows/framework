using System;
using System.Threading.Tasks;
using Stateflows.Entities.Registration.Interfaces;

namespace Stateflows.Entities
{
    /// <summary>Registers entity definitions and exposes them to inspection visitors.</summary>
    public interface IEntitiesRegister
    {
        /// <summary>Registers an entity template at version 1.</summary>
        /// <typeparam name="TTemplate">The entity template type.</typeparam>
        /// <param name="entityName">The registered entity name.</param>
        /// <param name="buildAction">Optional callback that configures the entity.</param>
        void AddEntity<TTemplate>(string entityName, EntityBuildAction<TTemplate>? buildAction = null)
            where TTemplate : class
            => AddEntity(entityName, 1, buildAction);

        /// <summary>Registers an entity template at a specified version.</summary>
        /// <typeparam name="TTemplate">The entity template type.</typeparam>
        /// <param name="entityName">The registered entity name.</param>
        /// <param name="version">The entity definition version.</param>
        /// <param name="buildAction">Optional callback that configures the entity.</param>
        void AddEntity<TTemplate>(string entityName, int version, EntityBuildAction<TTemplate>? buildAction = null)
            where TTemplate : class;

        /// <summary>Registers an entity template and implementation at version 1.</summary>
        /// <typeparam name="TTemplate">The entity template type.</typeparam>
        /// <param name="entityName">The registered entity name.</param>
        /// <param name="entityType">The concrete implementation type.</param>
        /// <param name="buildAction">Optional callback that configures the entity.</param>
        void AddEntity<TTemplate>(string entityName, Type entityType, EntityBuildAction<TTemplate>? buildAction = null)
            where TTemplate : class
            => AddEntity(entityName, 1, entityType, buildAction);

        /// <summary>Registers an entity template and implementation at a specified version.</summary>
        /// <typeparam name="TTemplate">The entity template type.</typeparam>
        /// <param name="entityName">The registered entity name.</param>
        /// <param name="version">The entity definition version.</param>
        /// <param name="entityType">The concrete implementation type.</param>
        /// <param name="buildAction">Optional callback that configures the entity.</param>
        void AddEntity<TTemplate>(string entityName, int version, Type entityType, EntityBuildAction<TTemplate>? buildAction = null)
            where TTemplate : class;

        /// <summary>Registers an entity template and implementation with optional inferred name and version.</summary>
        /// <typeparam name="TTemplate">The entity template type.</typeparam>
        /// <typeparam name="TEntity">The concrete entity implementation type.</typeparam>
        /// <param name="entityName">Optional registered entity name.</param>
        /// <param name="version">Entity definition version.</param>
        /// <param name="buildAction">Optional callback that configures the entity.</param>
        void AddEntity<TTemplate, TEntity>(string? entityName = null, int version = 1, EntityBuildAction<TTemplate>? buildAction = null)
            where TTemplate : class
            where TEntity : class, IEntity<TTemplate>;

        /// <summary>Visits all registered entity definitions and their elements.</summary>
        /// <param name="visitor">The visitor that receives registration callbacks.</param>
        Task VisitEntitiesAsync(IEntityVisitor visitor);
        /// <summary>Visits one registered entity definition and its elements.</summary>
        /// <param name="entityName">The registered entity name.</param>
        /// <param name="version">The entity definition version.</param>
        /// <param name="visitor">The visitor that receives registration callbacks.</param>
        Task VisitEntityAsync(string entityName, int version, IEntityVisitor visitor);
    }
}
