using System.Collections.Generic;
using System.Threading.Tasks;
using Stateflows.Entities.Attributes;
using Stateflows.Entities.Enums;

namespace Stateflows.Entities;

/// <summary>No-op base for selective specialization of the canonical entity visitor.</summary>
public abstract class EntityVisitor : IEntityVisitor
{
    /// <inheritdoc/>
    public virtual Task EntityAddingAsync<TTemplate>(string entityId, string entityName, int entityVersion, BehaviorClass? ownerClass, BehaviorClass? parentClass, bool hasDefaultInstance) where TTemplate : class => Task.CompletedTask;
    /// <inheritdoc/>
    public virtual Task EntityTypeAddedAsync<TTemplate, TEntity>(string entityId, string entityName, int entityVersion) where TTemplate : class where TEntity : class, IEntity<TTemplate> => Task.CompletedTask;
    /// <inheritdoc/>
    public virtual Task FieldAddedAsync<TTemplate, TField>(string fieldId, string entityName, int entityVersion, string fieldName, bool hasDefaultValue, TField defaultValue, bool isComputed, FieldAccess fieldAccess) where TTemplate : class => Task.CompletedTask;
    /// <inheritdoc/>
    public virtual Task DefaultInitializerAddedAsync<TTemplate>(string initializerId, string entityName, int entityVersion, int order) where TTemplate : class => Task.CompletedTask;
    /// <inheritdoc/>
    public virtual Task InitializerAddedAsync<TTemplate, TEvent>(string initializerId, string entityName, int entityVersion, IReadOnlyList<string> eventNames) where TTemplate : class => Task.CompletedTask;
    /// <inheritdoc/>
    public virtual Task MutationAddedAsync<TTemplate, TEvent>(string mutationId, string entityName, int entityVersion, IReadOnlyList<string> eventNames) where TTemplate : class => Task.CompletedTask;
    /// <inheritdoc/>
    public virtual Task ProjectionAddedAsync<TTemplate, TProjection>(string projectionId, string entityName, int entityVersion, PublishScope publishScope) where TTemplate : class => Task.CompletedTask;
    /// <inheritdoc/>
    public virtual Task EntityAddedAsync<TTemplate>(string entityId, string entityName, int entityVersion) where TTemplate : class => Task.CompletedTask;
}
