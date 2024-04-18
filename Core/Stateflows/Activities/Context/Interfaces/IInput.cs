using System.Collections.Generic;

namespace Stateflows.Activities.Context.Interfaces
{
    public interface IInput
    {
        IEnumerable<object> Input { get; }
    }
}
