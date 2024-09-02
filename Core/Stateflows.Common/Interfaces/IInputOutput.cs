using Stateflows.Activities.Events;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Stateflows.Common.Interfaces
{
    public interface IInputOutput
    {
        Task<SendResult> SendInputAsync(Action<ITokensInput> tokensAction);

        Task<SendResult> SendInputAsync<TToken>(params TToken[] tokens);

        Task WatchOutputAsync(Action<ITokensOutput> handler);

        Task WatchOutputAsync<TToken>(Action<IEnumerable<TToken>> handler);

        Task UnwatchOutputAsync();

        Task UnwatchOutputAsync<TToken>();
    }
}
