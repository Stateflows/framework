#nullable enable

namespace Stateflows.Testing.StateMachines.Sequence
{
    public interface IExecutionSequenceBuilder
    {
        IExecutionSequenceBuilder StateMachineInitialize();
        IExecutionSequenceBuilder StateMachineFinalize();

        IExecutionSequenceBuilder StateInitialize(string stateName);
        IExecutionSequenceBuilder StateFinalize(string stateName);
        IExecutionSequenceBuilder StateEntry(string stateName);
        IExecutionSequenceBuilder StateExit(string stateName);

        IExecutionSequenceBuilder TransitionGuard(string eventName, string sourceStateName, string? targetStateName);
        IExecutionSequenceBuilder TransitionEffect(string eventName, string sourceStateName, string? targetStateName);

        IExecutionSequenceBuilder InternalTransitionGuard(string eventName, string sourceStateName);
        IExecutionSequenceBuilder InternalTransitionEffect(string eventName, string sourceStateName);

        IExecutionSequenceBuilder DefaultTransitionGuard(string sourceStateName, string targetStateName);
        IExecutionSequenceBuilder DefaultTransitionEffect(string sourceStateName, string targetStateName);
    }
}
