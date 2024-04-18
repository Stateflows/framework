using System;
using System.Diagnostics;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Stateflows.Common;
using Stateflows.Common.Interfaces;
using Stateflows.Common.Trace.Models;
using Stateflows.StateMachines.Context.Interfaces;

namespace Stateflows.StateMachines.Engine
{
    internal class Tracer : IStateMachinePlugin
    {
        private readonly IServiceProvider ServiceProvider;

        private BehaviorTrace Trace { get; set; }  = new BehaviorTrace();

        private Exception Exception { get; set; } = null;

        private Stopwatch Stopwatch { get; set; } = new Stopwatch();

        private Stopwatch OverallStopwatch { get; set; } = new Stopwatch();

        public Tracer(IServiceProvider serviceProvider)
        {
            ServiceProvider = serviceProvider.CreateScope().ServiceProvider;
        }

        private Task StartMeasureAsync()
        {
            if (Trace.Event != null)
            {
                Exception = null;
                Stopwatch.Reset();
            }

            return Task.CompletedTask;
        }

        private Task StopMeasureAsync(string actionName, string elementName)
        {
            if (Trace.Event != null)
            {
                Stopwatch.Stop();
                var step = Trace.AddStep(actionName, elementName, Stopwatch.Elapsed);
                step.Exceptions.Add(Exception);
            }

            return Task.CompletedTask;
        }

        private Task SetExceptionAsync(Exception exception)
        {
            if (Trace.Event != null)
            {
                Exception = exception;
            }

            return Task.CompletedTask;
        }

        public Task StopMeasurementAsync()
        {
            if (Trace.Event != null)
            {
                Exception = null;
                Stopwatch.Reset();
                Stopwatch.Start();
            }

            return Task.CompletedTask;
        }

        public Task BeforeStateEntryAsync(IStateActionContext context)
            => StartMeasureAsync();

        public Task AfterStateEntryAsync(IStateActionContext context)
            => StopMeasureAsync(context.ActionName, context.CurrentState.Name);

        public Task BeforeStateExitAsync(IStateActionContext context)
            => StopMeasurementAsync();

        public Task AfterStateExitAsync(IStateActionContext context)
            => StopMeasureAsync(context.ActionName, context.CurrentState.Name);

        public Task BeforeStateFinalizeAsync(IStateActionContext context)
            => StopMeasurementAsync();

        public Task AfterStateFinalizeAsync(IStateActionContext context)
            => StopMeasureAsync(context.ActionName, context.CurrentState.Name);

        public Task BeforeStateInitializeAsync(IStateActionContext context)
            => StopMeasurementAsync();

        public Task AfterStateInitializeAsync(IStateActionContext context)
            => StopMeasureAsync(context.ActionName, context.CurrentState.Name);

        public Task BeforeStateMachineFinalizeAsync(IStateMachineActionContext context)
            => StopMeasurementAsync();

        public Task AfterStateMachineFinalizeAsync(IStateMachineActionContext context)
            => StopMeasureAsync("Finalize", context.StateMachine.Id.Name);

        public Task BeforeStateMachineInitializeAsync(IStateMachineInitializationContext context)
            => StopMeasurementAsync();

        public Task AfterStateMachineInitializeAsync(IStateMachineInitializationContext context)
            => StopMeasureAsync("Initialize", context.StateMachine.Id.Name);

        public Task BeforeTransitionEffectAsync<TEvent>(ITransitionContext<TEvent> context)
            => StopMeasurementAsync();

        public Task AfterTransitionEffectAsync<TEvent>(ITransitionContext<TEvent> context)
        {
            if (Trace.Event != null)
            {
                Stopwatch.Stop();

                var step = Trace.AddStep(
                    "Effect",
                    context.TargetState != null
                        ? $"{context.SourceState.Name} - {context.Event.GetType().GetEventName()} -> {context.TargetState.Name}"
                        : $"{context.SourceState.Name} - {context.Event.GetType().GetEventName()} ->",
                    Stopwatch.Elapsed
                );

                step.Exceptions.Add(Exception);
            }

            return Task.CompletedTask;
        }

        public Task BeforeTransitionGuardAsync<TEvent>(IGuardContext<TEvent> context)
            => StopMeasurementAsync();

        public Task AfterTransitionGuardAsync<TEvent>(IGuardContext<TEvent> context, bool guardResult)
        {
            if (Trace.Event != null)
            {
                Stopwatch.Stop();

                var step = Trace.AddStep(
                    "Guard",
                    context.TargetState != null
                        ? $"{context.SourceState.Name} - {context.Event.GetType().GetEventName()} -> {context.TargetState.Name}"
                        : $"{context.SourceState.Name} - {context.Event.GetType().GetEventName()} ->",
                    Stopwatch.Elapsed
                );

                step.Exceptions.Add(Exception);
            }

            return Task.CompletedTask;
        }

        public Task OnStateEntryExceptionAsync(IStateActionContext context, Exception exception)
            => SetExceptionAsync(exception);

        public Task OnStateExitExceptionAsync(IStateActionContext context, Exception exception)
            => SetExceptionAsync(exception);

        public Task OnStateMachineFinalizationExceptionAsync(IStateMachineActionContext context, Exception exception)
            => SetExceptionAsync(exception);

        public Task OnStateMachineInitializationExceptionAsync(IStateMachineInitializationContext context, Exception exception)
            => SetExceptionAsync(exception);

        public Task OnTransitionGuardExceptionAsync<TEvent>(IGuardContext<TEvent> context, Exception exception)
            => SetExceptionAsync(exception);

        public Task OnTransitionEffectExceptionAsync<TEvent>(ITransitionContext<TEvent> context, Exception exception)
            => SetExceptionAsync(exception);

        public Task OnStateInitializationExceptionAsync(IStateActionContext context, Exception exception)
            => SetExceptionAsync(exception);

        public Task OnStateFinalizationExceptionAsync(IStateActionContext context, Exception exception)
            => SetExceptionAsync(exception);

        public Task AfterHydrateAsync(IStateMachineActionContext context)
            => Task.CompletedTask;

        public Task BeforeDehydrateAsync(IStateMachineActionContext context)
            => Task.CompletedTask;

        public Task<bool> BeforeProcessEventAsync<TEvent>(IEventContext<TEvent> context)
        {
            if (context.Event.GetType().GetCustomAttribute<DoNotTraceAttribute>() == null)
            {
                Trace.Event = context.Event;
                OverallStopwatch.Start();
            }

            return Task.FromResult(true);
        }

        public Task AfterProcessEventAsync<TEvent>(IEventContext<TEvent> context)
        {
            if (Trace.Event != null)
            {
                OverallStopwatch.Stop();
                Trace.ExecutedAt = DateTime.Now;
                Trace.Duration = OverallStopwatch.Elapsed;
                Trace.Context = ((IStateflowsContextProvider)context).Context;
                Trace.BehaviorId = Trace.Context.Id;
                _ = Task.Run(() => ServiceProvider.GetRequiredService<IStateflowsStorage>().SaveTraceAsync(Trace));
            }

            return Task.CompletedTask;
        }
    }
}
