namespace Stateflows.StateMachines.Inspection.Interfaces
{
    /// <summary>Exposes an inspectable lifecycle, guard, or effect operation and its execution activity.</summary>
    public interface IActionInspection
    {
        /// <summary>The operation name used by the runtime inspection model.</summary>
        string Name { get; }

        /// <summary>Whether this operation is currently executing.</summary>
        bool Active { get; }
    }
}
