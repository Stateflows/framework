namespace Stateflows.StateMachines
{
    /// <summary>Describes one transition recorded while processing an event.</summary>
    public interface IExecutionStep
    {
        /// <summary>The registered name of the transition's source state or pseudostate.</summary>
        string SourceName { get; }

        /// <summary>The transition's target name; internal transitions have no external target.</summary>
        string TargetName { get; }

        /// <summary>The event object that caused this step, including a completion event for automatic transitions.</summary>
        object TransitionTrigger { get; }
    }
}
