namespace Stateflows.StateMachines.Inspection.Interfaces
{
    public interface ITransitionInspection
    {
        string Trigger { get; }

        bool Active { get; }

        IActionInspection Guard { get; }

        IActionInspection Effect { get; }

        IStateInspection Source { get; }

        IStateInspection Target { get; }
    }
}
