using System.Linq;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using Stateflows.Common;
using Stateflows.Common.Utilities;
using Stateflows.Utils;

namespace Stateflows.Activities
{
    /// <summary>Provides access to tokens staged for output by the current asynchronous activity execution.</summary>
    /// <remarks>Output tokens are ambient to the current execution flow and are forwarded through the activity's connected flows.</remarks>
    public static class OutputTokens
    {
        internal static readonly AsyncLocal<List<TokenHolder>> TokensHolder = new AsyncLocal<List<TokenHolder>>();

        internal static List<TokenHolder> Tokens
            => TokensHolder.Value ??= new List<TokenHolder>();

        /// <summary>Gets all output tokens assignable to the requested type.</summary>
        /// <typeparam name="TToken">The token type to select.</typeparam>
        /// <returns>A new list containing matching output tokens in their current order.</returns>
        /// <remarks>The returned list is a snapshot; modifying it does not change the staged output collection.</remarks>
        public static List<TToken> GetAllOfType<TToken>()
            => Tokens.ToTokensOfType<TToken>().ToList();

        /// <summary>Gets all staged output tokens as boxed payloads.</summary>
        /// <returns>A new list containing every output payload in its current order.</returns>
        /// <remarks>The returned list is a snapshot; modifying it does not change the staged output collection.</remarks>
        public static List<object> GetAll()
            => Tokens.ToBoxedTokens().ToList();

        /// <summary>Gets the number of tokens currently staged for output in this asynchronous flow.</summary>
        public static int Count => Tokens.Count;

        /// <summary>Determines whether an output token equal to the specified value is staged.</summary>
        /// <typeparam name="TToken">The token type to search for.</typeparam>
        /// <param name="item">The token value to locate.</param>
        /// <returns><see langword="true"/> if an equal token is staged; otherwise, <see langword="false"/>.</returns>
        public static bool Contains<TToken>(TToken item)
            => Tokens.Contains(item.ToTokenHolder());
    }

    internal class OutputTokens<TToken> : IOutputTokens<TToken>
    {
        private List<TToken> GetTokens()
            => OutputTokens.Tokens.ToTokensOfType<TToken>().ToList();

        public int Count => GetTokens().Count;

        public bool IsReadOnly => false;

        public void Add(TToken item)
            => OutputTokens.Tokens.Add(item.ToTokenHolder());

        public void AddRange(IEnumerable<TToken> items)
            => OutputTokens.Tokens.AddRange(items.Select(item => item.ToTokenHolder()));

        public void Clear()
        => OutputTokens.Tokens.RemoveAll(token => token is TToken);

        public bool Contains(TToken item)
            => OutputTokens.Tokens.Contains(item.ToTokenHolder());

        public void CopyTo(TToken[] array, int arrayIndex)
            => OutputTokens.Tokens.CopyTo(array.ToTokenHolders().ToArray(), arrayIndex);

        public bool Remove(TToken item)
            => OutputTokens.Tokens.Remove(item.ToTokenHolder());

        public IEnumerator<TToken> GetEnumerator()
            => GetTokens().GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
            => GetTokens().GetEnumerator();
    }
}
