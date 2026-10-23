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

        public override async Task OnStateEntryAsync(IStateActionContext context, Func<Task> next)
        {
            await next();
            SequenceBuilder.StateEntry(context.State.Name);
        }

        public override async Task OnStateExitAsync(IStateActionContext context, Func<Task> next)
        {
            await next();
            SequenceBuilder.StateExit(context.State.Name);
        }

        public override async Task OnStateInitializeAsync(IStateActionContext context, Func<Task> next)
        {
            await next();
            SequenceBuilder.StateInitialize(context.State.Name);
        }

        public override async Task OnStateFinalizeAsync(IStateActionContext context, Func<Task> next)
        {
            await next();
            SequenceBuilder.StateFinalize(context.State.Name);
        }

        public override async Task OnStateMachineInitializeAsync(IStateMachineInitializationContext context, bool implicitInitialization, Func<Task<bool>> next)
        {
            await next();
            SequenceBuilder.StateMachineInitialize();
        }

        public override async Task OnStateMachineFinalizeAsync(IStateMachineActionContext context, Func<Task> next)
        {
            await next();
            SequenceBuilder.StateMachineFinalize();
        }

        public override async Task OnTransitionEffectAsync<TEvent>(ITransitionContext<TEvent> context, Func<Task> next)
        {
            await next();
            SequenceBuilder.TransitionEffect(Event.GetName(context.Event.GetType()), context.Source.Name, context.Target?.Name);
        }

        public override async Task OnTransitionGuardAsync<TEvent>(ITransitionContext<TEvent> context, Func<Task<bool>> next)
        {
            await next();
            SequenceBuilder.TransitionGuard(Event.GetName(context.Event.GetType()), context.Source.Name, context.Target?.Name);
        }
    }
}
