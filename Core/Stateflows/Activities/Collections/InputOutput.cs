using System.Linq;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using Stateflows.Common.Utilities;
using Stateflows.Utils;

namespace Stateflows.Activities
{
    internal class BaseInputOutputTokens<TToken> : IInputTokens<TToken>, IOutputTokens<TToken>
    {
        protected readonly InputTokens<TToken> InputTokens = new();
        protected readonly OutputTokens<TToken> OutputTokens = new();

        public void AddRange(IEnumerable<TToken> items)
            => OutputTokens.AddRange(items);

        public IEnumerator<TToken> GetEnumerator()
            => InputTokens.Tokens.GetEnumerator();
        
        IEnumerator IEnumerable.GetEnumerator()
            => InputTokens.Tokens.GetEnumerator();

        public void Add(TToken item)
            => OutputTokens.Add(item);

        public void Clear()
            => OutputTokens.Clear();

        public bool Contains(TToken item)
            => OutputTokens.Contains(item);

        public void CopyTo(TToken[] array, int arrayIndex)
            => OutputTokens.CopyTo(array, arrayIndex);

        public bool Remove(TToken item)
            => OutputTokens.Remove(item);

        public int Count => OutputTokens.Count;
        public bool IsReadOnly => OutputTokens.IsReadOnly;
    }

    internal class InputOutputTokens<TToken> : BaseInputOutputTokens<TToken>, IInputOutputTokens<TToken>
    {
        public void PassAllOn()
            => OutputTokens.AddRange(InputTokens.Tokens);
    }

    internal class InputOutputToken<TToken> : BaseInputOutputTokens<TToken>, IInputOutputToken<TToken>
    {
        public TToken Token
            => InputTokens.Tokens.First();

        public void PassOn()
            => OutputTokens.Add(Token);
    }

    internal class OptionalInputOutputTokens<TToken> : BaseInputOutputTokens<TToken>, IOptionalInputTokens<TToken>
    {
        public void PassAllOn()
            => new OutputTokens<TToken>().AddRange(InputTokens.Tokens);
    }

    internal class OptionalInputOutputToken<TToken> : BaseInputOutputTokens<TToken>, IOptionalInputToken<TToken>
    {
        public bool IsAvailable
            => InputTokens.Tokens.OfType<TokenHolder<TToken>>().Any();
        
        public TToken TokenOrDefault
            => InputTokens.Tokens.OfType<TokenHolder<TToken>>().Select(t => t.Payload).FirstOrDefault();

        public void PassOn()
            => OutputTokens.Add(TokenOrDefault);
    }
}
