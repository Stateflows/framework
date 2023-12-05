using System;
using System.Collections.Generic;

namespace Stateflows.StateMachines.Context.Classes
{
    public class StateValues
    {
        public Dictionary<string, string> Values { get; set; } = new Dictionary<string, string>();

        public StateMachineId? SubmachineId { get; set; } = null;

        public Dictionary<string, Guid> TimeEventIds { get; set; } = new Dictionary<string, Guid>();

        public Dictionary<string, string> TimeTokenIds { get; set; } = new Dictionary<string, string>();
    }
}
