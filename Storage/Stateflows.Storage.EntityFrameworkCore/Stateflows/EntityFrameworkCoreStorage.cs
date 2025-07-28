using Microsoft.EntityFrameworkCore;
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
                await DbContext.Contexts_v1.Where(x => x.TriggerOnStartup).ToArrayAsync();
                var contextEntity = await DbContext.Contexts_v1.FindOrCreate(context, true);
                contextEntity.Data = StateflowsJsonConverter.SerializePolymorphicObject(context);
                contextEntity.TriggerTime = context.TriggerTime;
                contextEntity.TriggerOnStartup = context.TriggerOnStartup;

                if (context.Deleted)
                {
                    if (contextEntity.Id != 0)
                    {
                        DbContext.Contexts_v1.Remove(contextEntity);
                    }
                }
                else
                {
                    if (contextEntity.Id == 0)
                    {
                        DbContext.Contexts_v1.Add(contextEntity);
                    }
                    else
                    {
                        DbContext.Contexts_v1.Update(contextEntity);
                    }
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

            result ??= new StateflowsContext(behaviorId);

            return result;
        }

        public async Task<IEnumerable<StateflowsContext>> GetAllContextsAsync(IEnumerable<BehaviorClass> behaviorClasses)
        {
            StateflowsContext[] result = Array.Empty<StateflowsContext>();

            try
            {
                var contexts = await DbContext.Contexts_v1.FindByClassesAsync(behaviorClasses);

                result = contexts.Select(c => StateflowsJsonConverter.DeserializeObject<StateflowsContext>(c.Data ?? string.Empty)).ToArray();
            }
            catch (Exception e)
            {
                Logger.LogError(LogTemplates.ExceptionLogTemplate, typeof(EntityFrameworkCoreStorage).FullName, nameof(GetAllContextsAsync), e.GetType().Name, e.Message);
            }

            return result;
        }

        public async Task<IEnumerable<BehaviorId>> GetAllContextIdsAsync(IEnumerable<BehaviorClass> behaviorClasses)
        {
            BehaviorId[] result = Array.Empty<BehaviorId>();

            try
            {
                var contexts = await DbContext.Contexts_v1.FindByClassesAsync(behaviorClasses);

                result = contexts.Select(c => StateflowsJsonConverter.DeserializeObject<BehaviorId>(c.BehaviorId)).ToArray();
            }
            catch (Exception e)
            {
                Logger.LogError(LogTemplates.ExceptionLogTemplate, typeof(EntityFrameworkCoreStorage).FullName, nameof(GetAllContextIdsAsync), e.GetType().Name, e.Message);
            }

            return result;
        }

        public Task<IEnumerable<StateflowsContext>> GetTimeTriggeredContextsAsync(IEnumerable<BehaviorClass> behaviorClasses)
        {
            StateflowsContext[] result = Array.Empty<StateflowsContext>();

            try
            {
                var contexts = DbContext.Contexts_v1.FindByTriggerTime(behaviorClasses);

                result = contexts.Select(c => StateflowsJsonConverter.DeserializeObject<StateflowsContext>(c.Data ?? string.Empty)).ToArray();
            }
            catch (Exception e)
            {
                Logger.LogError(LogTemplates.ExceptionLogTemplate, typeof(EntityFrameworkCoreStorage).FullName, nameof(GetTimeTriggeredContextsAsync), e.GetType().Name, e.Message);
            }

            return Task.FromResult(result as IEnumerable<StateflowsContext>);
        }

        public Task<IEnumerable<StateflowsContext>> GetStartupTriggeredContextsAsync(IEnumerable<BehaviorClass> behaviorClasses)
        {
            StateflowsContext[] result = Array.Empty<StateflowsContext>();

            try
            {
                var contexts = DbContext.Contexts_v1.FindByTriggerOnStartup(behaviorClasses);

                result = contexts.Select(c => StateflowsJsonConverter.DeserializeObject<StateflowsContext>(c.Data ?? string.Empty)).ToArray();
            }
            catch (Exception e)
            {
                Logger.LogError(LogTemplates.ExceptionLogTemplate, typeof(EntityFrameworkCoreStorage).FullName, nameof(GetTimeTriggeredContextsAsync), e.GetType().Name, e.Message);
            }

            return Task.FromResult(result as IEnumerable<StateflowsContext>);
        }
    }
}
