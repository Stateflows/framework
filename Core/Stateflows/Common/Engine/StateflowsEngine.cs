using System;
using System.Linq;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Stateflows.Common.Classes;
using Stateflows.Common.Engine;
using Stateflows.Common.Interfaces;

namespace Stateflows.Common
{
    internal class StateflowsEngine : IStateflowsEngine
    {
        private readonly IServiceScope Scope;
        private IServiceProvider ServiceProvider => Scope.ServiceProvider;
        private readonly IStateflowsLock Lock;
        private readonly CommonInterceptor Interceptor;
        private readonly IStateflowsTenantProvider TenantProvider;
        private readonly ITenantAccessor TenantAccessor;
        private readonly IStateflowsValidator[] Validators;
        private Dictionary<string, IEventProcessor> processors;

        private Dictionary<string, IEventProcessor> Processors
            => processors ??= ServiceProvider.GetRequiredService<IEnumerable<IEventProcessor>>().ToDictionary(p => p.BehaviorType, p => p);

        public StateflowsEngine(IServiceProvider serviceProvider)
        {
            Scope = serviceProvider.CreateScope();
            Lock = ServiceProvider.GetRequiredService<IStateflowsLock>();
            Interceptor = ServiceProvider.GetRequiredService<CommonInterceptor>();
            TenantAccessor = ServiceProvider.GetRequiredService<ITenantAccessor>();
            TenantProvider = ServiceProvider.GetRequiredService<IStateflowsTenantProvider>();
            Validators = ServiceProvider.GetRequiredService<IEnumerable<IStateflowsValidator>>().ToArray();
        }

        [DebuggerHidden]
        public async Task HandleEventAsync(ExecutionToken token)
        {
            ResponseHolder.SetResponses(token.Responses);

            token.Validation = await token.EventHolder.ValidateAsync(Validators);

            ResponseHolder.ClearResponses();

            var status = EventStatus.Invalid;

            try
            {
                if (token.Validation.IsValid)
                {
                    status = await token.EventHolder.ProcessEventAsync(this, token.TargetId, token.Exceptions, token.Responses);
                }

                token.Status = token.Validation.IsValid
                    ? status
                    : EventStatus.Invalid;

                token.Handled.Set();
            }
            catch (Exception)
            {
                try
                {
                    throw;
                }
                finally
                {
                    token.Status = EventStatus.Failed;

                    token.Handled.Set();
                }
            }
        }

        [DebuggerHidden]
        async Task<EventStatus> IStateflowsEngine.ProcessEventAsync<TEvent>(BehaviorId id, EventHolder<TEvent> eventHolder, List<Exception> exceptions, Dictionary<object, EventHolder> responses)
        {
            var result = EventStatus.Undelivered;

            if (!Processors.TryGetValue(id.Type, out var processor) || !Interceptor.BeforeExecute(eventHolder))
            {
                return result;
            }
            
            TenantAccessor.CurrentTenantId = await TenantProvider.GetCurrentTenantIdAsync();

            await using var lockHandle = await (
                id.Type == BehaviorType.Action
                    ? Lock.AquireNoLockAsync(id)
                    : Lock.AquireLockAsync(id)
            );

            try
            {
                try
                {
                    ResponseHolder.SetResponses(responses);

                    result = await processor.ProcessEventAsync(id, eventHolder, exceptions);
                }
                finally
                {
                    var responseHolder = eventHolder.GetResponseHolder();
                    if (responseHolder != null)
                    {
                        responseHolder.SenderId = id;
                        responseHolder.SentAt = DateTime.Now;

                        if (eventHolder is EventHolder<CompoundRequest> compoundRequest)
                        {
                            foreach (var subResponseHolder in compoundRequest.Payload.Events
                                         .Select(subEventHolder => subEventHolder.GetResponseHolder())
                                         .Where(subResponseHolder => subResponseHolder != null)
                            )
                            {
                                subResponseHolder.SenderId = id;
                                subResponseHolder.SentAt = DateTime.Now;
                            }
                        }
                    }
                }
            }
            finally
            {
                Interceptor.AfterExecute(eventHolder);
            }

            return result;
        }
    }
}
