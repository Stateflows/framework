using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Stateflows.Common;
using Stateflows.Common.Engine;
using Stateflows.Common.Interfaces;
using Stateflows.StateMachines.Context.Classes;
using Stateflows.StateMachines.Context.Interfaces;
using Stateflows.StateMachines.Extensions;

namespace Stateflows.StateMachines.Engine
{
    internal class Notifications(IStateflowsValueStorage valueStorage) : StateMachinePlugin
    {
        public override async Task OnStateEntryAsync(IStateActionContext context, Func<Task> next)
        {
            await next();
            var stateflowsContext = ((BaseContext)context).Context.Context;
            Trace.WriteLine($"⦗→s⦘ State Machine '{stateflowsContext.Id.Name}:{stateflowsContext.Id.Instance}': entered '{context.State.Name}'");
        }

        public override async Task OnStateExitAsync(IStateActionContext context, Func<Task> next)
        {
            await next();
            var stateflowsContext = ((BaseContext)context).Context.Context;
            Trace.WriteLine($"⦗→s⦘ State Machine '{stateflowsContext.Id.Name}:{stateflowsContext.Id.Instance}': exited '{context.State.Name}'");
        }

        public override async Task OnStateInitializeAsync(IStateActionContext context, Func<Task> next)
        {
            await next();
            var stateflowsContext = ((BaseContext)context).Context.Context;
            Trace.WriteLine($"⦗→s⦘ State Machine '{stateflowsContext.Id.Name}:{stateflowsContext.Id.Instance}': initialized '{context.State.Name}'");
        }

        public override async Task OnStateFinalizeAsync(IStateActionContext context, Func<Task> next)
        {
            await next();
            var stateflowsContext = ((BaseContext)context).Context.Context;
            Trace.WriteLine($"⦗→s⦘ State Machine '{stateflowsContext.Id.Name}:{stateflowsContext.Id.Instance}': finalized '{context.State.Name}'");
        }

        public override async Task OnStateMachineInitializeAsync(IStateMachineInitializationContext context, bool implicitInitialization, Func<Task<bool>> next)
        {
            var result = await next();
            var stateflowsContext = ((BaseContext)context).Context.Context;
            Trace.WriteLine($"⦗→s⦘ State Machine '{stateflowsContext.Id.Name}:{stateflowsContext.Id.Instance}': {(result ? "" : "not ")}initialized{(implicitInitialization ? " implicitly" : "")}");

            var executor = context.Behavior.GetExecutor();
            var notification = new BehaviorInfo()
            {
                Id = executor.Context.Id,
                BehaviorStatus = BehaviorStatus.Initialized,
                ExpectedEvents = executor.GetExpectedEventNames()
            };

            context.Behavior.Publish(notification);
        }

        public override async Task OnStateMachineFinalizeAsync(IStateMachineActionContext context, Func<Task> next)
        {
            await next();
            var stateflowsContext = ((BaseContext)context).Context.Context;

            context.Behavior.GetExecutor().AddPendingTask(valueStorage.RemoveAsync(stateflowsContext.Id, CommonValues.ForceFinalizeKey));

            Trace.WriteLine($"⦗→s⦘ State Machine '{stateflowsContext.Id.Name}:{stateflowsContext.Id.Instance}': finalized");

            var notification = new BehaviorInfo()
            {
                Id = context.Behavior.Id,
                BehaviorStatus = BehaviorStatus.Finalized
            };

            context.Behavior.Publish(notification);
        }

        public override async Task OnTransitionEffectAsync<TEvent>(ITransitionContext<TEvent> context, Func<Task> next)
        {
            var stateflowsContext = ((BaseContext)context).Context.Context;
            var eventName = Event.GetName(context.Event.GetType());
            if (context.Target != null)
            {
                Trace.WriteLine(string.IsNullOrEmpty(eventName)
                    ? $"⦗→s⦘ State Machine '{stateflowsContext.Id.Name}:{stateflowsContext.Id.Instance}': default transition from '{context.Source.Name}' to '{context.Target.Name}'"
                    : $"⦗→s⦘ State Machine '{stateflowsContext.Id.Name}:{stateflowsContext.Id.Instance}': event '{eventName}' triggered transition from '{context.Source.Name}' to '{context.Target.Name}'");
            }
            else
            {
                Trace.WriteLine($"⦗→s⦘ State Machine '{stateflowsContext.Id.Name}:{stateflowsContext.Id.Instance}': event '{eventName}' triggered internal transition in '{context.Source.Name}'");
            }
            await next();
        }

        public override async Task OnTransitionGuardAsync<TEvent>(ITransitionContext<TEvent> context, Func<Task<bool>> next)
        {
            var result = await next();
            var stateflowsContext = ((BaseContext)context).Context.Context;
            if (result)
            {
                return;
            }

            var eventName = Event.GetName(context.Event.GetType());

            var transitionContext = (TransitionContext<TEvent>)context;
            var guardDelegations = context.Headers.Values.OfType<TransitionGuardDelegation>();
            if (guardDelegations.Any(g => g.EdgeIdentifier == transitionContext.Edge.Identifier))
            {
                if (context.Target != null)
                {
                    Trace.WriteLine(string.IsNullOrEmpty(eventName)
                        ? $"⦗→s⦘ State Machine '{stateflowsContext.Id.Name}:{stateflowsContext.Id.Instance}': delegated guard on default transition from '{context.Source.Name}' to '{context.Target.Name}'"
                        : $"⦗→s⦘ State Machine '{stateflowsContext.Id.Name}:{stateflowsContext.Id.Instance}': delegated guard on transition from '{context.Source.Name}' to '{context.Target.Name}' triggered by event '{eventName}'");
                }
                else
                {
                    Trace.WriteLine($"⦗→s⦘ State Machine '{stateflowsContext.Id.Name}:{stateflowsContext.Id.Instance}': delegated guard on internal transition in '{context.Source.Name}' triggered by event '{eventName}'");
                }
            }
            else
            {
                if (context.Target != null)
                {
                    Trace.WriteLine(string.IsNullOrEmpty(eventName)
                        ? $"⦗→s⦘ State Machine '{stateflowsContext.Id.Name}:{stateflowsContext.Id.Instance}': guard stopped default transition from '{context.Source.Name}' to '{context.Target.Name}'"
                        : $"⦗→s⦘ State Machine '{stateflowsContext.Id.Name}:{stateflowsContext.Id.Instance}': guard stopped event '{eventName}' from triggering transition from '{context.Source.Name}' to '{context.Target.Name}'");
                }
                else
                {
                    Trace.WriteLine($"⦗→s⦘ State Machine '{stateflowsContext.Id.Name}:{stateflowsContext.Id.Instance}': guard stopped event '{eventName}' from triggering internal transition in '{context.Source.Name}'");
                }
            }
        }

        public override async Task<EventStatus> ProcessEventAsync<TEvent>(IEventContext<TEvent> context, Func<Task<EventStatus>> next)
        {
            Before(context);

            var status = EventStatus.Failed;
            try
            {
                return status = await next();
            }
            finally
            {
                After(context, status);
            }

            void Before(IEventContext<TEvent> context)
            {
                var stateflowsContext = ((BaseContext)context).Context.Context;
                Trace.WriteLine($"⦗→s⦘ State Machine '{stateflowsContext.Id.Name}:{stateflowsContext.Id.Instance}': received event '{Event.GetName(context.Event.GetType())}', processing");

            }

            void After(IEventContext<TEvent> context, EventStatus eventStatus)
            {
                var stateflowsContext = ((BaseContext)context).Context.Context;
                Trace.WriteLine($"⦗→s⦘ State Machine '{stateflowsContext.Id.Name}:{stateflowsContext.Id.Instance}': processed event '{Event.GetName(context.Event.GetType())}' with result '{eventStatus}'");

                var executor = context.Behavior.GetExecutor();
                if (!executor.StateHasChanged)
                {
                    return;
                }

                var notification = new StateMachineInfo()
                {
                    Id = executor.Context.Id,
                    BehaviorStatus = executor.BehaviorStatus,
                    CurrentStates = executor.GetStatesTree(),
                    ExpectedEvents = executor.GetExpectedEventNames(),
                };

                context.Behavior.Publish(notification);
            }
        }

        public override Task<bool> OnStateMachineInitializationExceptionAsync(IStateMachineInitializationContext context, Exception exception)
        {
            var stateflowsContext = ((BaseContext)context).Context.Context;
            Trace.WriteLine($"⦗→s⦘ State Machine '{stateflowsContext.Id.Name}:{stateflowsContext.Id.Instance}': unhandled exception '{exception.GetType().Name}' thrown with message '{exception.Message}' on State Machine initialization");

            return Task.FromResult(false);
        }

        public override Task<bool> OnStateMachineFinalizationExceptionAsync(IStateMachineActionContext context, Exception exception)
        {
            var stateflowsContext = ((BaseContext)context).Context.Context;
            Trace.WriteLine($"⦗→s⦘ State Machine '{stateflowsContext.Id.Name}:{stateflowsContext.Id.Instance}': unhandled exception '{exception.GetType().Name}' thrown with message '{exception.Message}' on State Machine finalization");

            return Task.FromResult(false);
        }

        public override Task<bool> OnTransitionGuardExceptionAsync<TEvent>(ITransitionContext<TEvent> context, Exception exception)
        {
            var stateflowsContext = ((BaseContext)context).Context.Context;
            Trace.WriteLine($"⦗→s⦘ State Machine '{stateflowsContext.Id.Name}:{stateflowsContext.Id.Instance}': unhandled exception '{exception.GetType().Name}' thrown with message '{exception.Message}' on guard of transition from state '{context.Source.Name}'");

            return Task.FromResult(false);
        }

        public override Task<bool> OnTransitionEffectExceptionAsync<TEvent>(ITransitionContext<TEvent> context, Exception exception)
        {
            var stateflowsContext = ((BaseContext)context).Context.Context;
            Trace.WriteLine($"⦗→s⦘ State Machine '{stateflowsContext.Id.Name}:{stateflowsContext.Id.Instance}': unhandled exception '{exception.GetType().Name}' thrown with message '{exception.Message}' on effect of transition from state '{context.Source.Name}'");

            return Task.FromResult(false);
        }

        public override Task<bool> OnStateInitializationExceptionAsync(IStateActionContext context, Exception exception)
        {
            var stateflowsContext = ((BaseContext)context).Context.Context;
            Trace.WriteLine($"⦗→s⦘ State Machine '{stateflowsContext.Id.Name}:{stateflowsContext.Id.Instance}': unhandled exception '{exception.GetType().Name}' thrown with message '{exception.Message}' on state '{context.State.Name}' initialization");

            return Task.FromResult(false);
        }

        public override Task<bool> OnStateFinalizationExceptionAsync(IStateActionContext context, Exception exception)
        {
            var stateflowsContext = ((BaseContext)context).Context.Context;
            Trace.WriteLine($"⦗→s⦘ State Machine '{stateflowsContext.Id.Name}:{stateflowsContext.Id.Instance}': unhandled exception '{exception.GetType().Name}' thrown with message '{exception.Message}' on state '{context.State.Name}' finalization");

            return Task.FromResult(false);
        }

        public override Task<bool> OnStateEntryExceptionAsync(IStateActionContext context, Exception exception)
        {
            var stateflowsContext = ((BaseContext)context).Context.Context;
            Trace.WriteLine($"⦗→s⦘ State Machine '{stateflowsContext.Id.Name}:{stateflowsContext.Id.Instance}': unhandled exception '{exception.GetType().Name}' thrown with message '{exception.Message}' on entry to state '{context.State.Name}'");

            return Task.FromResult(false);
        }

        public override Task<bool> OnStateExitExceptionAsync(IStateActionContext context, Exception exception)
        {
            var stateflowsContext = ((BaseContext)context).Context.Context;
            Trace.WriteLine($"⦗→s⦘ State Machine '{stateflowsContext.Id.Name}:{stateflowsContext.Id.Instance}': unhandled exception '{exception.GetType().Name}' thrown with message '{exception.Message}' on exit from state '{context.State.Name}'");

            return Task.FromResult(false);
        }
    }
}
