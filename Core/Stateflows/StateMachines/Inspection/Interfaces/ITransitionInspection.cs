using System.Collections.Generic;

namespace Stateflows.StateMachines.Inspection.Interfaces
{
    public interface ITransitionInspection
    {
        IEnumerable<string> Triggers { get; }

        bool Active { get; }
        
        bool IsElse { get; }

        IActionInspection Guard { get; }

        IActionInspection Effect { get; }

        IStateInspection Source { get; }

        IStateInspection Target { get; }
    }
}
