using System.Threading.Tasks;
using System.Collections.Generic;
using Stateflows.Common;

namespace Stateflows.Entities
{
    public interface IEntityBehavior : IBehavior
    {
        public async Task<(bool Success, TProjection Projection)> TryGetProjection<TProjection>(
            IDictionary<string, EventHeader> headers = null)
        {
            var response = await RequestAsync(new ProjectionRequest<TProjection>(), headers);
            return response.Status == EventStatus.Consumed && response.Response != null
                ? (true, response.Response)
                : (false, default);
        }

        public async Task<(bool Success, TFieldValue FieldValue)> TryGetFieldValue<TFieldValue>(string fieldName,
            IDictionary<string, EventHeader> headers = null)
        {
            var response = await RequestAsync(new FieldValueRequest<TFieldValue>() { FieldName = fieldName }, headers);
            return response.Status == EventStatus.Consumed && response.Response != null
                ? (true, response.Response)
                : (false, default);
        }
    }
}
