using Stateflows.Common;
using Stateflows.StateMachines;

namespace WarszawskieDniInformatyki.StateMachines.Document.States;

public class New(
    ILogger<New> logger,
    IBehaviorContext context
) : IStateEntry
{
    public Task OnEntryAsync()
    {
        logger.LogTrace($"Created new document: {context.Id.Instance}");
        
        return Task.CompletedTask;
    }
}