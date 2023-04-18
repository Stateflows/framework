using System.Collections.Generic;
using Stateflows.Common;

namespace Stateflows.StateMachines.Events
{
    public sealed class ExpectedEventsResponse : Response
    {
        public IEnumerable<string> ExpectedEvents { get; set; }
    }
}
