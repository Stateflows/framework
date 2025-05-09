﻿using System.Collections.Generic;
using Stateflows.Common;
using Stateflows.Common.Exceptions;
using Stateflows.StateMachines;
#nullable enable

namespace Stateflows.Testing.StateMachines.Sequence
{
    internal class ExecutionSequence : IExecutionSequenceBuilder
    {
        private readonly List<string> Sequence = new List<string>();

        public void ValidateWith(IExecutionSequenceBuilder sequenceBuilder)
        {
            if (!(sequenceBuilder is ExecutionSequence executionSequence))
            {
                return;
            }

            var actualSequence = executionSequence.Sequence;

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
                    throw new StateflowsDefinitionException($"Expected execution step \"{entry}\" not found");
                }
            }
        }

        public IExecutionSequenceBuilder DefaultTransitionEffect(string sourceStateName, string targetStateName)
            => TransitionEffect(Event<Completion>.Name, sourceStateName, targetStateName);

        public IExecutionSequenceBuilder DefaultTransitionGuard(string sourceStateName, string targetStateName)
            => TransitionGuard(Event<Completion>.Name, sourceStateName, targetStateName);

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

        public IExecutionSequenceBuilder TransitionEffect(string eventName, string sourceStateName, string? targetStateName)
        {
            Sequence.Add($"{sourceStateName}--{eventName}/effect-->{targetStateName ?? string.Empty}");
            return this;
        }

        public IExecutionSequenceBuilder TransitionGuard(string eventName, string sourceStateName, string? targetStateName)
        {
            Sequence.Add($"{sourceStateName}--{eventName}[guard]-->{targetStateName ?? string.Empty}");
            return this;
        }
    }
}
