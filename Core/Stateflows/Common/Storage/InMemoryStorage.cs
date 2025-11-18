using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Stateflows.Common.Context;
using Stateflows.Common.Utilities;
using Stateflows.Common.Interfaces;

namespace Stateflows.Common.Storage
{
    public class InMemoryStorage : IStateflowsStorage
    {
        private readonly Dictionary<string, Dictionary<BehaviorId, string>> Contexts = new Dictionary<string, Dictionary<BehaviorId, string>>();

        public InMemoryStorage(ITenantAccessor tenantAccessor)
        {
            TenantAccessor = tenantAccessor;
        }

        private readonly ITenantAccessor TenantAccessor;

        public Task<StateflowsContext> HydrateAsync(BehaviorId behaviorId)
        {
            lock (Contexts)
            {
                var context = Contexts.TryGetValue(TenantAccessor.CurrentTenantId, out var tenantContexts) &&
                              tenantContexts.TryGetValue(behaviorId, out var contextStr)
                    ? StateflowsJsonConverter.DeserializeObject<StateflowsContext>(contextStr)
                    : new StateflowsContext(behaviorId);

                return Task.FromResult(context);
            }
        }

        public Task DehydrateAsync(StateflowsContext context)
        {
            lock (Contexts)
            {
                if (!Contexts.TryGetValue(TenantAccessor.CurrentTenantId, out var tenantContexts))
                {
                    tenantContexts = new Dictionary<BehaviorId, string>();
                    Contexts.Add(TenantAccessor.CurrentTenantId, tenantContexts);
                }
                
                if (context.Deleted)
                {
                    tenantContexts.Remove(context.Id);
                }
                else
                {
                    tenantContexts[context.Id] = StateflowsJsonConverter.SerializePolymorphicObject(context);
                    context.Stored = true;
                }
            }

            return Task.CompletedTask;
        }

        public Task<IEnumerable<StateflowsContext>> GetAllContextsAsync(IEnumerable<BehaviorClass> behaviorClasses)
        {
            IEnumerable<StateflowsContext> result;

            lock (Contexts)
            {
                if (!Contexts.TryGetValue(TenantAccessor.CurrentTenantId, out var tenantContexts))
                {
                    tenantContexts = new Dictionary<BehaviorId, string>();
                    Contexts.Add(TenantAccessor.CurrentTenantId, tenantContexts);
                }

                result = tenantContexts.Keys
                    .Where(key => behaviorClasses.Contains(key.BehaviorClass))
                    .Select(key => StateflowsJsonConverter.DeserializeObject<StateflowsContext>(tenantContexts[key]))
                    .ToArray();
            }

            return Task.FromResult(result);
        }

        public Task<IEnumerable<BehaviorId>> GetAllContextIdsAsync(IEnumerable<BehaviorClass> behaviorClasses)
        {
            IEnumerable<BehaviorId> result;

            lock (Contexts)
            {
                if (!Contexts.TryGetValue(TenantAccessor.CurrentTenantId, out var tenantContexts))
                {
                    tenantContexts = new Dictionary<BehaviorId, string>();
                    Contexts.Add(TenantAccessor.CurrentTenantId, tenantContexts);
                }

                result = tenantContexts.Keys
                    .Where(key => behaviorClasses.Contains(key.BehaviorClass))
                    .ToArray();
            }

            return Task.FromResult(result);
        }

        public async Task<IEnumerable<StateflowsContext>> GetTimeTriggeredContextsAsync(IEnumerable<BehaviorClass> behaviorClasses)
            => (await GetAllContextsAsync(behaviorClasses))
                .Where(context =>
                    context.TriggerTime != null &&
                    context.TriggerTime < DateTime.Now
                )
                .ToArray();

        public async Task<IEnumerable<StateflowsContext>> GetStartupTriggeredContextsAsync(IEnumerable<BehaviorClass> behaviorClasses)
            => (await GetAllContextsAsync(behaviorClasses))
                .Where(context => context.TriggerOnStartup)
                .ToArray();

        public void Dispose()
        {
            // TODO release managed resources here
        }
    }
}
