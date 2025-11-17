using Stateflows.Common;

namespace Stateflows.Examples.Behaviors.StateMachines.Document.Builders;

public class DocMetadataBuilder(IBehaviorContext behaviorContext) : IEndpointMetadataBuilder
{
    public async Task BuildMetadataAsync(IDictionary<string, object> metadata)
    {
        var (success, value) = await behaviorContext.Values.TryGetAsync<string>("metadata");
        if (success)
        {
            metadata["custom"] = value;
        }
    }
}