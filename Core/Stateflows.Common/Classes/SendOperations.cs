using System.Collections.Generic;
using System.Threading.Tasks;
using Stateflows.Common.Interfaces;
using Stateflows.Common.Utilities;

namespace Stateflows.Common.Classes;

internal static class SendOperations
{
    public static Task SendAsync(IBehavior behavior, EventHolder eventHolder)
    {
        _ = StateflowsJsonConverter.SerializePolymorphicObject(eventHolder.BoxedPayload);
        foreach (var header in eventHolder.Headers.Values)
            _ = StateflowsJsonConverter.SerializePolymorphicObject(header);

        return behavior.SendAsync(eventHolder);
    }
}
