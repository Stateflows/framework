using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Stateflows.Common.Extensions;
using Stateflows.Common.Utilities;
using Stateflows.Common.Interfaces;
using Stateflows.Storage.EntityFrameworkCore.EntityFrameworkCore;
using Stateflows.Storage.EntityFrameworkCore.EntityFrameworkCore.Entities;

namespace Stateflows.Storage.EntityFrameworkCore.Stateflows
{
    internal class EntityFrameworkCoreValueStorage<TDbContext>(IServiceProvider serviceProvider) : IStateflowsValueStorage
        where TDbContext : DbContext, IStateflowsDbContext_v1
    {
        public async Task SetAsync<T>(BehaviorId behaviorId, string key, T value)
        {
            await using var scope = serviceProvider.CreateAsyncScope();
            var dbContextFactory = scope.ServiceProvider.GetService<IDbContextFactory<TDbContext>>() ?? new DbContextFactory<TDbContext>(scope.ServiceProvider);
            var dbContext = await dbContextFactory.CreateDbContextAsync();
            
            var entry = await dbContext.Values_v1.Where(v =>
                v.BehaviorType == behaviorId.Type &&
                v.BehaviorName == behaviorId.Name &&
                v.BehaviorInstance == behaviorId.Instance &&
                v.Key == key).FirstOrDefaultAsync();

            if (entry == null)
            {
                entry = new Value_v1(behaviorId.Type, behaviorId.Name, behaviorId.Instance, key, string.Empty);
                dbContext.Values_v1.Add(entry);
            }

            entry.Value = StateflowsJsonConverter.SerializePolymorphicObject(value);

            await dbContext.SaveChangesAsync();
                
            dbContext.ChangeTracker.Clear();
        }

        public async Task<bool> IsSetAsync(BehaviorId behaviorId, string key)
        {
            await using var scope = serviceProvider.CreateAsyncScope();
            var dbContextFactory = scope.ServiceProvider.GetService<IDbContextFactory<TDbContext>>() ?? new DbContextFactory<TDbContext>(scope.ServiceProvider);
            var dbContext = await dbContextFactory.CreateDbContextAsync();
            return await dbContext.Values_v1.AnyAsync(v =>
                v.BehaviorType == behaviorId.Type &&
                v.BehaviorName == behaviorId.Name &&
                v.BehaviorInstance == behaviorId.Instance &&
                v.Key == key);
        }

        public async Task<bool> HasAnyPrefixedAsync(BehaviorId behaviorId, string prefix)
        {
            await using var scope = serviceProvider.CreateAsyncScope();
            var dbContextFactory = scope.ServiceProvider.GetService<IDbContextFactory<TDbContext>>() ?? new DbContextFactory<TDbContext>(scope.ServiceProvider);
            var dbContext = await dbContextFactory.CreateDbContextAsync();
            return await dbContext.Values_v1.AnyAsync(v =>
                v.BehaviorType == behaviorId.Type &&
                v.BehaviorName == behaviorId.Name &&
                v.BehaviorInstance == behaviorId.Instance &&
                v.Key.StartsWith(prefix));
        }

        public async Task<(bool Success, T? Value)> TryGetAsync<T>(BehaviorId behaviorId, string key)
        {
            (bool Success, T Value) result = (false, default(T));
            
            await using var scope = serviceProvider.CreateAsyncScope();
            var dbContextFactory = scope.ServiceProvider.GetService<IDbContextFactory<TDbContext>>() ?? new DbContextFactory<TDbContext>(scope.ServiceProvider);
            var dbContext = await dbContextFactory.CreateDbContextAsync();
            
            var entry = await dbContext.Values_v1.Where(v =>
                v.BehaviorType == behaviorId.Type &&
                v.BehaviorName == behaviorId.Name &&
                v.BehaviorInstance == behaviorId.Instance &&
                v.Key == key).FirstOrDefaultAsync();

            if (entry == null)
            {
                return result;
            }

            var type = typeof(T);
            var deserializedData = type.IsPrimitiveOrNullablePrimitive()
                ? StateflowsJsonConverter.ParseStringToTypedValue<T>(entry.Value)
                : type.IsEnum
                    ? StateflowsJsonConverter.ParseStringToEnum<T>(entry.Value)
                    : StateflowsJsonConverter.DeserializeObject(entry.Value);

            if (type.IsNullable() && deserializedData is null)
            {
                result.Success = true;
            }
            else
            {
                if (deserializedData is T t)
                {
                    result.Value = t;
                    result.Success = true;
                }
            }
            
            return result;
        }

        public async Task<T> GetOrDefaultAsync<T>(BehaviorId behaviorId, string key, T defaultValue = default)
        {
            var result = defaultValue;
            
            await using var scope = serviceProvider.CreateAsyncScope();
            var dbContextFactory = scope.ServiceProvider.GetService<IDbContextFactory<TDbContext>>() ?? new DbContextFactory<TDbContext>(scope.ServiceProvider);
            var dbContext = await dbContextFactory.CreateDbContextAsync();
            
            var entry = await dbContext.Values_v1.Where(v =>
                v.BehaviorType == behaviorId.Type &&
                v.BehaviorName == behaviorId.Name &&
                v.BehaviorInstance == behaviorId.Instance &&
                v.Key == key).FirstOrDefaultAsync();

            if (entry == null)
            {
                return result;
            }

            var type = typeof(T);
            var deserializedData = type.IsPrimitiveOrNullablePrimitive()
                ? StateflowsJsonConverter.ParseStringToTypedValue<T>(entry.Value)
                : type.IsEnum
                    ? StateflowsJsonConverter.ParseStringToEnum<T>(entry.Value)
                    : StateflowsJsonConverter.DeserializeObject(entry.Value);

            if (!(type.IsNullable() && deserializedData is null) && deserializedData is T t)
            {
                result = t;
            }
            
            return result;
        }

        public async Task UpdateAsync<T>(BehaviorId behaviorId, string key, Func<T, T> valueUpdater, T defaultValue = default)
        {
            var result = defaultValue;
            
            await using var scope = serviceProvider.CreateAsyncScope();
            var dbContextFactory = scope.ServiceProvider.GetService<IDbContextFactory<TDbContext>>() ?? new DbContextFactory<TDbContext>(scope.ServiceProvider);
            var dbContext = await dbContextFactory.CreateDbContextAsync();
            
            var entry = await dbContext.Values_v1.Where(v =>
                v.BehaviorType == behaviorId.Type &&
                v.BehaviorName == behaviorId.Name &&
                v.BehaviorInstance == behaviorId.Instance &&
                v.Key == key).FirstOrDefaultAsync();

            if (entry == null)
            {
                entry = new Value_v1(behaviorId.Type, behaviorId.Name, behaviorId.Instance, key, string.Empty);
                dbContext.Values_v1.Add(entry);
            }
            else
            {
                var type = typeof(T);
                var deserializedData = type.IsPrimitiveOrNullablePrimitive()
                    ? StateflowsJsonConverter.ParseStringToTypedValue<T>(entry.Value)
                    : type.IsEnum
                        ? StateflowsJsonConverter.ParseStringToEnum<T>(entry.Value)
                        : StateflowsJsonConverter.DeserializeObject(entry.Value);

                if (!(type.IsNullable() && deserializedData is null) && deserializedData is T t)
                {
                    result = t;
                }
            }

            result = valueUpdater.Invoke(result);
            
            entry.Value = StateflowsJsonConverter.SerializePolymorphicObject(result);

            await dbContext.SaveChangesAsync();
                
            dbContext.ChangeTracker.Clear();
        }

        public async Task RemoveAsync(BehaviorId behaviorId, string key)
        {
            
            await using var scope = serviceProvider.CreateAsyncScope();
            var dbContextFactory = scope.ServiceProvider.GetService<IDbContextFactory<TDbContext>>() ?? new DbContextFactory<TDbContext>(scope.ServiceProvider);
            var dbContext = await dbContextFactory.CreateDbContextAsync();
            await dbContext.Values_v1.Where(v =>
                v.BehaviorType == behaviorId.Type &&
                v.BehaviorName == behaviorId.Name &&
                v.BehaviorInstance == behaviorId.Instance &&
                v.Key == key).ExecuteDeleteAsync();
        }

        public async Task RemovePrefixedAsync(BehaviorId behaviorId, string prefix)
        {
            
            await using var scope = serviceProvider.CreateAsyncScope();
            var dbContextFactory = scope.ServiceProvider.GetService<IDbContextFactory<TDbContext>>() ?? new DbContextFactory<TDbContext>(scope.ServiceProvider);
            var dbContext = await dbContextFactory.CreateDbContextAsync();
            await dbContext.Values_v1.Where(v =>
                v.BehaviorType == behaviorId.Type &&
                v.BehaviorName == behaviorId.Name &&
                v.BehaviorInstance == behaviorId.Instance &&
                v.Key.StartsWith(prefix)).ExecuteDeleteAsync();
        }

        public async Task ClearAsync(BehaviorId behaviorId)
        {
            
            await using var scope = serviceProvider.CreateAsyncScope();
            var dbContextFactory = scope.ServiceProvider.GetService<IDbContextFactory<TDbContext>>() ?? new DbContextFactory<TDbContext>(scope.ServiceProvider);
            var dbContext = await dbContextFactory.CreateDbContextAsync();
            await dbContext.Values_v1.Where(v =>
                v.BehaviorType == behaviorId.Type &&
                v.BehaviorName == behaviorId.Name &&
                v.BehaviorInstance == behaviorId.Instance).ExecuteDeleteAsync();
        }

        public void Dispose()
        { }
    }
}
