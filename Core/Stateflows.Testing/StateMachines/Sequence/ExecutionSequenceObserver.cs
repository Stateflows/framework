﻿using System;
using Stateflows.Common;
using Stateflows.StateMachines;
using Stateflows.StateMachines.Context.Interfaces;

namespace Stateflows.Testing.StateMachines.Sequence
{
    public class ExecutionSequenceObserver : StateMachineObserver
    {
        private readonly IExecutionSequenceBuilder SequenceBuilder;

        public ExecutionSequenceObserver(IExecutionSequenceBuilder sequenceBuilder)
        {
            SequenceBuilder = sequenceBuilder;
        }

        public void Verify(Action<IExecutionSequenceBuilder> sequenceBuildAction)
        {
            var builder = new ExecutionSequence();
            sequenceBuildAction(builder);
            builder.ValidateWith(SequenceBuilder);
        }

        public override void AfterStateEntry(IStateActionContext context)
        {
            SequenceBuilder.StateEntry(context.CurrentState.Name);
        }

        public override void AfterStateExit(IStateActionContext context)
        {
            SequenceBuilder.StateExit(context.CurrentState.Name);
        }

        public override void AfterStateInitialize(IStateActionContext context)
        {
            SequenceBuilder.StateInitialize(context.CurrentState.Name);
        }

        public override void AfterStateFinalize(IStateActionContext context)
        {
            SequenceBuilder.StateFinalize(context.CurrentState.Name);
        }

        public override void AfterStateMachineInitialize(IStateMachineInitializationContext context, bool initialized)
        {
            SequenceBuilder.StateMachineInitialize();
        }

        public override void AfterStateMachineFinalize(IStateMachineActionContext context)
        {
            SequenceBuilder.StateMachineFinalize();
        }

        public override void AfterTransitionEffect<TEvent>(ITransitionContext<TEvent> context)
        {
            SequenceBuilder.TransitionEffect(Event.GetName(context.Event.GetType()), context.Source.Name, context.Target?.Name);
        }

        public override void AfterTransitionGuard<TEvent>(ITransitionContext<TEvent> context, bool guardResult)
        {
            SequenceBuilder.TransitionGuard(Event.GetName(context.Event.GetType()), context.Source.Name, context.Target?.Name);
        }
    }
}
