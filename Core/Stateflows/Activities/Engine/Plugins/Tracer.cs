using System;
using System.Threading;
using System.Reflection;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Stateflows.Common;
using Stateflows.Common.Interfaces;
using Stateflows.Common.Trace.Models;
using Stateflows.Activities.Context.Interfaces;

namespace Stateflows.Activities.Engine
{
    internal class Tracer : IActivityPlugin
    {
        private readonly IServiceProvider ServiceProvider;

        private BehaviorTrace Trace { get; set; } =  new BehaviorTrace();

        private AsyncLocal<Exception> ExceptionHolder { get; set; } = new AsyncLocal<Exception>();

        private Exception Exception
        {
            get => ExceptionHolder.Value;
            set => ExceptionHolder.Value = value;
        }

        private AsyncLocal<Stopwatch> StopwatchHolder { get; set; } = new AsyncLocal<Stopwatch>();

        private Stopwatch Stopwatch => StopwatchHolder.Value ?? (StopwatchHolder.Value = new Stopwatch());

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

                Trace
                    .AddStep(actionName, elementName, Stopwatch.Elapsed)
                    .Exceptions
                    .Add(Exception);
            }

            return Task.CompletedTask;
        }

        private Task SetExceptionAsync(Exception exception)
        {
            Exception = exception;
            return Task.CompletedTask;
        }

        public Task BeforeActivityInitializeAsync(IActivityInitializationContext context)
            => StartMeasureAsync();

        public Task AfterActivityInitializeAsync(IActivityInitializationContext context)
                => StopMeasureAsync("ActivityInitialization", context.Activity.Id.Name);

        public Task BeforeActivityFinalizeAsync(IActivityFinalizationContext context)
            => StartMeasureAsync();

        public Task AfterActivityFinalizeAsync(IActivityFinalizationContext context)
            => StopMeasureAsync("ActivityFinalization", context.Activity.Id.Name);

        public Task BeforeNodeInitializeAsync(IActivityNodeContext context)
            => StartMeasureAsync();

        public Task AfterNodeInitializeAsync(IActivityNodeContext context)
        => StopMeasureAsync("NodeInitialization", context.CurrentNode.NodeName);

        public Task BeforeNodeFinalizeAsync(IActivityNodeContext context)
            => StartMeasureAsync();

        public Task AfterNodeFinalizeAsync(IActivityNodeContext context)
        => StopMeasureAsync("NodeFinalization", context.CurrentNode.NodeName);

        public Task BeforeNodeExecuteAsync(IActivityNodeContext context)
            => StartMeasureAsync();

        public Task AfterNodeExecuteAsync(IActivityNodeContext context)
        {
            if (Trace.Event != null)
            {
                Stopwatch.Stop();

                Trace
                    .AddStep(
                        "Execute",
                        context.CurrentNode.NodeName,
                        Stopwatch.Elapsed
                        //(context as IActionContext<Token>)?.Input ?? Array.Empty<Token>()
                    )
                    .Exceptions
                    .Add(Exception);
            }

            return Task.CompletedTask;
        }

        public Task BeforeFlowGuardAsync<TToken>(IGuardContext<TToken> context)
            => StartMeasureAsync();

        public Task AfterFlowGuardAsync<TToken>(IGuardContext<TToken> context, bool guardResult)
            => StopMeasureAsync("Guard", $"{context.SourceNode.NodeName}-{context.Token.GetType().GetTokenName()}->{context.TargetNode.NodeName}");

        public Task BeforeFlowTransformAsync<TToken>(ITransformationContext<TToken> context)
            => StartMeasureAsync();

        public Task AfterFlowTransformAsync<TToken>(ITransformationContext<TToken> context)
            => StopMeasureAsync("Transformation", $"{context.SourceNode.NodeName}-{context.Token.GetType().GetTokenName()}->{context.TargetNode.NodeName}");

        public Task OnActivityInitializationExceptionAsync(IActivityInitializationContext context, Exception exception)
            => SetExceptionAsync(exception);

        public Task OnActivityFinalizationExceptionAsync(IActivityFinalizationContext context, Exception exception)
            => SetExceptionAsync(exception);

        public Task OnNodeInitializationExceptionAsync(IActivityNodeContext context, Exception exception)
            => SetExceptionAsync(exception);

        public Task OnNodeFinalizationExceptionAsync(IActivityNodeContext context, Exception exception)
            => SetExceptionAsync(exception);

        public Task OnNodeExecutionExceptionAsync(IActivityNodeContext context, Exception exception)
            => SetExceptionAsync(exception);

        public Task OnFlowGuardExceptionAsync<TToken>(IGuardContext<TToken> context, Exception exception)
            => SetExceptionAsync(exception);

        public Task OnFlowTransformationExceptionAsync<TToken>(ITransformationContext<TToken> context, Exception exception)
            => SetExceptionAsync(exception);

        public Task<bool> BeforeProcessEventAsync(IEventContext<Event> context)
        {
            var eventType = context.Event.GetType();
            var attribute = eventType.GetCustomAttribute<DoNotTraceAttribute>();
            if (attribute == null)
            {
                Trace.Event = context.Event;
                OverallStopwatch.Start();
            }

            return Task.FromResult(true);
        }

        public Task AfterProcessEventAsync(IEventContext<Event> context)
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
