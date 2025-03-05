using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Stateflows.Common;
using Stateflows.StateMachines.Extensions;
using Stateflows.StateMachines.Context.Interfaces;

namespace Stateflows.StateMachines.Engine
{
    internal class Notifications : StateMachinePlugin
    {
        private bool behaviorInitialized = false;
        
        private bool behaviorFinalized = false;
        
        public override Task AfterStateEntryAsync(IStateActionContext context)
        {
            Trace.WriteLine($"⦗→s⦘ State Machine '{context.Behavior.Id.Name}:{context.Behavior.Id.Instance}': entered '{context.CurrentState.Name}'");

            return Task.CompletedTask;
        }

        public override Task AfterStateExitAsync(IStateActionContext context)
        {
            Trace.WriteLine($"⦗→s⦘ State Machine '{context.Behavior.Id.Name}:{context.Behavior.Id.Instance}': exited '{context.CurrentState.Name}'");

            return Task.CompletedTask;
        }

        public override Task AfterStateInitializeAsync(IStateActionContext context)
        {
            Trace.WriteLine($"⦗→s⦘ State Machine '{context.Behavior.Id.Name}:{context.Behavior.Id.Instance}': initialized '{context.CurrentState.Name}'");

            return Task.CompletedTask;
        }

        public override Task AfterStateFinalizeAsync(IStateActionContext context)
        {
            Trace.WriteLine($"⦗→s⦘ State Machine '{context.Behavior.Id.Name}:{context.Behavior.Id.Instance}': finalized '{context.CurrentState.Name}'");

            return Task.CompletedTask;
        }

        public override Task AfterStateMachineInitializeAsync(IStateMachineInitializationContext context, bool initialized)
        {
            Trace.WriteLine($"⦗→s⦘ State Machine '{context.Behavior.Id.Name}:{context.Behavior.Id.Instance}': {(initialized ? "" : "not ")}initialized");
            
            behaviorInitialized = true;
       
            return Task.CompletedTask;
        }

        public override Task AfterStateMachineFinalizeAsync(IStateMachineActionContext context)
        {
            Trace.WriteLine($"⦗→s⦘ State Machine '{context.Behavior.Id.Name}:{context.Behavior.Id.Instance}': finalized");

            behaviorFinalized = true;

            return Task.CompletedTask;
        }

        public override Task BeforeTransitionEffectAsync<TEvent>(ITransitionContext<TEvent> context)
        {
            var eventName = Event.GetName(context.Event.GetType());
            if (context.Target != null)
            {
                Trace.WriteLine(string.IsNullOrEmpty(eventName)
                    ? $"⦗→s⦘ State Machine '{context.Behavior.Id.Name}:{context.Behavior.Id.Instance}': default transition from '{context.Source.Name}' to '{context.Target.Name}'"
                    : $"⦗→s⦘ State Machine '{context.Behavior.Id.Name}:{context.Behavior.Id.Instance}': event '{eventName}' triggered transition from '{context.Source.Name}' to '{context.Target.Name}'");
            }
            else
            {
                Trace.WriteLine($"⦗→s⦘ State Machine '{context.Behavior.Id.Name}:{context.Behavior.Id.Instance}': event '{eventName}' triggered internal transition in '{context.Source.Name}'");
            }
            
            return Task.CompletedTask;
        }

        public override Task AfterTransitionGuardAsync<TEvent>(ITransitionContext<TEvent> context, bool guardResult)
        {
            if (guardResult) return Task.CompletedTask;
            var eventName = Event.GetName(context.Event.GetType());
            if (context.Target != null)
            {
                Trace.WriteLine(string.IsNullOrEmpty(eventName)
                    ? $"⦗→s⦘ State Machine '{context.Behavior.Id.Name}:{context.Behavior.Id.Instance}': guard stopped default transition from '{context.Source.Name}' to '{context.Target.Name}'"
                    : $"⦗→s⦘ State Machine '{context.Behavior.Id.Name}:{context.Behavior.Id.Instance}': guard stopped event '{eventName}' from triggering transition from '{context.Source.Name}' to '{context.Target.Name}'");
            }
            else
            {
                Trace.WriteLine($"⦗→s⦘ State Machine '{context.Behavior.Id.Name}:{context.Behavior.Id.Instance}': guard stopped event '{eventName}' from triggering internal transition in '{context.Source.Name}'");
            }

            return Task.CompletedTask;
        }
        
        // public override Task<bool> BeforeProcessEventAsync<TEvent>(IEventActionContext<TEvent> context)
        // {
        //     Trace.WriteLine($"⦗→s⦘ State Machine '{context.Behavior.Id.Name}:{context.Behavior.Id.Instance}': received event '{Event.GetName(context.Event.GetType())}', trying to process it");
        //
        //     return Task.FromResult(true);
        // }

        // public override Task AfterProcessEventAsync<TEvent>(IEventActionContext<TEvent> context, EventStatus eventStatus)
        // {
        //     // Trace.WriteLine($"⦗→s⦘ State Machine '{context.Behavior.Id.Name}:{context.Behavior.Id.Instance}': processed event '{Event.GetName(context.Event.GetType())}'");
        //     //
        //     // var executor = context.Behavior.GetExecutor();
        //     //
        //     // if (behaviorInitialized)
        //     // {
        //     //     var behaviorInfo = new BehaviorInfo()
        //     //     {
        //     //         BehaviorStatus = BehaviorStatus.Initialized,
        //     //         ExpectedEvents = executor.GetExpectedEventNames()
        //     //     };
        //     //
        //     //     context.Behavior.Publish(behaviorInfo);
        //     //
        //     //     behaviorInitialized = false;
        //     // }
        //     //
        //     // if (behaviorFinalized)
        //     // {
        //     //     var behaviorInfo = new BehaviorInfo()
        //     //     {
        //     //         BehaviorStatus = BehaviorStatus.Finalized
        //     //     };
        //     //
        //     //     context.Behavior.Publish(behaviorInfo);
        //     // }
        //     //
        //     // if (!executor.StateHasChanged) return Task.CompletedTask;
        //     //
        //     // var stateMachineInfo = new StateMachineInfo()
        //     // {
        //     //     BehaviorStatus = executor.BehaviorStatus,
        //     //     StatesTree = executor.GetStateTree(),
        //     //     ExpectedEvents = executor.GetExpectedEventNames(),
        //     // };
        //     //
        //     // context.Behavior.Publish(stateMachineInfo);
        //
        //     return Task.CompletedTask;
        // }

        public override async Task<EventStatus> ProcessEventAsync<TEvent>(IEventActionContext<TEvent> context, Func<IEventActionContext<TEvent>, Task<EventStatus>> next)
        {
            Trace.WriteLine($"⦗→s⦘ State Machine '{context.Behavior.Id.Name}:{context.Behavior.Id.Instance}': received event '{Event.GetName(context.Event.GetType())}', trying to process it");
            
            var result = await next(context);

            Trace.WriteLine($"⦗→s⦘ State Machine '{context.Behavior.Id.Name}:{context.Behavior.Id.Instance}': processed event '{Event.GetName(context.Event.GetType())}'");

            var executor = context.Behavior.GetExecutor();

            if (behaviorInitialized)
            {
                var behaviorInfo = new BehaviorInfo()
                {
                    BehaviorStatus = BehaviorStatus.Initialized,
                    ExpectedEvents = executor.GetExpectedEventNames()
                };

                context.Behavior.Publish(behaviorInfo);

                behaviorInitialized = false;
            }

            if (behaviorFinalized)
            {
                var behaviorInfo = new BehaviorInfo()
                {
                    BehaviorStatus = BehaviorStatus.Finalized
                };

                context.Behavior.Publish(behaviorInfo);
            }

            if (executor.StateHasChanged)
            {
                var stateMachineInfo = new StateMachineInfo()
                {
                    BehaviorStatus = executor.BehaviorStatus,
                    StatesTree = executor.GetStateTree(),
                    ExpectedEvents = executor.GetExpectedEventNames(),
                };

                context.Behavior.Publish(stateMachineInfo);
            }

            return result;
        }

        public override Task<bool> OnStateMachineInitializationExceptionAsync(IStateMachineInitializationContext context, Exception exception)
        {
            Trace.WriteLine($"⦗→s⦘ State Machine '{context.Behavior.Id.Name}:{context.Behavior.Id.Instance}': unhandled exception '{exception.GetType().Name}' with message '{exception.Message}' on State Machine initialization");

            return Task.FromResult(false);
        }

        public override Task<bool> OnStateMachineFinalizationExceptionAsync(IStateMachineActionContext context, Exception exception)
        {
            Trace.WriteLine($"⦗→s⦘ State Machine '{context.Behavior.Id.Name}:{context.Behavior.Id.Instance}': unhandled exception '{exception.GetType().Name}' with message '{exception.Message}' on State Machine finalization");

            return Task.FromResult(false);
        }

        public override Task<bool> OnTransitionGuardExceptionAsync<TEvent>(ITransitionContext<TEvent> context, Exception exception)
        {
            Trace.WriteLine($"⦗→s⦘ State Machine '{context.Behavior.Id.Name}:{context.Behavior.Id.Instance}': unhandled exception '{exception.GetType().Name}' with message '{exception.Message}' on transition guard");

            return Task.FromResult(false);
        }

        public override Task<bool> OnTransitionEffectExceptionAsync<TEvent>(ITransitionContext<TEvent> context, Exception exception)
        {
            Trace.WriteLine($"⦗→s⦘ State Machine '{context.Behavior.Id.Name}:{context.Behavior.Id.Instance}': unhandled exception '{exception.GetType().Name}' with message '{exception.Message}' on transition effect");

            return Task.FromResult(false);
        }

        public override Task<bool> OnStateInitializationExceptionAsync(IStateActionContext context, Exception exception)
        {
            Trace.WriteLine($"⦗→s⦘ State Machine '{context.Behavior.Id.Name}:{context.Behavior.Id.Instance}': unhandled exception '{exception.GetType().Name}' with message '{exception.Message}' on initialization");

            return Task.FromResult(false);
        }

        public override Task<bool> OnStateFinalizationExceptionAsync(IStateActionContext context, Exception exception)
        {
            Trace.WriteLine($"⦗→s⦘ State Machine '{context.Behavior.Id.Name}:{context.Behavior.Id.Instance}': unhandled exception '{exception.GetType().Name}' with message '{exception.Message}' on finalization");

            return Task.FromResult(false);
        }

        public override Task<bool> OnStateEntryExceptionAsync(IStateActionContext context, Exception exception)
        {
            Trace.WriteLine($"⦗→s⦘ State Machine '{context.Behavior.Id.Name}:{context.Behavior.Id.Instance}': unhandled exception '{exception.GetType().Name}' with message '{exception.Message}' on entry");

            return Task.FromResult(false);
        }

        public override Task<bool> OnStateExitExceptionAsync(IStateActionContext context, Exception exception)
        {
            Trace.WriteLine($"⦗→s⦘ State Machine '{context.Behavior.Id.Name}:{context.Behavior.Id.Instance}': unhandled exception '{exception.GetType().Name}' with message '{exception.Message}' on exit");

            return Task.FromResult(false);
        }
    }
}
