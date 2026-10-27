using System.Collections.Generic;
using System.Threading.Tasks;
using Stateflows.Entities;

namespace Stateflows.Common.Interfaces;

/// <summary>Provides operations for mutating and reading related entity behaviors from a behavior context.</summary>
/// <typeparam name="TReturn">The context type used by related fluent API contracts.</typeparam>
public interface IEntityOperations<TReturn>
{
    /// <summary>Sends a mutation event to a related entity by its registered name.</summary>
    /// <typeparam name="TMutationEvent">The mutation event type.</typeparam>
    /// <param name="entityName">The registered entity name.</param>
    /// <param name="mutationEvent">The event to apply to the entity.</param>
    /// <param name="headers">Optional delivery headers.</param>
    /// <returns><see langword="true"/> when the entity consumed the event.</returns>
    Task<bool> TryMutateAsync<TMutationEvent>(string entityName, TMutationEvent mutationEvent, IDictionary<string, EventHeader>? headers = null);
    
    /// <summary>Sends a mutation event to the entity identified by its template type.</summary>
    /// <typeparam name="TTemplate">The entity template type.</typeparam>
    /// <typeparam name="TMutationEvent">The mutation event type.</typeparam>
    /// <param name="mutationEvent">The event to apply to the entity.</param>
    /// <param name="headers">Optional delivery headers.</param>
    /// <returns><see langword="true"/> when the entity consumed the event.</returns>
    Task<bool> TryMutateAsync<TTemplate, TMutationEvent>(TMutationEvent mutationEvent, IDictionary<string, EventHeader>? headers = null)
        where TTemplate : class
        =>  TryMutateAsync<TMutationEvent>(Entity<TTemplate>.Name, mutationEvent, headers);

    /// <summary>Requests a projection from an entity by its registered name.</summary>
    /// <typeparam name="TProjection">The projection value type.</typeparam>
    /// <param name="entityName">The registered entity name.</param>
    /// <param name="headers">Optional delivery headers.</param>
    /// <returns>A success flag and the projection value when available.</returns>
    Task<(bool Success, TProjection Projection)> TryGetProjectionAsync<TProjection>(string entityName, IDictionary<string, EventHeader>? headers = null);
    
    /// <summary>Requests a projection from the entity identified by its template type.</summary>
    /// <typeparam name="TTemplate">The entity template type.</typeparam>
    /// <typeparam name="TProjection">The projection value type.</typeparam>
    /// <param name="headers">Optional delivery headers.</param>
    /// <returns>A success flag and the projection value when available.</returns>
    Task<(bool Success, TProjection Projection)> TryGetProjectionAsync<TTemplate, TProjection>(IDictionary<string, EventHeader>? headers = null)
        where TTemplate : class
        => TryGetProjectionAsync<TProjection>(Entity<TTemplate>.Name, headers);

    /// <summary>Sets a field on an entity by its registered name.</summary>
    /// <typeparam name="TFieldType">The field value type.</typeparam>
    /// <param name="entityName">The registered entity name.</param>
    /// <param name="fieldName">The registered field name.</param>
    /// <param name="fieldValue">The value to assign.</param>
    /// <param name="headers">Optional delivery headers.</param>
    /// <returns><see langword="true"/> when the field update is consumed.</returns>
    Task<bool> TrySetFieldAsync<TFieldType>(string entityName, string fieldName, TFieldType fieldValue, IDictionary<string, EventHeader>? headers = null);

    /// <summary>Sets a field on the entity identified by its template type.</summary>
    /// <typeparam name="TTemplate">The entity template type.</typeparam>
    /// <typeparam name="TFieldType">The field value type.</typeparam>
    /// <param name="fieldName">The registered field name.</param>
    /// <param name="fieldValue">The value to assign.</param>
    /// <param name="headers">Optional delivery headers.</param>
    /// <returns><see langword="true"/> when the field update is consumed.</returns>
    Task<bool> TrySetFieldAsync<TTemplate, TFieldType>(string fieldName, TFieldType fieldValue, IDictionary<string, EventHeader>? headers = null)
        where TTemplate : class
        => TrySetFieldAsync<TFieldType>(Entity<TTemplate>.Name, fieldName, fieldValue, headers);

    /// <summary>Reads a field from an entity by its registered name.</summary>
    /// <typeparam name="TFieldType">The expected field value type.</typeparam>
    /// <param name="entityName">The registered entity name.</param>
    /// <param name="fieldName">The registered field name.</param>
    /// <param name="headers">Optional delivery headers.</param>
    /// <returns>A success flag and the field value when the read is available.</returns>
    Task<(bool Success, TFieldType Field)> TryGetFieldAsync<TFieldType>(string entityName, string fieldName, IDictionary<string, EventHeader>? headers = null);

    /// <summary>Reads a field from the entity identified by its template type.</summary>
    /// <typeparam name="TTemplate">The entity template type.</typeparam>
    /// <typeparam name="TFieldType">The expected field value type.</typeparam>
    /// <param name="fieldName">The registered field name.</param>
    /// <param name="headers">Optional delivery headers.</param>
    /// <returns>A success flag and the field value when the read is available.</returns>
    Task<(bool Success, TFieldType Field)> TryGetFieldAsync<TTemplate, TFieldType>(string fieldName,
        IDictionary<string, EventHeader>? headers = null)
        where TTemplate : class
        => TryGetFieldAsync<TFieldType>(Entity<TTemplate>.Name, fieldName, headers);
}