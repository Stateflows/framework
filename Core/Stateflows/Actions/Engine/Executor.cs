using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Stateflows.Actions.Context.Classes;
using Stateflows.Actions.Context.Interfaces;
using Stateflows.Actions.Registration;
using Stateflows.Activities;
using Stateflows.Common;
using Stateflows.Common.Classes;
using Stateflows.Common.Context;
using Stateflows.Common.Interfaces;

namespace Stateflows.Actions.Engine
{
    public class Executor : IStateflowsExecutor
    {
        public Executor(StateflowsContext stateflowsContext, IServiceProvider serviceProvider, (string Name, int Version, bool Reentrant, ActionDelegateAsync Delegate) action)
        {
            StateflowsContext = stateflowsContext;
            ServiceProvider = serviceProvider;
            Action = action;
        }
        
        private readonly StateflowsContext StateflowsContext;
        private readonly IServiceProvider ServiceProvider;
        private readonly (string Name, int Version, bool Reentrant, ActionDelegateAsync Delegate) Action;
        
        public async Task<EventStatus> DoProcessAsync<TEvent>(EventHolder<TEvent> eventHolder)
        {
            var result = EventStatus.NotConsumed;
            
            Trace.WriteLine($"⦗→s⦘ Action '{StateflowsContext.Id.Name}:{StateflowsContext.Id.Instance}': received event '{Event.GetName(eventHolder.PayloadType)}', trying to process it");

            if (eventHolder is EventHolder<TokensInput> tokensInputHolder)
            {
                var context = new ActionDelegateContext(StateflowsContext, ServiceProvider, tokensInputHolder.Payload.Tokens);
                InputTokens.TokensHolder.Value = context.InputTokens.ToList();
                
                await Action.Delegate.Invoke(context);

                InputTokens.TokensHolder.Value = null;
                
                Trace.WriteLine($"⦗→s⦘ Action '{StateflowsContext.Id.Name}:{StateflowsContext.Id.Instance}': executed");
                
                var tokensOutput = new TokensOutput()
                {
                    Tokens = context.OutputTokens.ToList()
                };
                    
                tokensInputHolder.Respond(tokensOutput.ToEventHolder());
                
                result = EventStatus.Consumed;
            }
            
            if (eventHolder is EventHolder<SetGlobalValues> setGlobalValuesHolder)
            {
                IActionDelegateContext context = new ActionDelegateContext(StateflowsContext, ServiceProvider);
                var values = (ContextValuesCollection)context.Action.Values;
            
                values.Values.Clear();
                foreach (var entry in @setGlobalValuesHolder.Payload.Values)
                {
                    values.Values[entry.Key] = entry.Value;
                }
            }

            if (eventHolder is EventHolder<Subscribe> subscribeHolder)
            {
                var subscribe = subscribeHolder.Payload;
                if (StateflowsContext.AddSubscribers(subscribe.BehaviorId, subscribe.NotificationNames))
                {
                    result = EventStatus.Consumed;
                }
            }

            if (eventHolder is EventHolder<Unsubscribe> unsubscribeHolder)
            {
                var unsubscribe = unsubscribeHolder.Payload;
                if (StateflowsContext.RemoveSubscribers(unsubscribe.BehaviorId, unsubscribe.NotificationNames))
                {
                    result = EventStatus.Consumed;
                }
            }

            Trace.WriteLine($"⦗→s⦘ Action '{StateflowsContext.Id.Name}:{StateflowsContext.Id.Instance}': processed event '{Event.GetName(eventHolder.PayloadType)}'");

            return result;
        }
    }
}