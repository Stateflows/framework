using System.Collections.Generic;

namespace Stateflows.Activities.Context.Interfaces
{
    public interface ITokenContext<out TToken> : IActivityActionContext
    {
        IEnumerable<TToken> Tokens { get; }
    }
}
