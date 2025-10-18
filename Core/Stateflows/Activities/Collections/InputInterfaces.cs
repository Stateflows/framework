using System.Collections.Generic;

namespace Stateflows.Activities
{
    public interface IInputTokens<out TToken> : IEnumerable<TToken>
    { }

    public interface IInputToken<out TToken>
    {
        TToken Token { get; }
    }

    public interface IOptionalInputTokens<out TToken> : IEnumerable<TToken>
    { }

    public interface IOptionalInputToken<out TToken>
    {
        bool IsAvailable { get; }
        
        TToken TokenOrDefault { get; }
    }
}
