using System.Collections.Generic;

namespace Stateflows.StateMachines.Inspection.Interfaces
{
    public interface IStateInspection
    {
        string Name { get; }

        bool Active { get; }

        bool IsInitial { get; }

        bool IsFinal { get; }

        IEnumerable<ITransitionInspection> Transitions { get; }

        IEnumerable<IActionInspection> Actions { get; }

        IEnumerable<IRegionInspection> Regions { get; }
    }
}
