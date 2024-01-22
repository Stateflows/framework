using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Stateflows.Common;
using Stateflows.Common.Interfaces;
using Stateflows.Tools.Tracer.Classes;
using Stateflows.StateMachines;
using Stateflows.StateMachines.Context.Interfaces;
using Stateflows.StateMachines.Inspection.Interfaces;

namespace Stateflows.Tools.Tracer.StateMachines.Classes
{
    internal class Tracer : IStateMachineInspector, IStateMachineInterceptor
    {
        private readonly TraceRepository Repository;

        private Models.Trace Trace { get; set; }  = new Models.Trace();

        private Exception? Exception { get; set; } = null;

        private Stopwatch Stopwatch { get; set; } = new Stopwatch();

        private Stopwatch OverallStopwatch { get; set; } = new Stopwatch();

        public Tracer(TraceRepository repository)
        {
            Repository = repository;
        }

        public Task BeforeStateEntryAsync(IStateActionInspectionContext context)
        {
            Exception = null;
            Stopwatch.Reset();
            return Task.CompletedTask;
        }

        public Task AfterStateEntryAsync(IStateActionInspectionContext context)
        {
            Stopwatch.Stop();
            var step = Trace.AddStep(context.ActionName, context.CurrentState.Name, Stopwatch.Elapsed);
            step.Exception = Exception;
            return Task.CompletedTask;
        }

        public Task BeforeStateExitAsync(IStateActionInspectionContext context)
        {
            Exception = null;
            Stopwatch.Reset();
            Stopwatch.Start();
            return Task.CompletedTask;
        }

        public Task AfterStateExitAsync(IStateActionInspectionContext context)
        {
            Stopwatch.Stop();
            var step = Trace.AddStep(context.ActionName, context.CurrentState.Name, Stopwatch.Elapsed);
            step.Exception = Exception;
            return Task.CompletedTask;
        }

        public Task BeforeStateFinalizeAsync(IStateActionInspectionContext context)
        {
            Exception = null;
            Stopwatch.Reset();
            Stopwatch.Start();
            return Task.CompletedTask;
        }

        public Task AfterStateFinalizeAsync(IStateActionInspectionContext context)
        {
            Stopwatch.Stop();
            var step = Trace.AddStep(context.ActionName, context.CurrentState.Name, Stopwatch.Elapsed);
            step.Exception = Exception;
            return Task.CompletedTask;
        }

        public Task BeforeStateInitializeAsync(IStateActionInspectionContext context)
        {
            Exception = null;
            Stopwatch.Reset();
            Stopwatch.Start();
            return Task.CompletedTask;
        }

        public Task AfterStateInitializeAsync(IStateActionInspectionContext context)
        {
            Stopwatch.Stop();
            var step = Trace.AddStep(context.ActionName, context.CurrentState.Name, Stopwatch.Elapsed);
            step.Exception = Exception;
            return Task.CompletedTask;
        }

        public Task BeforeStateMachineFinalizeAsync(IStateMachineActionInspectionContext context)
        {
            Exception = null;
            Stopwatch.Reset();
            Stopwatch.Start();
            return Task.CompletedTask;
        }

        public Task AfterStateMachineFinalizeAsync(IStateMachineActionInspectionContext context)
        {
            Stopwatch.Stop();
            var step = Trace.AddStep("Finalize", context.StateMachine.Id.Name, Stopwatch.Elapsed);
            step.Exception = Exception;
            return Task.CompletedTask;
        }

        public Task BeforeStateMachineInitializeAsync(IStateMachineInitializationInspectionContext context)
        {
            Exception = null;
            Stopwatch.Reset();
            Stopwatch.Start();
            return Task.CompletedTask;
        }

        public Task AfterStateMachineInitializeAsync(IStateMachineInitializationInspectionContext context)
        {
            Stopwatch.Stop();
            var step = Trace.AddStep("Initialize", context.StateMachine.Id.Name, Stopwatch.Elapsed);
            step.Exception = Exception;
            return Task.CompletedTask;
        }

        public Task BeforeTransitionEffectAsync(ITransitionInspectionContext<Event> context)
        {
            Exception = null;
            Stopwatch.Reset();
            Stopwatch.Start();
            return Task.CompletedTask;
        }

        public Task AfterTransitionEffectAsync(ITransitionInspectionContext<Event> context)
        {
            Stopwatch.Stop();
            var step = Trace.AddStep(
                "Effect",
                context.TargetState != null
                    ? $"{context.SourceState.Name} - {context.Event.Name} -> {context.TargetState.Name}"
                    : $"{context.SourceState.Name} - {context.Event.Name} ->",
                Stopwatch.Elapsed
            );
            step.Exception = Exception;
            return Task.CompletedTask;
        }

        public Task BeforeTransitionGuardAsync(IGuardInspectionContext<Event> context)
        {
            Exception = null;
            Stopwatch.Reset();
            Stopwatch.Start();
            return Task.CompletedTask;
        }

        public Task AfterTransitionGuardAsync(IGuardInspectionContext<Event> context, bool guardResult)
        {
            Stopwatch.Stop();
            var step = Trace.AddStep(
                "Guard",
                context.TargetState != null
                    ? $"{context.SourceState.Name} - {context.Event.Name} -> {context.TargetState.Name}"
                    : $"{context.SourceState.Name} - {context.Event.Name} ->",
                Stopwatch.Elapsed
            );
            step.Exception = Exception;
            return Task.CompletedTask;
        }

        public Task OnStateEntryExceptionAsync(IStateActionInspectionContext context, Exception exception)
        {
            Exception = exception;
            return Task.CompletedTask;
        }

        public Task OnStateExitExceptionAsync(IStateActionInspectionContext context, Exception exception)
        {
            Exception = exception;
            return Task.CompletedTask;
        }

        public Task OnStateFinalizeExceptionAsync(IStateActionInspectionContext context, Exception exception)
        {
            Exception = exception;
            return Task.CompletedTask;
        }

        public Task OnStateInitializeExceptionAsync(IStateActionInspectionContext context, Exception exception)
        {
            Exception = exception;
            return Task.CompletedTask;
        }

        public Task OnStateMachineFinalizationExceptionAsync(IStateMachineActionInspectionContext context, Exception exception)
        {
            Exception = exception;
            return Task.CompletedTask;
        }

        public Task OnStateMachineInitializationExceptionAsync(IStateMachineInitializationInspectionContext context, Exception exception)
        {
            Exception = exception;
            return Task.CompletedTask;
        }

        public Task OnTransitionEffectExceptionAsync(IEventInspectionContext<Event> context, Exception exception)
        {
            Exception = exception;
            return Task.CompletedTask;
        }

        public Task OnTransitionGuardExceptionAsync(IGuardInspectionContext<Event> context, Exception exception)
        {
            return Task.CompletedTask;
        }

        public Task AfterHydrateAsync(IStateMachineActionContext context)
        {
            return Task.CompletedTask;
        }

        public Task BeforeDehydrateAsync(IStateMachineActionContext context)
        {
            return Task.CompletedTask;
        }

        public Task<bool> BeforeProcessEventAsync(IEventContext<Event> context)
        {
            Trace.Event = context.Event;
            OverallStopwatch.Start();
            return Task.FromResult(true);
        }

        public Task AfterProcessEventAsync(IEventContext<Event> context)
        {
            OverallStopwatch.Stop();
            Trace.ExecutedAt = DateTime.Now;
            Trace.Duration = OverallStopwatch.Elapsed;
            Trace.Context = ((IStateflowsContextProvider)context).Context;
            Trace.BehaviorId = Trace.Context.Id;
            Repository.Traces.Push(Trace);
            return Task.CompletedTask;
        }
    }
}
