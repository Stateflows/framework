using System.Collections.Generic;
using System.Threading.Tasks;

namespace Stateflows.Common.Interfaces;

public interface IEntityOperations<TReturn>
{
    Task<bool> TryMutateAsync<TMutationEvent>(TMutationEvent mutationEvent, IDictionary<string, EventHeader> headers = null);

    Task<(bool Success, TProjection Projection)> TryGetProjectionAsync<TProjection>(IDictionary<string, EventHeader> headers = null);

    Task<bool> TrySetAsync<T>(string fieldName, T fieldValue, IDictionary<string, EventHeader> headers = null);

    Task<(bool Success, T Field)> TryGetAsync<T>(string fieldName, IDictionary<string, EventHeader> headers = null);
}