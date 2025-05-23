using Stateflows.Common;
using Stateflows.Common.Attributes;
using Stateflows.StateMachines;
using WarszawskieDniInformatyki.StateMachines.Document.Events;

namespace WarszawskieDniInformatyki.StateMachines.Document.Effects;

public class ReviewEffect(
    [GlobalValue] IValue<int> Rating
) : ITransitionEffect<Review>
{
    public async Task EffectAsync(Review @event)
    {
        await Rating.SetAsync(@event.Rating);
    }
}