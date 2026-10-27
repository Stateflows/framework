using System;
using System.Threading.Tasks;
using Stateflows.Common;
using Stateflows.StateMachines;
using Stateflows.StateMachines.Context.Interfaces;

namespace Stateflows.Testing.StateMachines.Sequence
{
    /// <summary>Records executed state lifecycle callbacks and transition guards/effects for later sequence verification.</summary>
    public class ExecutionSequenceObserver : StateMachineObserver
    {
        private readonly IExecutionSequenceBuilder SequenceBuilder;

        /// <summary>Creates an observer that records callbacks in the supplied sequence builder.</summary>
        /// <param name="sequenceBuilder">The builder that receives observed execution steps.</param>
        public ExecutionSequenceObserver(IExecutionSequenceBuilder sequenceBuilder)
        {
            SequenceBuilder = sequenceBuilder;
        }

        /// <summary>Verifies that the expected steps occur in order in the recorded execution.</summary>
        /// <param name="sequenceBuildAction">Describes the expected steps using the sequence builder API.</param>
        /// <exception cref="Stateflows.Common.Exceptions.StateflowsDefinitionException">An expected step is missing or appears out of order.</exception>
        public void Verify(Action<IExecutionSequenceBuilder> sequenceBuildAction)
        {
            var builder = new ExecutionSequence();
            sequenceBuildAction(builder);
            builder.ValidateWith(SequenceBuilder);
        }

        /// <summary>Records state entry after the state-entry pipeline completes.</summary>
        /// <param name="context">The state action context for the entered state.</param>
        /// <param name="next">The remaining state-entry pipeline.</param>
        public override async Task OnStateEntryAsync(IStateActionContext context, Func<Task> next)
        {
            await next();
            SequenceBuilder.StateEntry(context.State.Name);
        }

        /// <summary>Records state exit after the state-exit pipeline completes.</summary>
        /// <param name="context">The state action context for the exited state.</param>
        /// <param name="next">The remaining state-exit pipeline.</param>
        public override async Task OnStateExitAsync(IStateActionContext context, Func<Task> next)
        {
            await next();
            SequenceBuilder.StateExit(context.State.Name);
        }

        /// <summary>Records state initialization after the state initialization pipeline completes.</summary>
        /// <param name="context">The state action context for the initialized state.</param>
        /// <param name="next">The remaining state initialization pipeline.</param>
        public override async Task OnStateInitializeAsync(IStateActionContext context, Func<Task> next)
        {
            await next();
            SequenceBuilder.StateInitialize(context.State.Name);
        }

        /// <summary>Records state finalization after the state finalization pipeline completes.</summary>
        /// <param name="context">The state action context for the finalized state.</param>
        /// <param name="next">The remaining state finalization pipeline.</param>
        public override async Task OnStateFinalizeAsync(IStateActionContext context, Func<Task> next)
        {
            await next();
            SequenceBuilder.StateFinalize(context.State.Name);
        }

        /// <summary>Records state-machine initialization after the initialization pipeline completes.</summary>
        /// <param name="context">The state-machine initialization context.</param>
        /// <param name="implicitInitialization">Whether initialization was requested implicitly by event processing.</param>
        /// <param name="next">The remaining initialization pipeline and its success result.</param>
        public override async Task OnStateMachineInitializeAsync(IStateMachineInitializationContext context, bool implicitInitialization, Func<Task<bool>> next)
        {
            await next();
            SequenceBuilder.StateMachineInitialize();
        }

        /// <summary>Records state-machine finalization after the finalization pipeline completes.</summary>
        /// <param name="context">The state-machine action context.</param>
        /// <param name="next">The remaining finalization pipeline.</param>
        public override async Task OnStateMachineFinalizeAsync(IStateMachineActionContext context, Func<Task> next)
        {
            await next();
            SequenceBuilder.StateMachineFinalize();
        }

        /// <summary>Records an event-triggered transition effect after the effect pipeline completes.</summary>
        /// <typeparam name="TEvent">The event type that triggered the transition.</typeparam>
        /// <param name="context">The transition context, including its event, source, and optional target.</param>
        /// <param name="next">The remaining transition-effect pipeline.</param>
        public override async Task OnTransitionEffectAsync<TEvent>(ITransitionContext<TEvent> context, Func<Task> next)
        {
            await next();
            SequenceBuilder.TransitionEffect(Event.GetName(context.Event.GetType()), context.Source.Name, context.Target?.Name);
        }

        /// <summary>Records an event-triggered transition guard after the guard pipeline completes.</summary>
        /// <typeparam name="TEvent">The event type that triggered the transition.</typeparam>
        /// <param name="context">The transition context, including its event, source, and optional target.</param>
        /// <param name="next">The remaining guard pipeline and its decision.</param>
        public override async Task OnTransitionGuardAsync<TEvent>(ITransitionContext<TEvent> context, Func<Task<bool>> next)
        {
            await next();
            SequenceBuilder.TransitionGuard(Event.GetName(context.Event.GetType()), context.Source.Name, context.Target?.Name);
        }
    }
}
