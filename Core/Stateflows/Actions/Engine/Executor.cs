using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Stateflows.Common;
using Stateflows.Common.Context;
using Stateflows.Common.Interfaces;
using Stateflows.Actions.Context.Classes;
using Stateflows.Actions.Models;
using Stateflows.Actions.Registration;
using Stateflows.Activities;
using Stateflows.Common.Utilities;
using Stateflows.Common.Engine;
using Stateflows.Common.Extensions;
using Stateflows.Common.Classes;
using Stateflows.StateMachines.Models;

namespace Stateflows.Actions.Engine
{
    internal class Executor : IStateflowsExecutor
    {
        private INotificationsHub Hub;
        
        public Executor(ActionsRegister register, StateflowsContext stateflowsContext, IServiceProvider serviceProvider, ActionModel actionModelModel)
        {
            Register = register;
            StateflowsContext = stateflowsContext;
            ServiceProvider = serviceProvider;
            ActionModel = actionModelModel;
            Hub = ServiceProvider.GetRequiredService<INotificationsHub>();
            Logger = serviceProvider.GetService<ILogger<Executor>>();
        }

        private readonly ILogger<Executor> Logger;
        private readonly StateflowsContext StateflowsContext;
        internal readonly IServiceProvider ServiceProvider;
        internal readonly ActionModel ActionModel;
        internal readonly ActionsRegister Register;

        private Inspector? inspector = null;
        
        private readonly PendingTasks pendingTasks = PendingTasksScope.RequireCurrent();

        public void AddPendingTask(Task task)
            => pendingTasks.Add(task);

        public async Task<Inspector> GetInspectorAsync()
        {
            if (inspector == null)
            {
                inspector = new Inspector(this, Logger);
                await inspector.BuildAsync();
            }

            return inspector;
        }
        
        public async Task HydrateAsync(EventHolder eventHolder)
        {
            // var inspector = await GetInspectorAsync();
            
            await using var serviceScope = new StateflowsServiceScope(ServiceProvider.CreateAsyncScope());
            using var context = new ActionDelegateContext(StateflowsContext, this, eventHolder, serviceScope.ServiceProvider);
            // inspector.AfterHydrate(context);
        }

        public async Task DehydrateAsync(EventHolder eventHolder)
        {
            // var inspector = await GetInspectorAsync();

            await using var scope = new StateflowsServiceScope(ServiceProvider.CreateAsyncScope());
            using var context = new ActionDelegateContext(StateflowsContext, this, eventHolder, scope.ServiceProvider);
            // inspector.BeforeDehydrate(context);
        }
        
        public async Task<EventStatus> DoProcessAsync<TEvent>(EventHolder<TEvent> eventHolder)
        {
            var result = EventStatus.NotConsumed;

            var inspector = await GetInspectorAsync();

            Trace.WriteLine($"⦗→s⦘ {StateflowsContext.Id.Type} '{StateflowsContext.Id.Name}:{StateflowsContext.Id.Instance}': received event '{Event.GetName(eventHolder.PayloadType)}', processing");

            using var eventContext = new EventContext<TEvent>(StateflowsContext, this, eventHolder, ServiceProvider);

            if (eventHolder.Payload is TokensInputEvent tokensInput)
            {
                using var context = new ActionDelegateContext(StateflowsContext, this, eventHolder, ServiceProvider, tokensInput.Tokens);
                try
                {
                    InputTokens.TokensHolder.Value = context.InputTokens.ToList();

                    await InvokeActionAsync(context);

                    InputTokens.TokensHolder.Value = null;

                    Trace.WriteLine($"⦗→s⦘ {StateflowsContext.Id.Type} '{StateflowsContext.Id.Name}:{StateflowsContext.Id.Instance}': executed");

                    var tokensOutput = new TokensOutput()
                    {
                        Tokens = context.OutputTokens.ToList()
                    };

                    if (eventHolder.Payload is IRequest<TokensOutput> request)
                    {
                        ResponseHolder.Respond(request, tokensOutput.ToEventHolder(context.Behavior.Id));
                    }
                }
                finally
                {
                    result = EventStatus.Consumed;
                }
            }
            else
            if (eventHolder is EventHolder<BehaviorInfoRequest> behaviorInfoRequest)
            {
                behaviorInfoRequest.Payload.Respond(new BehaviorInfo()
                {
                    Id = StateflowsContext.Id,
                    BehaviorStatus = StateflowsContext.Status,
                    ExpectedEvents = StateflowsContext.Status switch
                    {
                        BehaviorStatus.NotInitialized => [Event<Initialize>.Name],
                        _ => []
                    }
                });
            }
            else
            if (eventHolder is EventHolder<Subscribe> subscribeHolder)
            {
                var subscribe = subscribeHolder.Payload;
                if (StateflowsContext.AddSubscribers(subscribe.BehaviorId, subscribe.NotificationNames))
                {
                    result = EventStatus.Consumed;
                }
            }
            else
            if (eventHolder is EventHolder<Unsubscribe> unsubscribeHolder)
            {
                var unsubscribe = unsubscribeHolder.Payload;
                if (StateflowsContext.RemoveSubscribers(unsubscribe.BehaviorId, unsubscribe.NotificationNames))
                {
                    result = EventStatus.Consumed;
                }
            }
            else if (eventHolder is EventHolder<Initialize>)
            {
                using var context = new ActionDelegateContext(
                    StateflowsContext,
                    this,
                    eventHolder,
                    ServiceProvider,
                    [eventHolder.Payload.ToTokenHolder()]
                );

                try
                {
                    InputTokens.TokensHolder.Value = context.InputTokens.ToList();

                    await InvokeActionAsync(context);

                    InputTokens.TokensHolder.Value = null;

                    Trace.WriteLine($"⦗→s⦘ {StateflowsContext.Id.Type} '{StateflowsContext.Id.Name}:{StateflowsContext.Id.Instance}': executed");
                }
                finally
                {
                    result = EventStatus.Consumed;
                }
            }
            else if (eventHolder is EventHolder<Reset> resetHolder)
            {
                ResetBehavior(resetHolder.Payload.Mode);

                result = EventStatus.Consumed;
            }
            else if (eventHolder is EventHolder<Finalize>)
            {
                await inspector.OnActionFinalizeAsync(eventContext, () =>
                {
                    FinalizeBehavior(eventContext);
                    return Task.CompletedTask;
                });

                result = EventStatus.Consumed;
            }
            else
            {
                using var context = new ActionDelegateContext(StateflowsContext, this, eventHolder, ServiceProvider, [eventHolder.Payload.ToTokenHolder()]);
                try
                {
                    InputTokens.TokensHolder.Value = context.InputTokens.ToList();

                    await InvokeActionAsync(context);

                    InputTokens.TokensHolder.Value = null;

                    Trace.WriteLine($"⦗→s⦘ {StateflowsContext.Id.Type} '{StateflowsContext.Id.Name}:{StateflowsContext.Id.Instance}': executed");

                    HandleGuardRequest(eventHolder, context);
                }
                finally
                {
                    result = EventStatus.Consumed;
                }
            }

            await pendingTasks.DrainAsync();

            Trace.WriteLine($"⦗→s⦘ {StateflowsContext.Id.Type} '{StateflowsContext.Id.Name}:{StateflowsContext.Id.Instance}': processed event '{Event.GetName(eventHolder.PayloadType)}' with result '{result}'");

            return result;
        }

        private static void HandleGuardRequest<TEvent>(EventHolder<TEvent> eventHolder, ActionDelegateContext context)
        {
            var guardRequest = context.Headers.Values.OfType<TransitionGuardRequest>().FirstOrDefault();
            if (guardRequest != null)
            {
                var output = context.OutputTokens.OfType<TokenHolder<bool>>().FirstOrDefault()?.Payload ?? false;

                if (output && context.TryGetParentBehaviorContext(out var parentBehaviorContext))
                {
                    var headers = context.Headers
                        .Where(h => h.Value is not TransitionGuardRequest)
                        .ToDictionary();

                    headers[nameof(TransitionGuardResponse)] = new TransitionGuardResponse
                    {
                        GuardIdentifier = guardRequest.GuardIdentifier
                    };

                    parentBehaviorContext.Send(eventHolder.Payload, headers);
                }
                        
                var behaviorId = context.RootContext.Context.ContextOwnerId.Value;
                if (behaviorId.Type == BehaviorType.StateMachine)
                {
                    if (output)
                    {
                        switch (guardRequest.EdgeType)
                        {
                            case EdgeType.Transition:
                                Trace.WriteLine(
                                    $"⦗→s⦘ State Machine '{behaviorId.Name}:{behaviorId.Instance}': delegated guard passed transition from '{guardRequest.SourceName}' to '{guardRequest.TargetName}' triggered by event '{eventHolder.Name}', retransmitting event");
                                break;

                            case EdgeType.DefaultTransition:
                                Trace.WriteLine(
                                    $"⦗→s⦘ State Machine '{behaviorId.Name}:{behaviorId.Instance}': delegated guard passed default transition from '{guardRequest.SourceName}' to '{guardRequest.TargetName}', retransmitting event");
                                break;

                            case EdgeType.InternalTransition:
                                Trace.WriteLine(
                                    $"⦗→s⦘ State Machine '{behaviorId.Name}:{behaviorId.Instance}': delegated guard passed internal transition in '{guardRequest.SourceName}' triggered by event '{eventHolder.Name}', retransmitting event");
                                break;
                        }
                    }
                    else
                    {
                        switch (guardRequest.EdgeType)
                        {
                            case EdgeType.Transition:
                                Trace.WriteLine(
                                    $"⦗→s⦘ State Machine '{behaviorId.Name}:{behaviorId.Instance}': delegated guard stopped event '{eventHolder.Name}' from triggering transition from '{guardRequest.SourceName}' to '{guardRequest.TargetName}'");
                                break;

                            case EdgeType.DefaultTransition:
                                Trace.WriteLine(
                                    $"⦗→s⦘ State Machine '{behaviorId.Name}:{behaviorId.Instance}': delegated guard stopped default transition from '{guardRequest.SourceName}' to '{guardRequest.TargetName}'");
                                break;

                            case EdgeType.InternalTransition:
                                Trace.WriteLine(
                                    $"⦗→s⦘ State Machine '{behaviorId.Name}:{behaviorId.Instance}': delegated guard stopped event '{eventHolder.Name}' from triggering internal transition in '{guardRequest.SourceName}'");
                                break;
                        }
                    }
                }
            }
        }

        [DebuggerHidden]
        private async Task InvokeActionAsync(ActionDelegateContext context)
        {
            try
            {
                await ActionModel.Delegate.Invoke(context);
            }
            catch (Exception e)
            {
                StateflowsContext.ExecutionExceptions.Enqueue(e);
                Trace.WriteLine($"⦗→s⦘ {StateflowsContext.Id.Type} '{StateflowsContext.Id.Name}:{StateflowsContext.Id.Instance}': exception '{e.GetType().FullName}' thrown with message '{e.Message}'");
                await inspector.OnActionExceptionAsync(context, e);
                throw;
            }

            await Hub.PublishRangeAsync(context.OutputTokens.ToOutputNotifications(context.Behavior.Id));
        }


        [DebuggerHidden]
        private void FinalizeBehavior(ActionContext context)
        {
            StateflowsContext.Status = BehaviorStatus.Finalized;
        }

        [DebuggerHidden]
        private void ResetBehavior(ResetMode resetMode)
        {
            StateflowsContext.Values.Clear();

            if (resetMode != ResetMode.KeepVersionAndSubscriptions) // KeepSubcscriptions || Full
            {
                StateflowsContext.Version = 0;

                if (resetMode != ResetMode.KeepSubscriptions) // Full
                {
                    StateflowsContext.Deleted = true;
                }
            }

            StateflowsContext.Status = BehaviorStatus.NotInitialized;
        }
    }
}