using System.Collections.Generic;
using Stateflows.Common;

namespace Stateflows.Activities.Context.Interfaces
{
    public interface IInput
    {
        IEnumerable<Token> Input { get; }
    }
}
