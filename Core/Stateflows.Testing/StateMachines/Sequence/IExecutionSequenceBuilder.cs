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

        IExecutionSequenceBuilder TransitionGuard(string eventName, string sourceStateName, string? targetVertexName);
        IExecutionSequenceBuilder TransitionEffect(string eventName, string sourceStateName, string? targetVertexName);

        IExecutionSequenceBuilder InternalTransitionGuard(string eventName, string sourceStateName);
        IExecutionSequenceBuilder InternalTransitionEffect(string eventName, string sourceStateName);

        IExecutionSequenceBuilder DefaultTransitionGuard(string sourceStateName, string targetVertexName);
        IExecutionSequenceBuilder DefaultTransitionEffect(string sourceStateName, string targetVertexName);
    }
}
