using System.Collections.Generic;
using System.Threading.Tasks;
using Stateflows.Entities.Attributes;
using Stateflows.Entities.Enums;

namespace Stateflows.Entities;

/// <summary>Inspects a registered entity definition to build structural integrations, rather than observing a live instance.</summary>
/// <remarks>
/// Callbacks are awaited serially: definition start, optional implementation type, fields,
/// default initializers, event initializers, mutations, projections, and definition end.
/// Element identifiers describe the registered model, not an instance or delivery. Interpret
/// them within the entity name and version; unchanged definitions retain their identifiers.
/// Use <see cref="EntityVisitor"/> to override only selected callbacks.
/// </remarks>
public interface IEntityVisitor
{
    /// <summary>Begins traversal of a finalized entity definition, before its implementation type and members.</summary>
    /// <typeparam name="TTemplate">The interface declaring the entity's fields, mutations, and projections.</typeparam>
    /// <param name="entityId">The canonical entity definition identifier, not an instance identifier.</param>
    /// <param name="entityName">The registered entity name.</param>
    /// <param name="entityVersion">The registered definition version.</param>
    /// <param name="ownerClass">The owning behavior class, or null for a top-level entity.</param>
    /// <param name="parentClass">The immediate parent behavior class, or null when none is registered.</param>
    /// <param name="hasDefaultInstance">Whether default-instance initialization is registered for this entity class.</param>
    /// <returns>A task completing when the visitor is ready to receive the definition's members.</returns>
    Task EntityAddingAsync<TTemplate>(string entityId, string entityName, int entityVersion, BehaviorClass? ownerClass, BehaviorClass? parentClass, bool hasDefaultInstance) where TTemplate : class;

    /// <summary>Reports the implementation in addition to the general definition callback.</summary>
    /// <typeparam name="TTemplate">The entity template interface.</typeparam>
    /// <typeparam name="TEntity">The registered implementation type; omitted for definitions without an implementation class.</typeparam>
    /// <param name="entityId">The canonical entity definition identifier.</param>
    /// <param name="entityName">The registered entity name.</param>
    /// <param name="entityVersion">The registered definition version.</param>
    /// <returns>A task completing when implementation metadata has been inspected.</returns>
    Task EntityTypeAddedAsync<TTemplate, TEntity>(string entityId, string entityName, int entityVersion) where TTemplate : class where TEntity : class, IEntity<TTemplate>;

    /// <summary>Reports a field's finalized policy and explicitly configured default.</summary>
    /// <typeparam name="TTemplate">The entity template interface.</typeparam>
    /// <typeparam name="TField">The declared field value type.</typeparam>
    /// <param name="fieldId">The canonical field identifier within the entity definition.</param>
    /// <param name="entityName">The registered entity name, not the field name.</param>
    /// <param name="entityVersion">The registered definition version.</param>
    /// <param name="fieldName">The registered field name used by field read and write events.</param>
    /// <param name="hasDefaultValue">Whether a default was explicitly configured, including an explicit default of null or zero.</param>
    /// <param name="defaultValue">The configured default; meaningful only when <paramref name="hasDefaultValue"/> is true.</param>
    /// <param name="isComputed">Whether the field is calculated rather than stored directly.</param>
    /// <param name="fieldAccess">The permitted external read and write operations.</param>
    /// <returns>A task completing when field metadata has been inspected.</returns>
    Task FieldAddedAsync<TTemplate, TField>(string fieldId, string entityName, int entityVersion, string fieldName, bool hasDefaultValue, TField defaultValue, bool isComputed, FieldAccess fieldAccess) where TTemplate : class;

    /// <summary>Reports a default initializer registration.</summary>
    /// <typeparam name="TTemplate">The entity template interface.</typeparam>
    /// <param name="initializerId">The canonical identifier of this initializer registration.</param>
    /// <param name="entityName">The registered entity name.</param>
    /// <param name="entityVersion">The registered definition version.</param>
    /// <param name="order">The zero-based default-initializer registration order.</param>
    /// <returns>A task completing when initializer metadata has been inspected.</returns>
    Task DefaultInitializerAddedAsync<TTemplate>(string initializerId, string entityName, int entityVersion, int order) where TTemplate : class;

    /// <summary>Reports an event initialization binding.</summary>
    /// <typeparam name="TTemplate">The entity template interface.</typeparam>
    /// <typeparam name="TEvent">The event payload type accepted by the initializer.</typeparam>
    /// <param name="initializerId">The canonical identifier of this initialization binding.</param>
    /// <param name="entityName">The registered entity name.</param>
    /// <param name="entityVersion">The registered definition version.</param>
    /// <param name="eventNames">The event names associated with the binding.</param>
    /// <returns>A task completing when the initialization binding has been inspected.</returns>
    Task InitializerAddedAsync<TTemplate, TEvent>(string initializerId, string entityName, int entityVersion, IReadOnlyList<string> eventNames) where TTemplate : class;

    /// <summary>Reports an event mutation binding.</summary>
    /// <typeparam name="TTemplate">The entity template interface.</typeparam>
    /// <typeparam name="TEvent">The mutation event payload type.</typeparam>
    /// <param name="mutationId">The canonical identifier of this mutation binding.</param>
    /// <param name="entityName">The registered entity name.</param>
    /// <param name="entityVersion">The registered definition version.</param>
    /// <param name="eventNames">The event names associated with the binding.</param>
    /// <returns>A task completing when mutation metadata has been inspected.</returns>
    Task MutationAddedAsync<TTemplate, TEvent>(string mutationId, string entityName, int entityVersion, IReadOnlyList<string> eventNames) where TTemplate : class;

    /// <summary>Reports a projection and its publication policy.</summary>
    /// <typeparam name="TTemplate">The entity template interface.</typeparam>
    /// <typeparam name="TProjection">The projection result type.</typeparam>
    /// <param name="projectionId">The canonical identifier of this projection registration.</param>
    /// <param name="entityName">The registered entity name.</param>
    /// <param name="entityVersion">The registered definition version.</param>
    /// <param name="publishScope">The configured scope for publishing projection updates.</param>
    /// <returns>A task completing when projection metadata has been inspected.</returns>
    Task ProjectionAddedAsync<TTemplate, TProjection>(string projectionId, string entityName, int entityVersion, PublishScope publishScope) where TTemplate : class;

    /// <summary>Ends the definition, after all members and types have been reported.</summary>
    /// <typeparam name="TTemplate">The entity template interface.</typeparam>
    /// <param name="entityId">The same canonical entity identifier reported at traversal start.</param>
    /// <param name="entityName">The registered entity name.</param>
    /// <param name="entityVersion">The registered definition version.</param>
    /// <returns>A task completing when derived structural output for this definition has been finalized.</returns>
    Task EntityAddedAsync<TTemplate>(string entityId, string entityName, int entityVersion) where TTemplate : class;
}
