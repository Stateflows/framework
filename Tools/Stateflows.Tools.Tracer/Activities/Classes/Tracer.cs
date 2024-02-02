using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Stateflows.Common;
using Stateflows.Common.Interfaces;
using Stateflows.Tools.Tracer.Classes;
using Stateflows.Activities;
using Stateflows.Activities.Context.Interfaces;
using Stateflows.Activities.Inspection.Interfaces;
using Stateflows.Tools.Tracer.Activities.Models;

namespace Stateflows.Tools.Tracer.Activities.Classes
{
    internal class Tracer : IActivityInspector, IActivityInterceptor
    {
        private readonly TraceRepository Repository;

        private Models.Trace Trace { get; set; }  = new Models.Trace();
        
        private Exception? Exception { get; set; }

        private Stopwatch Stopwatch { get; set; } = new Stopwatch();

        private Stopwatch OverallStopwatch { get; set; } = new Stopwatch();

        public Tracer(TraceRepository repository)
        {
            Repository = repository;
        }

        public Task BeforeActivityInitializationAsync(IActivityInitializationInspectionContext context)
        {
            Exception = null;
            Stopwatch.Reset();
            return Task.CompletedTask;
        }

        public Task AfterActivityInitializationAsync(IActivityInitializationInspectionContext context)
        {
            Stopwatch.Stop();
            var step = Trace.EntryStep.AddStep("ActivityInitialization", context.Activity.Id.Name, Stopwatch.Elapsed, Array.Empty<Token>());
            step.Exception = Exception;
            return Task.CompletedTask;
        }

        public Task BeforeActivityFinalizationAsync(IActivityFinalizationInspectionContext context)
        {
            Exception = null;
            Stopwatch.Reset();
            return Task.CompletedTask;
        }

        public Task AfterActivityFinalizationAsync(IActivityFinalizationInspectionContext context)
        {
            Stopwatch.Stop();
            var step = Trace.EntryStep.AddStep("ActivityFinalization", context.Activity.Id.Name, Stopwatch.Elapsed, Array.Empty<Token>());
            step.Exception = Exception;
            return Task.CompletedTask;
        }

        public Task BeforeNodeInitializationAsync(IActivityNodeInspectionContext context)
        {
            Exception = null;
            Stopwatch.Reset();
            return Task.CompletedTask;
        }

        public Task AfterNodeInitializationAsync(IActivityNodeInspectionContext context)
        {
            Stopwatch.Stop();
            var step = Trace.EntryStep.AddStep("NodeInitialization", context.CurrentNode.NodeName, Stopwatch.Elapsed, Array.Empty<Token>());
            step.Exception = Exception;
            return Task.CompletedTask;
        }

        public Task BeforeNodeFinalizationAsync(IActivityNodeInspectionContext context)
        {
            Exception = null;
            Stopwatch.Reset();
            return Task.CompletedTask;
        }

        public Task AfterNodeFinalizationAsync(IActivityNodeInspectionContext context)
        {
            Stopwatch.Stop();
            var step = Trace.EntryStep.AddStep("NodeFinalization", context.CurrentNode.NodeName, Stopwatch.Elapsed, Array.Empty<Token>());
            step.Exception = Exception;
            return Task.CompletedTask;
        }

        public Task BeforeNodeExecuteAsync(IActivityNodeInspectionContext context)
        {
            Exception = null;
            Stopwatch.Reset();
            return Task.CompletedTask;
        }

        public Task AfterNodeExecuteAsync(IActivityNodeInspectionContext context)
        {
            Stopwatch.Stop();
            var step = Trace.EntryStep.AddStep(
                "Execute",
                context.CurrentNode.NodeName,
                Stopwatch.Elapsed,
                (context as IActionContext<Token>)?.Input ?? Array.Empty<Token>()
            );
            step.Exception = Exception;
            return Task.CompletedTask;
        }

        public Task BeforeFlowGuardAsync(IGuardInspectionContext context)
        {
            Exception = null;
            Stopwatch.Reset();
            return Task.CompletedTask;
        }

        public Task AfterFlowGuardAsync(IGuardInspectionContext context, bool guardResult)
        {
            Stopwatch.Stop();
            var step = Trace.EntryStep.AddStep("Guard", $"{context.SourceNode.NodeName}-{context.Token.Name}->{context.TargetNode.NodeName}", Stopwatch.Elapsed, Array.Empty<Token>());
            step.Exception = Exception;
            return Task.CompletedTask;
        }

        public Task BeforeFlowTransformationAsync(ITransformationInspectionContext context)
        {
            Exception = null;
            Stopwatch.Reset();
            return Task.CompletedTask;
        }

        public Task AfterFlowTransformationAsync(ITransformationInspectionContext context)
        {
            Stopwatch.Stop();
            var step = Trace.EntryStep.AddStep("Transformation", $"{context.SourceNode.NodeName}-{context.Token.Name}->{context.TargetNode.NodeName}", Stopwatch.Elapsed, Array.Empty<Token>());
            step.Exception = Exception;
            return Task.CompletedTask;
        }

        public Task OnActivityInitializationExceptionAsync(IActivityInitializationInspectionContext context, Exception exception)
        {
            Exception = exception;
            return Task.CompletedTask;
        }

        public Task OnActivityFinalizationExceptionAsync(IActivityFinalizationInspectionContext context, Exception exception)
        {
            Exception = exception;
            return Task.CompletedTask;
        }

        public Task OnNodeInitializationExceptionAsync(IActivityNodeInspectionContext context, Exception exception)
        {
            Exception = exception;
            return Task.CompletedTask;
        }

        public Task OnNodeFinalizationExceptionAsync(IActivityNodeInspectionContext context, Exception exception)
        {
            Exception = exception;
            return Task.CompletedTask;
        }

        public Task OnNodeExecutionExceptionAsync(IActivityNodeInspectionContext context, Exception exception)
        {
            Exception = exception;
            return Task.CompletedTask;
        }

        public Task OnFlowGuardExceptionAsync(IGuardInspectionContext context, Exception exception)
        {
            Exception = exception;
            return Task.CompletedTask;
        }

        public Task OnFlowTransformationExceptionAsync(ITransformationInspectionContext context, Exception exception)
        {
            Exception = exception;
            return Task.CompletedTask;
        }

        public Task AfterHydrateAsync(IActivityActionContext context)
        {
            return Task.CompletedTask;
        }

        public Task BeforeDehydrateAsync(IActivityActionContext context)
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
