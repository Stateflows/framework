using System.Threading.Tasks;
using Stateflows.Entities.Attributes;
using Stateflows.Entities.Enums;

namespace Stateflows.Entities
{
    public interface IEntityVisitor
    {
        Task EntityAddingAsync<TTemplate>(string entityName, int entityVersion, BehaviorClass? ownerClass = null, BehaviorClass? parentClass = null, bool hasDefaultInstance = false)
            where TTemplate : class;

        Task EntityAddedAsync<TTemplate>(string entityName, int entityVersion)
            where TTemplate : class;

        Task EntityTypeAddedAsync<TTemplate, TEntity>(string entityName, int entityVersion)
            where TTemplate : class
            where TEntity : class, IEntity<TTemplate>;
        
        Task FieldAddedAsync<TTemplate, TField>(string entityName, int entityVersion, string fieldName, TField defaultValue, bool computed, FieldAccess access)
            where TTemplate : class;
        
        Task InitializerAddedAsync<TTemplate, TInitializationEvent>(string entityName, int entityVersion)
            where TTemplate : class;
        
        Task MutationAddedAsync<TTemplate, TMutationEvent>(string entityName, int entityVersion)
            where TTemplate : class;
        
        Task ProjectionAddedAsync<TTemplate, TProjectionTemplate>(string entityName, int entityVersion, PublishScope publishScope)
            where TTemplate : class;
    }
}

