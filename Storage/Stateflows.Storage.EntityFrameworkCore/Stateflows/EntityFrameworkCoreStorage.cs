using Microsoft.Extensions.Logging;
using Stateflows.Common;
using Stateflows.Common.Context;
using Stateflows.Common.Utilities;
using Stateflows.Common.Interfaces;
using Stateflows.Common.Trace.Models;
using Stateflows.Storage.EntityFrameworkCore.Utils;
using Stateflows.Storage.EntityFrameworkCore.EntityFrameworkCore;
using Stateflows.Storage.EntityFrameworkCore.EntityFrameworkCore.Entities;

namespace Stateflows.Storage.EntityFrameworkCore.Stateflows
{
    internal class EntityFrameworkCoreStorage : IStateflowsStorage
    {
        private readonly IStateflowsDbContext_v1 DbContext;
        private readonly ILogger<EntityFrameworkCoreStorage> Logger;

        public EntityFrameworkCoreStorage(IStateflowsDbContext_v1 dbContext, ILogger<EntityFrameworkCoreStorage> logger)
        {
            DbContext = dbContext;
            Logger = logger;
        }

        public async Task DehydrateAsync(StateflowsContext context)
        {
            try
            {
                var contextEntity = await DbContext.Contexts_v1.FindOrCreate(context, true);
                contextEntity.Data = StateflowsJsonConverter.SerializePolymorphicObject(context);
                contextEntity.TriggerTime = context.TriggerTime;
                if (contextEntity.Id == 0)
                {
                    DbContext.Contexts_v1.Add(contextEntity);
                }
                else
                {
                    DbContext.Contexts_v1.Update(contextEntity);
                }

                await DbContext.SaveChangesAsync();
            }
            catch (Exception e)
            {
                Logger.LogError(LogTemplates.ExceptionLogTemplate, typeof(EntityFrameworkCoreStorage).FullName, nameof(DehydrateAsync), e.GetType().Name, e.Message);
            }
        }

        public async Task<StateflowsContext> HydrateAsync(BehaviorId behaviorId)
        {
            StateflowsContext? result = null;

            try
            {
                var c = await DbContext.Contexts_v1.FindOrCreate(behaviorId);

                result = StateflowsJsonConverter.DeserializeObject<StateflowsContext>(c.Data ?? string.Empty);
            }
            catch (Exception e)
            {
                Logger.LogError(LogTemplates.ExceptionLogTemplate, typeof(EntityFrameworkCoreStorage).FullName, nameof(HydrateAsync), e.GetType().Name, e.Message);
            }

            result ??= new StateflowsContext() { Id = behaviorId };

            return result;
        }

        public async Task<IEnumerable<StateflowsContext>> GetContextsAsync(IEnumerable<BehaviorClass> behaviorClasses)
        {
            StateflowsContext[] result = Array.Empty<StateflowsContext>();

            try
            {
                var contexts = await DbContext.Contexts_v1.FindByClassesAsync(behaviorClasses);

                result = contexts.Select(c => StateflowsJsonConverter.DeserializeObject<StateflowsContext>(c.Data ?? string.Empty)).ToArray();
            }
            catch (Exception e)
            {
                Logger.LogError(LogTemplates.ExceptionLogTemplate, typeof(EntityFrameworkCoreStorage).FullName, nameof(GetContextsAsync), e.GetType().Name, e.Message);
            }

            return result;
        }

        public Task<IEnumerable<StateflowsContext>> GetContextsToTimeTriggerAsync(IEnumerable<BehaviorClass> behaviorClasses)
        {
            StateflowsContext[] result = Array.Empty<StateflowsContext>();

            try
            {
                var contexts = DbContext.Contexts_v1.FindByTriggerTime(behaviorClasses);

                result = contexts.Select(c => StateflowsJsonConverter.DeserializeObject<StateflowsContext>(c.Data ?? string.Empty)).ToArray();
            }
            catch (Exception e)
            {
                Logger.LogError(LogTemplates.ExceptionLogTemplate, typeof(EntityFrameworkCoreStorage).FullName, nameof(GetContextsToTimeTriggerAsync), e.GetType().Name, e.Message);
            }

            return Task.FromResult(result as IEnumerable<StateflowsContext>);
        }

        public async Task SaveTraceAsync(BehaviorTrace behaviorTrace)
        {
            try
            {
                var traceEntity = new Trace_v1(
                    behaviorTrace.BehaviorId.ToString(),
                    behaviorTrace.ExecutedAt,
                    StateflowsJsonConverter.SerializePolymorphicObject(behaviorTrace)
                );

                DbContext.Traces_v1.Add(traceEntity);

                await DbContext.SaveChangesAsync();
            }
            catch (Exception e)
            {
                Logger.LogError(LogTemplates.ExceptionLogTemplate, typeof(EntityFrameworkCoreStorage).FullName, nameof(SaveTraceAsync), e.GetType().Name, e.Message);
            }
        }

        public async Task<IEnumerable<BehaviorTrace>> GetTracesAsync(BehaviorId behaviorId)
        {
            BehaviorTrace[] result = Array.Empty<BehaviorTrace>();

            try
            {
                var contexts = await DbContext.Traces_v1.FindByBehaviorIdAsync(behaviorId);

                result = contexts.Select(c => StateflowsJsonConverter.DeserializeObject<BehaviorTrace>(c.Data ?? string.Empty)).ToArray();
            }
            catch (Exception e)
            {
                Logger.LogError(LogTemplates.ExceptionLogTemplate, typeof(EntityFrameworkCoreStorage).FullName, nameof(GetTracesAsync), e.GetType().Name, e.Message);
            }

            return result;
        }
    }
}
