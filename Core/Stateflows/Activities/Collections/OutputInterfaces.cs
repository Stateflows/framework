using System.Collections.Generic;

namespace Stateflows.Activities
{
    public interface IOutputTokens<TToken> : ICollection<TToken>
    {
        void AddRange(IEnumerable<TToken> items);
    }
}
