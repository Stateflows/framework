using System;
using System.Threading.Tasks;
using Stateflows.Common;
using Stateflows.StateMachines;
using Stateflows.StateMachines.Context.Interfaces;

namespace Stateflows.Testing.StateMachines.Sequence
{
    public class ExecutionSequenceObserver : IStateMachineObserver
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

        Task IStateMachineObserver.AfterStateEntryAsync(IStateActionContext context)
        {
            SequenceBuilder.StateEntry(context.CurrentState.Name);

            return Task.CompletedTask;
        }

        Task IStateMachineObserver.AfterStateExitAsync(IStateActionContext context)
        {
            SequenceBuilder.StateExit(context.CurrentState.Name);

            return Task.CompletedTask;
        }

        Task IStateMachineObserver.AfterStateInitializeAsync(IStateActionContext context)
        {
            SequenceBuilder.StateInitialize(context.CurrentState.Name);

            return Task.CompletedTask;
        }

        Task IStateMachineObserver.AfterStateFinalizeAsync(IStateActionContext context)
        {
            SequenceBuilder.StateFinalize(context.CurrentState.Name);

            return Task.CompletedTask;
        }

        Task IStateMachineObserver.AfterStateMachineInitializeAsync(IStateMachineInitializationContext context, bool initialized)
        {
            SequenceBuilder.StateMachineInitialize();

            return Task.CompletedTask;
        }

        Task IStateMachineObserver.AfterStateMachineFinalizeAsync(IStateMachineActionContext context)
        {
            SequenceBuilder.StateMachineFinalize();

            return Task.CompletedTask;
        }

        Task IStateMachineObserver.AfterTransitionEffectAsync<TEvent>(ITransitionContext<TEvent> context)
        {
            SequenceBuilder.TransitionEffect(Event.GetName(context.Event.GetType()), context.SourceState.Name, context.TargetState?.Name);

            return Task.CompletedTask;
        }

        Task IStateMachineObserver.AfterTransitionGuardAsync<TEvent>(ITransitionContext<TEvent> context, bool guardResult)
        {
            SequenceBuilder.TransitionGuard(Event.GetName(context.Event.GetType()), context.SourceState.Name, context.TargetState?.Name);

            return Task.CompletedTask;
        }
    }
}
