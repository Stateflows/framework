using System.Collections.Generic;
using System.Threading.Tasks;
using Stateflows.Entities;

namespace Stateflows.Common.Interfaces;

public interface IEntityOperations<TReturn>
{
    Task<bool> TryMutateAsync<TMutationEvent>(string entityName, TMutationEvent mutationEvent, IDictionary<string, EventHeader>? headers = null);
    
    Task<bool> TryMutateAsync<TTemplate, TMutationEvent>(TMutationEvent mutationEvent, IDictionary<string, EventHeader>? headers = null)
        where TTemplate : class
        =>  TryMutateAsync<TMutationEvent>(Entity<TTemplate>.Name, mutationEvent, headers);

    Task<(bool Success, TProjection Projection)> TryGetProjectionAsync<TProjection>(string entityName, IDictionary<string, EventHeader>? headers = null);
    
    Task<(bool Success, TProjection Projection)> TryGetProjectionAsync<TTemplate, TProjection>(IDictionary<string, EventHeader>? headers = null)
        where TTemplate : class
        => TryGetProjectionAsync<TProjection>(Entity<TTemplate>.Name, headers);

    Task<bool> TrySetFieldAsync<TFieldType>(string entityName, string fieldName, TFieldType fieldValue, IDictionary<string, EventHeader>? headers = null);

    Task<bool> TrySetFieldAsync<TTemplate, TFieldType>(string fieldName, TFieldType fieldValue, IDictionary<string, EventHeader>? headers = null)
        where TTemplate : class
        => TrySetFieldAsync<TFieldType>(Entity<TTemplate>.Name, fieldName, fieldValue, headers);

    Task<(bool Success, TFieldType Field)> TryGetFieldAsync<TFieldType>(string entityName, string fieldName, IDictionary<string, EventHeader>? headers = null);

    Task<(bool Success, TFieldType Field)> TryGetFieldAsync<TTemplate, TFieldType>(string fieldName,
        IDictionary<string, EventHeader>? headers = null)
        where TTemplate : class
        => TryGetFieldAsync<TFieldType>(Entity<TTemplate>.Name, fieldName, headers);
}