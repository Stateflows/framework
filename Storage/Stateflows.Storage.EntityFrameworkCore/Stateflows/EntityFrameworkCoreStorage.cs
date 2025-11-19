using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Stateflows.Common;
using Stateflows.Common.Context;
using Stateflows.Common.Utilities;
using Stateflows.Common.Interfaces;
using Stateflows.Storage.EntityFrameworkCore.Utils;
using Stateflows.Storage.EntityFrameworkCore.EntityFrameworkCore;
using Stateflows.Storage.EntityFrameworkCore.EntityFrameworkCore.Entities;

namespace Stateflows.Storage.EntityFrameworkCore.Stateflows
{
    internal class EntityFrameworkCoreStorage<TDbContext>(
        ILogger<EntityFrameworkCoreStorage<TDbContext>> logger,
        IServiceProvider serviceProvider)
        : IStateflowsStorage
        where TDbContext : DbContext, IStateflowsDbContext_v1
    {
        public async Task DehydrateAsync(StateflowsContext context)
        {            
            await using var scope = serviceProvider.CreateAsyncScope();
            var dbContextFactory = scope.ServiceProvider.GetService<IDbContextFactory<TDbContext>>() ?? new DbContextFactory<TDbContext>(scope.ServiceProvider);
            var dbContext = await dbContextFactory.CreateDbContextAsync();
            try
            {
                var contextEntity = context.RuntimeMetadata.TryGetValue(nameof(EntityFrameworkCoreStorage<TDbContext>), out var contextEntityObj)
                    ? (Context_v1)contextEntityObj
                    : await dbContext.Contexts_v1.FindOrCreate(context, true);

                context.RuntimeMetadata.Remove(nameof(EntityFrameworkCoreStorage<TDbContext>));

                // var contextEntity = await dbContext.Contexts_v1.FindOrCreate(context, true);
                contextEntity.Data = StateflowsJsonConverter.SerializePolymorphicObject(context);
                contextEntity.TriggerTime = context.TriggerTime;
                contextEntity.TriggerOnStartup = context.TriggerOnStartup;

                if (context.Deleted)
                {
                    if (contextEntity.Id != 0)
                    {
                        dbContext.Contexts_v1.Remove(contextEntity);
                    }
                }
                else
                {
                    if (contextEntity.Id == 0)
                    {
                        dbContext.Contexts_v1.Add(contextEntity);
                    }
                    else
                    {
                        dbContext.Contexts_v1.Update(contextEntity);
                    }
                }

                await dbContext.SaveChangesAsync();
                
                dbContext.ChangeTracker.Clear();
            }
            catch (Exception e)
            {
                logger.LogError(e, LogTemplates.ExceptionLogTemplate, typeof(EntityFrameworkCoreStorage<TDbContext>).FullName, nameof(DehydrateAsync), e.GetType().Name, e.Message);
            }
        }

        public async Task<StateflowsContext> HydrateAsync(BehaviorId behaviorId)
        {            
            await using var scope = serviceProvider.CreateAsyncScope();
            var dbContextFactory = scope.ServiceProvider.GetService<IDbContextFactory<TDbContext>>() ?? new DbContextFactory<TDbContext>(scope.ServiceProvider);
            var dbContext = await dbContextFactory.CreateDbContextAsync();
            
            StateflowsContext? result = null;

            try
            {
                var c = await dbContext.Contexts_v1.FindOrCreate(behaviorId);

                result = StateflowsJsonConverter.DeserializeObject<StateflowsContext>(c.Data ?? string.Empty);
                result?.RuntimeMetadata.Add(nameof(EntityFrameworkCoreStorage<TDbContext>), c);
            }
            catch (Exception e)
            {
                logger.LogError(e, LogTemplates.ExceptionLogTemplate, typeof(EntityFrameworkCoreStorage<TDbContext>).FullName, nameof(HydrateAsync), e.GetType().Name, e.Message);
            }

            result ??= new StateflowsContext(behaviorId);

            return result;
        }

        public async Task<IEnumerable<StateflowsContext>> GetAllContextsAsync(IEnumerable<BehaviorClass> behaviorClasses)
        {            
            await using var scope = serviceProvider.CreateAsyncScope();
            var dbContextFactory = scope.ServiceProvider.GetService<IDbContextFactory<TDbContext>>() ?? new DbContextFactory<TDbContext>(scope.ServiceProvider);
            var dbContext = await dbContextFactory.CreateDbContextAsync();
            
            var result = Array.Empty<StateflowsContext>();

            try
            {
                var contexts = await dbContext.Contexts_v1.FindByClassesAsync(behaviorClasses);

                result = contexts.Select(c => StateflowsJsonConverter.DeserializeObject<StateflowsContext>(c.Data ?? string.Empty)).ToArray();
            }
            catch (Exception e)
            {
                logger.LogError(e, LogTemplates.ExceptionLogTemplate, typeof(EntityFrameworkCoreStorage<TDbContext>).FullName, nameof(GetAllContextsAsync), e.GetType().Name, e.Message);
            }

            return result;
        }

        public async Task<IEnumerable<BehaviorId>> GetAllContextIdsAsync(IEnumerable<BehaviorClass> behaviorClasses)
        {            
            await using var scope = serviceProvider.CreateAsyncScope();
            var dbContextFactory = scope.ServiceProvider.GetService<IDbContextFactory<TDbContext>>() ?? new DbContextFactory<TDbContext>(scope.ServiceProvider);
            var dbContext = await dbContextFactory.CreateDbContextAsync();
            
            BehaviorId[] result = Array.Empty<BehaviorId>();

            try
            {
                var contexts = await dbContext.Contexts_v1.FindByClassesAsync(behaviorClasses);

                result = contexts.Select(c => StateflowsJsonConverter.DeserializeObject<BehaviorId>(c.BehaviorId)).ToArray();
            }
            catch (Exception e)
            {
                logger.LogError(e, LogTemplates.ExceptionLogTemplate, typeof(EntityFrameworkCoreStorage<TDbContext>).FullName, nameof(GetAllContextIdsAsync), e.GetType().Name, e.Message);
            }

            return result;
        }

        public async Task<IEnumerable<StateflowsContext>> GetTimeTriggeredContextsAsync(IEnumerable<BehaviorClass> behaviorClasses)
        {            
            await using var scope = serviceProvider.CreateAsyncScope();
            var dbContextFactory = scope.ServiceProvider.GetService<IDbContextFactory<TDbContext>>() ?? new DbContextFactory<TDbContext>(scope.ServiceProvider);
            var dbContext = await dbContextFactory.CreateDbContextAsync();
            
            var result = Array.Empty<StateflowsContext>();

            try
            {
                var contexts = dbContext.Contexts_v1.FindByTriggerTime(behaviorClasses);

                result = contexts.Select(c => StateflowsJsonConverter.DeserializeObject<StateflowsContext>(c.Data ?? string.Empty)).ToArray();
            }
            catch (Exception e)
            {
                logger.LogError(e, LogTemplates.ExceptionLogTemplate, typeof(EntityFrameworkCoreStorage<TDbContext>).FullName, nameof(GetTimeTriggeredContextsAsync), e.GetType().Name, e.Message);
            }

            return result;
        }

        public async Task<IEnumerable<StateflowsContext>> GetStartupTriggeredContextsAsync(IEnumerable<BehaviorClass> behaviorClasses)
        {            
            await using var scope = serviceProvider.CreateAsyncScope();
            var dbContextFactory = scope.ServiceProvider.GetService<IDbContextFactory<TDbContext>>() ?? new DbContextFactory<TDbContext>(scope.ServiceProvider);
            var dbContext = await dbContextFactory.CreateDbContextAsync();
            
            var result = Array.Empty<StateflowsContext>();

            try
            {
                var contexts = dbContext.Contexts_v1.FindByTriggerOnStartup(behaviorClasses);

                result = contexts.Select(c => StateflowsJsonConverter.DeserializeObject<StateflowsContext>(c.Data ?? string.Empty)).ToArray();
            }
            catch (Exception e)
            {
                logger.LogError(e, LogTemplates.ExceptionLogTemplate, typeof(EntityFrameworkCoreStorage<TDbContext>).FullName, nameof(GetTimeTriggeredContextsAsync), e.GetType().Name, e.Message);
            }

            return result;
        }

        public void Dispose()
        { }
    }
}
