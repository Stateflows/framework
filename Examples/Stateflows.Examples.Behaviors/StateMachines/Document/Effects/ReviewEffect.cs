using Stateflows.Common;
using Stateflows.Common.Attributes;
using Stateflows.Examples.Common.Events;
using Stateflows.StateMachines;

namespace Stateflows.Examples.Behaviors.StateMachines.Document.Effects;

public class ReviewEffect(
    [GlobalValue] IValue<int> Rating
) : ITransitionEffect<Review>
{
    public async Task EffectAsync(Review @event)
    {
        await Rating.SetAsync(@event.Rating);
    }
}