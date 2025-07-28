using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Stateflows.Activities
{
    public interface IInputTokens<out TToken> : IEnumerable<TToken>
    {
        void PassAllOn();
    }

    public interface IInputToken<out TToken>
    {
        TToken Token { get; }

        void PassOn();
    }

    public interface IOptionalInputTokens<out TToken> : IEnumerable<TToken>
    {
        void PassAllOn();
    }

    public interface IOptionalInputToken<out TToken>
    {
        bool IsAvailable { get; }
        
        TToken TokenOrDefault { get; }

        void PassOn();
    }
}
