using Stateflows.Common.Context;

namespace Stateflows.Common.Extensions;

internal static class StateflowsContextExtensions
{
    public static SetContextOwner GetContextOwnerSetter(this StateflowsContext stateflowsContext)
        => new()
        {
            ContextOwnerId = stateflowsContext.ContextOwnerId ?? stateflowsContext.Id,
            ContextParentId = stateflowsContext.Id
        };
}