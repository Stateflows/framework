using System;
using System.Threading.Tasks;
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

        public override Task AfterStateEntryAsync(IStateActionContext context)
        {
            SequenceBuilder.StateEntry(context.CurrentState.Name);

            return Task.CompletedTask;
        }

        public override Task AfterStateExitAsync(IStateActionContext context)
        {
            SequenceBuilder.StateExit(context.CurrentState.Name);

            return Task.CompletedTask;
        }

        public override Task AfterStateInitializeAsync(IStateActionContext context)
        {
            SequenceBuilder.StateInitialize(context.CurrentState.Name);

            return Task.CompletedTask;
        }

        public override Task AfterStateFinalizeAsync(IStateActionContext context)
        {
            SequenceBuilder.StateFinalize(context.CurrentState.Name);

            return Task.CompletedTask;
        }

        public override Task AfterStateMachineInitializeAsync(IStateMachineInitializationContext context, bool initialized)
        {
            SequenceBuilder.StateMachineInitialize();

            return Task.CompletedTask;
        }

        public override Task AfterStateMachineFinalizeAsync(IStateMachineActionContext context)
        {
            SequenceBuilder.StateMachineFinalize();

            return Task.CompletedTask;
        }

        public override Task AfterTransitionEffectAsync<TEvent>(ITransitionContext<TEvent> context)
        {
            SequenceBuilder.TransitionEffect(Event.GetName(context.Event.GetType()), context.SourceState.Name, context.TargetState?.Name);

            return Task.CompletedTask;
        }

        public override Task AfterTransitionGuardAsync<TEvent>(ITransitionContext<TEvent> context, bool guardResult)
        {
            SequenceBuilder.TransitionGuard(Event.GetName(context.Event.GetType()), context.SourceState.Name, context.TargetState?.Name);

            return Task.CompletedTask;
        }
    }
}
