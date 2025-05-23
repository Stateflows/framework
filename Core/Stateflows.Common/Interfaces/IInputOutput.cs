using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Stateflows.Activities;

namespace Stateflows.Common.Interfaces
{
    public interface IInputOutput : IWatches
    {
        Task<RequestResult<TokensOutput>> SendInputAsync(Action<ITokensInput> tokensAction, IEnumerable<EventHeader> headers = null);

        Task<RequestResult<TokensOutput>> SendInputAsync<TToken>(params TToken[] tokens);

        Task<IWatcher> WatchOutputAsync(Action<ITokensOutput> handler)
            => WatchAsync<TokensOutput>(handler);

        Task<IWatcher> WatchOutputAsync<TToken>(Action<IEnumerable<TToken>> handler)
            => WatchAsync<TokensOutput<TToken>>(output => handler(output.GetAll()));
    }
}
