using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Stateflows.Actions.Context.Classes;
using Stateflows.Actions.Context.Interfaces;
using Stateflows.Actions.Models;
using Stateflows.Actions.Registration;
using Stateflows.Activities;
using Stateflows.Common;
using Stateflows.Common.Classes;
using Stateflows.Common.Context;
using Stateflows.Common.Exceptions;
using Stateflows.Common.Interfaces;
using Stateflows.StateMachines;

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
            
            await inspector.AfterHydrateAsync(new ActionDelegateContext(StateflowsContext, eventHolder, scope.ServiceProvider));
            
            scope.Dispose();
        }

        public async Task DehydrateAsync(EventHolder eventHolder)
        {
            var inspector = await GetInspectorAsync();

            var scope = ServiceProvider.CreateScope();

            await inspector.BeforeDehydrateAsync(new ActionDelegateContext(StateflowsContext, eventHolder, scope.ServiceProvider));
            
            scope.Dispose();
        }
        
        public async Task<EventStatus> DoProcessAsync<TEvent>(EventHolder<TEvent> eventHolder)
        {
            var result = EventStatus.NotConsumed;
            
            var inspector = await GetInspectorAsync();
            
            Trace.WriteLine($"⦗→s⦘ Action '{StateflowsContext.Id.Name}:{StateflowsContext.Id.Instance}': received event '{Event.GetName(eventHolder.PayloadType)}', trying to process it");

            var eventContext = new EventContext<TEvent>(StateflowsContext, eventHolder, ServiceProvider);
            await this.inspector.BeforeProcessEventAsync(eventContext);
            
            if (eventHolder is EventHolder<TokensInput> tokensInputHolder)
            {
                var context = new ActionDelegateContext(StateflowsContext, eventHolder, ServiceProvider, tokensInputHolder.Payload.Tokens);
                InputTokens.TokensHolder.Value = context.InputTokens.ToList();
                
                await ActionModel.Delegate.Invoke(context);

                InputTokens.TokensHolder.Value = null;
                
                Trace.WriteLine($"⦗→s⦘ Action '{StateflowsContext.Id.Name}:{StateflowsContext.Id.Instance}': executed");
                
                var tokensOutput = new TokensOutput()
                {
                    Tokens = context.OutputTokens.ToList()
                };
                    
                tokensInputHolder.Respond(tokensOutput.ToEventHolder());
                
                result = EventStatus.Consumed;
            }
            else
            if (eventHolder is EventHolder<SetGlobalValues> setGlobalValuesHolder)
            {
                IActionDelegateContext context = eventContext;
                var values = (ContextValuesCollection)context.Behavior.Values;
            
                values.Values.Clear();
                foreach (var entry in @setGlobalValuesHolder.Payload.Values)
                {
                    values.Values[entry.Key] = entry.Value;
                }
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
            else if (eventHolder is EventHolder<Initialize> initializeHolder)
            {
                var context = new ActionDelegateContext(StateflowsContext, eventHolder, ServiceProvider, new List<TokenHolder>() { eventHolder.Payload.ToTokenHolder() });
                InputTokens.TokensHolder.Value = context.InputTokens.ToList();
                
                await ActionModel.Delegate.Invoke(context);

                InputTokens.TokensHolder.Value = null;
                
                Trace.WriteLine($"⦗→s⦘ Action '{StateflowsContext.Id.Name}:{StateflowsContext.Id.Instance}': executed");

                result = EventStatus.Consumed;
            }
            else if (eventHolder is EventHolder<Reset> resetHolder)
            {
                Reset(resetHolder.Payload.Mode);
                
                result = EventStatus.Consumed;
            }
            else if (eventHolder is EventHolder<Finalize> finalizeHolder)
            {
                Finalize();
                
                result = EventStatus.Consumed;
            }
            else if (eventHolder is EventHolder<NotificationsRequest> notificationsRequestHolder)
            {
                var pendingNotifications = Hub.Notifications.TryGetValue(StateflowsContext.Id, out var notifications)
                    ? notifications
                        .Where(h => notificationsRequestHolder.Payload.NotificationNames.Contains(h.Name))
                        .Where(h => h.SentAt >= DateTime.Now - notificationsRequestHolder.Payload.Period)
                        .ToArray()
                    : Array.Empty<EventHolder>();
            
                notificationsRequestHolder.Payload.Respond(
                    new NotificationsResponse
                    {
                        Notifications = pendingNotifications
                    });
                
                result = EventStatus.Consumed;
            }
            else
            {
                var context = new ActionDelegateContext(StateflowsContext, eventHolder, ServiceProvider, new List<TokenHolder>() { eventHolder.Payload.ToTokenHolder() });
                InputTokens.TokensHolder.Value = context.InputTokens.ToList();

                try
                {
                    await ActionModel.Delegate.Invoke(context);
                }
                catch (Exception e)
                {
                    Trace.WriteLine($"⦗→s⦘ Action '{StateflowsContext.Id.Name}:{StateflowsContext.Id.Instance}': exception thrown '{e.Message}'");
                    if (!await inspector.OnActionExceptionAsync(context, e))
                    {
                        throw;
                    }
                    else
                    {
                        throw new BehaviorExecutionException(e);
                    }
                }

                InputTokens.TokensHolder.Value = null;
                
                Trace.WriteLine($"⦗→s⦘ Action '{StateflowsContext.Id.Name}:{StateflowsContext.Id.Instance}': executed");

                result = EventStatus.Consumed;
            }
            
            await this.inspector.AfterProcessEventAsync(eventContext);
            
            Trace.WriteLine($"⦗→s⦘ Action '{StateflowsContext.Id.Name}:{StateflowsContext.Id.Instance}': processed event '{Event.GetName(eventHolder.PayloadType)}'");

            return result;
        }


        [DebuggerHidden]
        public void Finalize()
        {
            StateflowsContext.Status = BehaviorStatus.Finalized;
        }

        [DebuggerHidden]
        public void Reset(ResetMode resetMode)
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