using Microsoft.Extensions.Logging;
using Stateflows.Common;
using Stateflows.Common.Context;
using Stateflows.Common.Utilities;
using Stateflows.Common.Interfaces;
using Stateflows.Storage.EntityFrameworkCore.Utils;
using Stateflows.Storage.EntityFrameworkCore.EntityFrameworkCore;

namespace Stateflows.Storage.EntityFrameworkCore.Stateflows
{
    internal class EntityFrameworkCoreStorage : IStateflowsStorage
    {
        private readonly IStateflowsDbContext_v1 DbContext;
        private readonly EventWaitHandle EventWaitHandle;
        private readonly ILogger<EntityFrameworkCoreStorage> Logger;

        public EntityFrameworkCoreStorage(IStateflowsDbContext_v1 dbContext, ILogger<EntityFrameworkCoreStorage> logger)
        {
            DbContext = dbContext;
            Logger = logger;
            EventWaitHandle = new EventWaitHandle(true, EventResetMode.AutoReset);
        }

        public async Task Dehydrate(StateflowsContext context)
        {
            await EventWaitHandle.WaitOneAsync();

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
            finally
            {
                EventWaitHandle.Set();
            }
        }

        public async Task<StateflowsContext> Hydrate(BehaviorId id)
        {
            StateflowsContext? result = null;

            await EventWaitHandle.WaitOneAsync();

            try
            {
                try
                {
                    var c = await DbContext.Contexts_v1.FindOrCreate(id);

                    result = StateflowsJsonConverter.DeserializeObject<StateflowsContext>(c.Data ?? string.Empty);
                }
                catch (Exception e)
                {
                    Logger.LogError(LogTemplates.ExceptionLogTemplate, typeof(EntityFrameworkCoreStorage).FullName, nameof(Hydrate), e.GetType().Name, e.Message);
                }

                result ??= new StateflowsContext() { Id = id };
            }
            finally
            {
                EventWaitHandle.Set();
            }

            return result;
        }

        public async Task<IEnumerable<StateflowsContext>> GetContexts(IEnumerable<BehaviorClass> behaviorClasses)
        {
            StateflowsContext[] result = Array.Empty<StateflowsContext>();

            await EventWaitHandle.WaitOneAsync();

            try
            {
                try
                {
                    var contexts = await DbContext.Contexts_v1.FindByClasses(behaviorClasses);

                    result = contexts.Select(c => StateflowsJsonConverter.DeserializeObject<StateflowsContext>(c.Data ?? string.Empty)).ToArray();
                }
                catch (Exception e)
                {
                    Logger.LogError(LogTemplates.ExceptionLogTemplate, typeof(EntityFrameworkCoreStorage).FullName, nameof(GetContexts), e.GetType().Name, e.Message);
                }
            }
            finally
            {
                EventWaitHandle.Set();
            }

            return result;
        }

        public async Task<IEnumerable<StateflowsContext>> GetContextsToTimeTrigger(IEnumerable<BehaviorClass> behaviorClasses)
        {
            StateflowsContext[] result = Array.Empty<StateflowsContext>();

            await EventWaitHandle.WaitOneAsync();

            try
            {
                try
                {
                    var contexts = await DbContext.Contexts_v1.FindByTriggerTime(behaviorClasses);

                    result = contexts.Select(c => StateflowsJsonConverter.DeserializeObject<StateflowsContext>(c.Data ?? string.Empty)).ToArray();
                }
                catch (Exception e)
                {
                    Logger.LogError(LogTemplates.ExceptionLogTemplate, typeof(EntityFrameworkCoreStorage).FullName, nameof(GetContextsToTimeTrigger), e.GetType().Name, e.Message);
                }
            }
            finally
            {
                EventWaitHandle.Set();
            }

            return result;
        }
    }
}
