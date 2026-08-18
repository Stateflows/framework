using System.Threading.Tasks;
using Stateflows.Entities.Enums;

namespace Stateflows.Entities
{
    public abstract class EntityVisitor : IEntityVisitor
    {
        public virtual Task EntityAddingAsync<TTemplate>(string entityName, int entityVersion, BehaviorClass? ownerClass = null, BehaviorClass? parentClass = null, bool hasDefaultInstance = false)
            where TTemplate : class
            => Task.CompletedTask;

        public virtual Task EntityAddedAsync<TTemplate>(string entityName, int entityVersion)
            where TTemplate : class
            => Task.CompletedTask;

        public virtual Task EntityTypeAddedAsync<TTemplate, TEntity>(string entityName, int entityVersion)
            where TTemplate : class
            where TEntity : class, IEntity<TTemplate>
            => Task.CompletedTask;

        public virtual Task FieldAddedAsync<TEntityTemplate, TField>(string entityName, int entityVersion, string fieldName, TField defaultValue, bool computed)
            where TEntityTemplate : class
            => Task.CompletedTask;

        public virtual Task MutationAddedAsync<TEntityTemplate, TMutationEvent>(string entityName, int entityVersion)
            where TEntityTemplate : class
            => Task.CompletedTask;

        public virtual Task ProjectionAddedAsync<TEntityTemplate, TProjectionTemplate>(string entityName, int entityVersion, PublishScope publishScope)
            where TEntityTemplate : class
            => Task.CompletedTask;
    }
}

