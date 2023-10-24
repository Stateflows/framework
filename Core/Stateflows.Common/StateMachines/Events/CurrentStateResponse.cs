using Stateflows.Common;
using System.Collections.Generic;

namespace Stateflows.StateMachines.Events
{
    public sealed class CurrentStateResponse : BehaviorStatusResponse
    {
        public IEnumerable<string> StatesStack { get; set; }

        public IEnumerable<string> ExpectedEvents { get; set; }
    }
}
