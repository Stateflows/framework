using System.Threading.Tasks;
using Stateflows.Entities.Attributes;
using Stateflows.Entities.Enums;

namespace Stateflows.Entities
{
    /// <summary>Provides no-op entity registration visitor methods for selective specialization.</summary>
    public abstract class EntityVisitor : IEntityVisitor
    {
        /// <inheritdoc />
        public virtual Task EntityAddingAsync<TTemplate>(string entityName, int entityVersion, BehaviorClass? ownerClass = null, BehaviorClass? parentClass = null, bool hasDefaultInstance = false)
            where TTemplate : class
            => Task.CompletedTask;

        /// <inheritdoc />
        public virtual Task EntityAddedAsync<TTemplate>(string entityName, int entityVersion)
            where TTemplate : class
            => Task.CompletedTask;

        /// <inheritdoc />
        public virtual Task EntityTypeAddedAsync<TTemplate, TEntity>(string entityName, int entityVersion)
            where TTemplate : class
            where TEntity : class, IEntity<TTemplate>
            => Task.CompletedTask;

        /// <inheritdoc />
        public virtual Task FieldAddedAsync<TTemplate, TField>(string entityName, int entityVersion, string fieldName, TField defaultValue, bool computed, FieldAccess access)
            where TTemplate : class
            => Task.CompletedTask;

        /// <inheritdoc />
        public virtual Task InitializerAddedAsync<TTemplate, TInitializationEvent>(string entityName, int entityVersion)
            where TTemplate : class
            => Task.CompletedTask;

        /// <inheritdoc />
        public virtual Task MutationAddedAsync<TTemplate, TMutationEvent>(string entityName, int entityVersion)
            where TTemplate : class
            => Task.CompletedTask;

        /// <inheritdoc />
        public virtual Task ProjectionAddedAsync<TTemplate, TProjectionTemplate>(string entityName, int entityVersion, PublishScope publishScope)
            where TTemplate : class
            => Task.CompletedTask;
    }
}
