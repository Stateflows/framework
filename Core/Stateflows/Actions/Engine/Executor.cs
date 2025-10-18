using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Stateflows.Common;
using Stateflows.Common.Classes;
using Stateflows.Common.Context;
using Stateflows.Common.Exceptions;
using Stateflows.Common.Interfaces;
using Stateflows.Actions.Context.Classes;
using Stateflows.Actions.Context.Interfaces;
using Stateflows.Actions.Models;
using Stateflows.Actions.Registration;
using Stateflows.Activities;
using Stateflows.Common.Utilities;
using Stateflows.StateMachines;
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

        private Inspector inspector;

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
            var inspector = await GetInspectorAsync();
            
            var scope = ServiceProvider.CreateScope();

            var context = new ActionDelegateContext(StateflowsContext, eventHolder, scope.ServiceProvider);
            try
            {
                inspector.AfterHydrate(context);
            }
            finally
            {
                context.Clear();
            
                scope.Dispose();
            }
        }

        public async Task DehydrateAsync(EventHolder eventHolder)
        {
            var inspector = await GetInspectorAsync();

            var scope = ServiceProvider.CreateScope();

            var context = new ActionDelegateContext(StateflowsContext, eventHolder, scope.ServiceProvider);
            
            try
            {
                inspector.BeforeDehydrate(context);
            }
            finally
            {
                context.Clear();
            
                scope.Dispose();
            }
        }
        
        public async Task<EventStatus> DoProcessAsync<TEvent>(EventHolder<TEvent> eventHolder)
        {
            var result = EventStatus.NotConsumed;
            
            var inspector = await GetInspectorAsync();
            
            Trace.WriteLine($"⦗→s⦘ Action '{StateflowsContext.Id.Name}:{StateflowsContext.Id.Instance}': received event '{Event.GetName(eventHolder.PayloadType)}', processing");

            var eventContext = new EventContext<TEvent>(StateflowsContext, eventHolder, ServiceProvider);
            this.inspector.BeforeProcessEvent(eventContext);
            
            try
            {
                if (eventHolder is EventHolder<TokensInput> tokensInputHolder)
                {
                    var context = new ActionDelegateContext(StateflowsContext, eventHolder, ServiceProvider, tokensInputHolder.Payload.Tokens);
                    try
                    {
                        InputTokens.TokensHolder.Value = context.InputTokens.ToList();
                    
                        await InvokeActionAsync(context);

                        InputTokens.TokensHolder.Value = null;
                    
                        Trace.WriteLine($"⦗→s⦘ Action '{StateflowsContext.Id.Name}:{StateflowsContext.Id.Instance}': executed");
                    
                        var tokensOutput = new TokensOutput()
                        {
                            Tokens = context.OutputTokens.ToList()
                        };
                        
                        tokensInputHolder.Respond(tokensOutput.ToEventHolder());
                    }
                    finally
                    {
                        context.Clear();
                    
                        result = EventStatus.Consumed;
                    }
                }
                else
                if (eventHolder is EventHolder<SetContextOwner> setContextOwnerHolder)
                {
                    eventContext.RootContext.Context.ContextOwnerId = setContextOwnerHolder.Payload.ContextOwnerId;
                    eventContext.RootContext.Context.ContextParentId = setContextOwnerHolder.Payload.ContextParentId;
                    
                    result = EventStatus.Consumed;
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
                else
                if (eventHolder is EventHolder<StartRelay> startRelayHolder)
                {
                    var startRelay = startRelayHolder.Payload;
                    if (StateflowsContext.AddRelays(startRelay.BehaviorId, startRelay.NotificationNames))
                    {
                        result = EventStatus.Consumed;
                    }
                }
                else
                if (eventHolder is EventHolder<StopRelay> stopRelayHolder)
                {
                    var stopRelay = stopRelayHolder.Payload;
                    if (StateflowsContext.RemoveRelays(stopRelay.BehaviorId, stopRelay.NotificationNames))
                    {
                        result = EventStatus.Consumed;
                    }
                }
                else if (eventHolder is EventHolder<Initialize>)
                {
                    var context = new ActionDelegateContext(StateflowsContext, eventHolder, ServiceProvider, new List<TokenHolder>() { eventHolder.Payload.ToTokenHolder() });
                    try
                    {
                        InputTokens.TokensHolder.Value = context.InputTokens.ToList();
                    
                        await InvokeActionAsync(context);

                        InputTokens.TokensHolder.Value = null;
                    
                        Trace.WriteLine($"⦗→s⦘ Action '{StateflowsContext.Id.Name}:{StateflowsContext.Id.Instance}': executed");
                    }
                    finally
                    {
                        context.Clear();

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
                    FinalizeBehavior(eventContext);
                    
                    result = EventStatus.Consumed;
                }
                else
                {
                    var context = new ActionDelegateContext(StateflowsContext, eventHolder, ServiceProvider, [eventHolder.Payload.ToTokenHolder()]);
                    try
                    {
                        InputTokens.TokensHolder.Value = context.InputTokens.ToList();

                        await InvokeActionAsync(context);

                        InputTokens.TokensHolder.Value = null;
                    
                        Trace.WriteLine($"⦗→s⦘ Action '{StateflowsContext.Id.Name}:{StateflowsContext.Id.Instance}': executed");

                        HandleGuardRequest(eventHolder, context);
                    }
                    finally
                    {
                        context.Clear();

                        result = EventStatus.Consumed;
                    }
                }
            }
            finally
            {
                if (result == EventStatus.Undelivered)
                {
                    result = EventStatus.Failed;
                }
                
                inspector.AfterProcessEvent(eventContext, result);
            }
            
            Trace.WriteLine($"⦗→s⦘ Action '{StateflowsContext.Id.Name}:{StateflowsContext.Id.Instance}': processed event '{Event.GetName(eventHolder.PayloadType)}' with result '{result}'");

            return result;
        }

        private static void HandleGuardRequest<TEvent>(EventHolder<TEvent> eventHolder, ActionDelegateContext context)
        {
            var guardRequest = context.Headers.OfType<GuardRequest>().FirstOrDefault();
            if (guardRequest != null)
            {
                var output = context.OutputTokens.OfType<TokenHolder<bool>>().FirstOrDefault()?.Payload ?? false;

                if (output)
                {
                    var headers = context.Headers
                        .Where(h => !(h is GuardRequest))
                        .Append(
                            new GuardResponse()
                            {
                                GuardIdentifier = guardRequest.GuardIdentifier
                            }
                        )
                        .ToArray();

                    context.Send(eventHolder.Payload, headers);
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
                Trace.WriteLine($"⦗→s⦘ Action '{StateflowsContext.Id.Name}:{StateflowsContext.Id.Instance}': exception '{e.GetType().FullName}' thrown with message '{e.Message}'");
                if (!inspector.OnActionException(context, e))
                {
                    throw;
                }
                else
                {
                    throw new BehaviorExecutionException(e);
                }
            }
        }


        [DebuggerHidden]
        private void FinalizeBehavior(ActionContext context)
        {
            StateflowsContext.Status = BehaviorStatus.Finalized;

            if (context.Context.ContextParentId != null)
            {
                context.Send(new DoActionFinalized());
            }
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
        }
    }
}