using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Stateflows.Common;
using Stateflows.Common.Extensions;
using Stateflows.Common.Interfaces;
using Stateflows.Common.Utilities;
using Stateflows.Storage.EntityFrameworkCore.EntityFrameworkCore;
using Stateflows.Storage.EntityFrameworkCore.EntityFrameworkCore.Entities;

namespace Stateflows.Storage.EntityFrameworkCore.Stateflows
{
    internal class EntityFrameworkCoreValueStorage<TDbContext>(IServiceProvider serviceProvider) : IStateflowsValueStorage
        where TDbContext : DbContext, IStateflowsDbContext_v1
    {
        public async Task<IReadOnlyDictionary<string, StateflowsValue>> LoadAsync(BehaviorId behaviorId)
        {
            await using var scope = serviceProvider.CreateAsyncScope();
            var dbContextFactory = scope.ServiceProvider.GetService<IDbContextFactory<TDbContext>>() ?? new DbContextFactory<TDbContext>(scope.ServiceProvider);
            var dbContext = await dbContextFactory.CreateDbContextAsync();
            
            var entries = await dbContext.Values_v1
                .AsNoTracking()
                .Where(v =>
                    v.BehaviorType == behaviorId.Type &&
                    v.BehaviorName == behaviorId.Name &&
                    v.BehaviorInstance == behaviorId.Instance
                )
                .ToArrayAsync();
            
            var result = new Dictionary<string, StateflowsValue>();
            foreach (var entry in entries)
            {
                result[entry.Key] = new StateflowsValue()
                {
                    Name = entry.Key,
                    Value = entry.Value,
                    Version = entry.Version,
                };
            }

            return result;
        }

        public Task SaveAsync(BehaviorId behaviorId, IReadOnlyDictionary<string, StateflowsValue> values)
        {
            throw new NotImplementedException();
        }

        public Task<IReadOnlyDictionary<string, StateflowsValue>> SaveAndLoadAsync(BehaviorId behaviorId, IReadOnlyDictionary<string, StateflowsValue> values)
        {
            throw new NotImplementedException();
        }

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

            entry.Value = typeof(T) == typeof(Guid)
                ? ((Guid)(object)value).ToString()
                : StateflowsJsonConverter.SerializePolymorphicObject(value);

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
                    : type == typeof(Guid)
                        ? Guid.Parse(entry.Value)
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
                    : type == typeof(Guid) && (entry.Value != null)
                        ? Guid.Parse(entry.Value)
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
                        : type == typeof(Guid)
                            ? Guid.Parse(entry.Value)
                            : StateflowsJsonConverter.DeserializeObject(entry.Value);

                if (!(type.IsNullable() && deserializedData is null) && deserializedData is T t)
                {
                    result = t;
                }
            }

            result = valueUpdater.Invoke(result);
            
            entry.Value = typeof(T) == typeof(Guid)
                ? ((Guid)(object)result).ToString()
                : StateflowsJsonConverter.SerializePolymorphicObject(result);

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
