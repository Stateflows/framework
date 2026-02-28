using Stateflows.Common;
using Stateflows.Common.Attributes;
using Stateflows.Examples.Common.Events;
using Stateflows.StateMachines;
using IExecutionContext = Stateflows.Common.IExecutionContext;

namespace Stateflows.Examples.Behaviors.StateMachines.Document.Effects;

public class ReviewEffect(
    [GlobalValue] IValue<int> Rating,
    [GlobalValue(required: false)] string? projectName,
    [GlobalValue(Required = false)] DateTime? dueDate,
    // IExecutionContext commonExecutionContext,
    Stateflows.StateMachines.IExecutionContext stateMachineExecutionContext
) : ITransitionEffect<Review>
{
    public async Task EffectAsync(Review @event)
    {
        await Rating.SetAsync(@event.Rating);
    }
}