using Microsoft.Extensions.Logging;
using Stateflows.Common;
using Stateflows.Common.Attributes;
using Stateflows.StateMachines;

namespace Stateflows.Examples.Behaviors.StateMachines.Document.States;

public class New(
    ILogger<New> logger,
    IBehaviorContext context,
    [GlobalValue(required: false)] string? projectName
) : IStateEntry
{
    public Task OnEntryAsync()
    {
        logger.LogTrace($"Created new document: {context.Id.Instance}");

        return context.Values.SetAsync("the-answer", 42);
    }
}