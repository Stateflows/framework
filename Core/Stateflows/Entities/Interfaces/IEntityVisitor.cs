using System.Threading.Tasks;
using Stateflows.Entities.Attributes;
using Stateflows.Entities.Enums;

namespace Stateflows.Entities
{
    /// <summary>Receives callbacks as entity definitions and their elements are registered.</summary>
    public interface IEntityVisitor
    {
        /// <summary>Reports that an entity registration is beginning.</summary>
        /// <typeparam name="TTemplate">The entity template type.</typeparam>
        /// <param name="entityName">The registered entity name.</param>
        /// <param name="entityVersion">The entity definition version.</param>
        /// <param name="ownerClass">The owning behavior class, when the entity is nested.</param>
        /// <param name="parentClass">The parent behavior class, when the entity is nested.</param>
        /// <param name="hasDefaultInstance">Whether a default instance is configured.</param>
        Task EntityAddingAsync<TTemplate>(string entityName, int entityVersion, BehaviorClass? ownerClass = null, BehaviorClass? parentClass = null, bool hasDefaultInstance = false)
            where TTemplate : class;

        /// <summary>Reports that an entity registration is complete.</summary>
        /// <typeparam name="TTemplate">The entity template type.</typeparam>
        /// <param name="entityName">The registered entity name.</param>
        /// <param name="entityVersion">The entity definition version.</param>
        Task EntityAddedAsync<TTemplate>(string entityName, int entityVersion)
            where TTemplate : class;

        /// <summary>Reports that a concrete entity implementation has been registered.</summary>
        /// <typeparam name="TTemplate">The entity template type.</typeparam>
        /// <typeparam name="TEntity">The concrete entity implementation type.</typeparam>
        /// <param name="entityName">The registered entity name.</param>
        /// <param name="entityVersion">The entity definition version.</param>
        Task EntityTypeAddedAsync<TTemplate, TEntity>(string entityName, int entityVersion)
            where TTemplate : class
            where TEntity : class, IEntity<TTemplate>;
        
        /// <summary>Reports registration of an entity field.</summary>
        /// <typeparam name="TTemplate">The entity template type.</typeparam>
        /// <typeparam name="TField">The field value type.</typeparam>
        /// <param name="entityName">The registered entity name.</param>
        /// <param name="entityVersion">The entity definition version.</param>
        /// <param name="fieldName">The registered field name.</param>
        /// <param name="defaultValue">The field's default value, if one is configured.</param>
        /// <param name="computed">Whether the field is computed.</param>
        /// <param name="access">The field's read and write access policy.</param>
        Task FieldAddedAsync<TTemplate, TField>(string entityName, int entityVersion, string fieldName, TField defaultValue, bool computed, FieldAccess access)
            where TTemplate : class;
        
        /// <summary>Reports registration of an event-triggered initializer.</summary>
        /// <typeparam name="TTemplate">The entity template type.</typeparam>
        /// <typeparam name="TInitializationEvent">The initialization event type.</typeparam>
        /// <param name="entityName">The registered entity name.</param>
        /// <param name="entityVersion">The entity definition version.</param>
        Task InitializerAddedAsync<TTemplate, TInitializationEvent>(string entityName, int entityVersion)
            where TTemplate : class;
        
        /// <summary>Reports registration of a mutation.</summary>
        /// <typeparam name="TTemplate">The entity template type.</typeparam>
        /// <typeparam name="TMutationEvent">The mutation event type.</typeparam>
        /// <param name="entityName">The registered entity name.</param>
        /// <param name="entityVersion">The entity definition version.</param>
        Task MutationAddedAsync<TTemplate, TMutationEvent>(string entityName, int entityVersion)
            where TTemplate : class;
        
        /// <summary>Reports registration of a projection.</summary>
        /// <typeparam name="TTemplate">The entity template type.</typeparam>
        /// <typeparam name="TProjectionTemplate">The projection value type.</typeparam>
        /// <param name="entityName">The registered entity name.</param>
        /// <param name="entityVersion">The entity definition version.</param>
        /// <param name="publishScope">The projection's configured publication targets.</param>
        Task ProjectionAddedAsync<TTemplate, TProjectionTemplate>(string entityName, int entityVersion, PublishScope publishScope)
            where TTemplate : class;
    }
}
