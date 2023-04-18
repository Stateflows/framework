using System.Collections.Generic;

namespace Stateflows.StateMachines.Models
{
    internal class Action<TDelegate>
    {
        public Graph Graph { get; set; }

        public IList<TDelegate> Actions { get; set; } = new List<TDelegate>();

        public string Name { get; set; }
    }
}