using System.Threading.Tasks;
using Stateflows.Entities.Enums;

namespace Stateflows.Entities
{
    public interface IEntityVisitor
    {
        Task EntityAddingAsync<TEntityTemplate>(string entityName, int entityVersion, BehaviorClass? ownerClass = null, BehaviorClass? parentClass = null, bool hasDefaultInstance = false)
            where TEntityTemplate : class;

        Task EntityAddedAsync<TEntityTemplate>(string entityName, int entityVersion)
            where TEntityTemplate : class;

        Task EntityTypeAddedAsync<TEntityTemplate, TEntity>(string entityName, int entityVersion)
            where TEntityTemplate : class
            where TEntity : class, IEntity<TEntityTemplate>;
        
        Task FieldAddedAsync<TEntityTemplate, TField>(string entityName, int entityVersion, string fieldName, TField defaultValue, bool computed)
            where TEntityTemplate : class;
        
        Task MutationAddedAsync<TEntityTemplate, TMutationEvent>(string entityName, int entityVersion)
            where TEntityTemplate : class;
        
        Task ProjectionAddedAsync<TEntityTemplate, TProjectionTemplate>(string entityName, int entityVersion, PublishScope publishScope)
            where TEntityTemplate : class;
    }
}

