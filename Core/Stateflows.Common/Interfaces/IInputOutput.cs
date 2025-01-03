﻿using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Stateflows.Activities.Events;

namespace Stateflows.Common.Interfaces
{
    public interface IInputOutput : IWatches
    {
        Task<SendResult> SendInputAsync(Action<ITokensInput> tokensAction);

        Task<SendResult> SendInputAsync<TToken>(params TToken[] tokens);

        Task<IWatcher> WatchOutputAsync(Action<ITokensOutput> handler)
            => WatchAsync<TokensOutput>(handler);

        Task<IWatcher> WatchOutputAsync<TToken>(Action<IEnumerable<TToken>> handler)
            => WatchAsync<TokensOutput<TToken>>(output => handler(output.GetAll()));
    }
}
