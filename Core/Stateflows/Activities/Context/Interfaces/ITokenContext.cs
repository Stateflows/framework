using Stateflows.Common;
using System.Collections.Generic;

namespace Stateflows.Activities.Context.Interfaces
{
    public interface ITokenContext<out TToken> : IActivityActionContext
        // where TToken : Token, new()
    {
        IEnumerable<TToken> Tokens { get; }
    }
}
