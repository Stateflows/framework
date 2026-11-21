using System.Collections.Generic;
using System.Threading.Tasks;
using Stateflows.Common.Interfaces;
using Stateflows.Common.Utilities;

namespace Stateflows.Common.Classes;

internal static class SendOperations
{
    public static Task SendAsync<TEvent>(
        IBehavior behavior,
        TEvent @event,
        IDictionary<string, EventHeader>? headers)
    {
        var eventHolder = @event.ToTypedEventHolder(headers);
        _ = StateflowsJsonConverter.SerializePolymorphicObject(eventHolder.BoxedPayload);
        foreach (var header in eventHolder.Headers.Values)
            _ = StateflowsJsonConverter.SerializePolymorphicObject(header);

        return behavior.SendAsync(eventHolder);
    }
}
