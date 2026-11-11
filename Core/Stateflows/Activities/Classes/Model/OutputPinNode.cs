using System.Collections.Generic;
using System.Threading.Tasks;
using Stateflows.Common.Interfaces;

namespace Stateflows.Activities;

/// <summary>Publishes each input token as an output notification.</summary>
/// <typeparam name="TEvent">The output notification type.</typeparam>
public sealed class OutputPinNode<TEvent>(IInputTokens<TEvent> tokens) :
    IPublishEventActionNode<TEvent>, ITokenConsumer<TEvent>
{
    /// <inheritdoc />
    public Task<IEnumerable<TEvent>> GenerateEventsAsync()
        => Task.FromResult<IEnumerable<TEvent>>([.. tokens]);
}
