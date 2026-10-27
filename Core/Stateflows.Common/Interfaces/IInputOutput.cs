using System;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Stateflows.Common.Interfaces
{
    /// <summary>Sends input tokens to a behavior and watches for output tokens.</summary>
    public interface IInputOutput : IWatches
    {
        /// <summary>Sends tokens built through an input callback and awaits the output tokens.</summary>
        /// <param name="tokensAction">Adds the input token values.</param>
        /// <param name="headers">Optional delivery headers.</param>
        /// <returns>The request result containing all output token types.</returns>
        Task<RequestResult<TokensOutput>> SendInputAsync(Action<ITokensInput> tokensAction, IDictionary<string, EventHeader> headers = null);

        /// <summary>Sends input tokens of a single type and awaits the output tokens.</summary>
        /// <typeparam name="TToken">The input token type.</typeparam>
        /// <param name="tokens">The token values to send.</param>
        /// <returns>The request result containing all output token types.</returns>
        Task<RequestResult<TokensOutput>> SendInputAsync<TToken>(params TToken[] tokens);

        /// <summary>Watches for output tokens of any type.</summary>
        /// <param name="handler">Callback receiving the output token collection.</param>
        /// <returns>A watcher that ends the subscription when disposed.</returns>
        Task<IWatcher> WatchOutputAsync(Action<ITokensOutput> handler)
            => WatchAsync<TokensOutput>(handler);

        /// <summary>Watches for output tokens assignable to a specified type.</summary>
        /// <typeparam name="TToken">The requested output token type.</typeparam>
        /// <param name="handler">Callback receiving the matching output tokens.</param>
        /// <returns>A watcher that ends the subscription when disposed.</returns>
        Task<IWatcher> WatchOutputAsync<TToken>(Action<IEnumerable<TToken>> handler)
            => WatchAsync<TokensOutput<TToken>>(output => handler(output.GetAll()));
    }
}
