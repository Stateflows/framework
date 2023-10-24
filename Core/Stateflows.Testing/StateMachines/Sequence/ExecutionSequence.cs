using System;
using System.Collections.Generic;
using Stateflows.Common;
using Stateflows.StateMachines.Events;

namespace Stateflows.Testing.StateMachines.Sequence
{
    internal class ExecutionSequence : IExecutionSequenceBuilder
    {
        private List<string> Sequence = new List<string>();

        public void ValidateWith(IExecutionSequenceBuilder sequenceBuilder)
        {
            var actualSequence = (sequenceBuilder as ExecutionSequence).Sequence;

            int index = 0;
            for (int x = 0; x < Sequence.Count; x++)
            {
                var entry = Sequence[x];

                bool found = false;
                for (int i = index; i < actualSequence.Count; i++)
                {
                    if (actualSequence[i] == entry)
                    {
                        index = i + 1;
                        found = true;
                        break;
                    }
                }

                if (!found)
                {
                    throw new Exception($"Expected execution step \"{entry}\" not found");
                }
            }
        }

        public IExecutionSequenceBuilder DefaultTransitionEffect(string sourceStateName, string targetVertexName)
            => TransitionEffect(EventInfo<Completion>.Name, sourceStateName, targetVertexName);

        public IExecutionSequenceBuilder DefaultTransitionGuard(string sourceStateName, string targetVertexName)
            => TransitionGuard(EventInfo<Completion>.Name, sourceStateName, targetVertexName);

        public IExecutionSequenceBuilder InternalTransitionEffect(string eventName, string sourceStateName)
            => TransitionEffect(eventName, sourceStateName, "");

        public IExecutionSequenceBuilder InternalTransitionGuard(string eventName, string sourceStateName)
            => TransitionGuard(eventName, sourceStateName, "");

        public IExecutionSequenceBuilder StateEntry(string stateName)
        {
            Sequence.Add($"{stateName}::entry");
            return this;
        }

        public IExecutionSequenceBuilder StateExit(string stateName)
        {
            Sequence.Add($"{stateName}::exit");
            return this;
        }

        public IExecutionSequenceBuilder StateInitialize(string stateName)
        {
            Sequence.Add($"{stateName}::initialize");
            return this;
        }

        public IExecutionSequenceBuilder StateFinalize(string stateName)
        {
            Sequence.Add($"{stateName}::finalize");
            return this;
        }

        public IExecutionSequenceBuilder StateMachineInitialize()
        {
            Sequence.Add($"StateMachine::initialize");
            return this;
        }

        public IExecutionSequenceBuilder StateMachineFinalize()
        {
            Sequence.Add($"StateMachine::finalize");
            return this;
        }

        public IExecutionSequenceBuilder TransitionEffect(string eventName, string sourceStateName, string targetVertexName)
        {
            Sequence.Add($"{sourceStateName}--{eventName}/effect-->{targetVertexName}");
            return this;
        }

        public IExecutionSequenceBuilder TransitionGuard(string eventName, string sourceStateName, string targetVertexName)
        {
            Sequence.Add($"{sourceStateName}--{eventName}/[guard]-->{targetVertexName}");
            return this;
        }
    }
}
