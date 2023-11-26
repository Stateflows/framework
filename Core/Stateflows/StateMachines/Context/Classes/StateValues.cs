using System.Collections.Generic;

namespace Stateflows.StateMachines.Context.Classes
{
    public class StateValues
    {
        public Dictionary<string, string> Values { get; set; } = new Dictionary<string, string>();

        public StateMachineId? SubmachineId { get; set; } = null;

        public Dictionary<string, string> TimeEventIds { get; set; } = new Dictionary<string, string>();
    }
}
