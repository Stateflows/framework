using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Stateflows.Common;
using Stateflows.Common.Extensions;
using Stateflows.Common.Utilities;
using Stateflows.Common.Interfaces;
using Stateflows.Storage.EntityFrameworkCore.EntityFrameworkCore;
using Stateflows.Storage.EntityFrameworkCore.EntityFrameworkCore.Entities;

namespace Stateflows.Storage.EntityFrameworkCore.Stateflows
{
    internal class EntityFrameworkCoreValueStorage : IStateflowsValueStorage
    {
        private readonly IStateflowsDbContext_v1 DbContext;
        private readonly ILogger<EntityFrameworkCoreNotificationsStorage> Logger;

        public EntityFrameworkCoreValueStorage(IStateflowsDbContext_v1 dbContext, ILogger<EntityFrameworkCoreNotificationsStorage> logger)
        {
            DbContext = dbContext;
            Logger = logger;
        }
        
        public Task SaveNotificationsAsync(BehaviorId behaviorId, EventHolder[] notifications)
        {
            DbContext.Notifications_v1.AddRange(notifications.Select(n => new Notification_v1(n)));
            return DbContext.SaveChangesAsync();
        }

        public async Task<IEnumerable<EventHolder>> GetNotificationsAsync(BehaviorId behaviorId, string[] notificationNames, DateTime lastNotificationCheck)
        {
            var notifications = await DbContext.Notifications_v1.Where(n =>
                n.SenderType == behaviorId.Type &&
                n.SenderName == behaviorId.Name &&
                n.SenderInstance == behaviorId.Instance &&
                notificationNames.Contains(n.Name) &&
                (
                    n.SentAt.AddSeconds(n.TimeToLive) >= lastNotificationCheck ||
                    n.Retained
                )
            ).ToArrayAsync();

            notifications = notifications.Except(notifications.Where(n => 
                n.Retained &&
                notifications.Any(m =>
                    m.Retained &&
                    m.SenderType == n.SenderType &&
                    m.SenderName == n.SenderName &&
                    m.SenderInstance == n.SenderInstance &&
                    m.Name == n.Name &&
                    m.SentAt < n.SentAt
                )
            )).ToArray();

            var result = notifications.Select(n => (EventHolder)StateflowsJsonConverter.DeserializeObject(n.Data));
            
            return result;
        }

        public async Task SetAsync<T>(BehaviorId behaviorId, string key, T value)
        {
            var entry = await DbContext.Values_v1.Where(v =>
                v.BehaviorType == behaviorId.Type &&
                v.BehaviorName == behaviorId.Name &&
                v.BehaviorInstance == behaviorId.Instance &&
                v.Key == key).FirstOrDefaultAsync();

            if (entry == null)
            {
                entry = new Value_v1(behaviorId.Type, behaviorId.Name, behaviorId.Instance, key, string.Empty);
                DbContext.Values_v1.Add(entry);
            }

            entry.Value = StateflowsJsonConverter.SerializePolymorphicObject(value);

            await DbContext.SaveChangesAsync();
        }

        public Task<bool> IsSetAsync(BehaviorId behaviorId, string key)
            => DbContext.Values_v1.AnyAsync(v =>
                v.BehaviorType == behaviorId.Type &&
                v.BehaviorName == behaviorId.Name &&
                v.BehaviorInstance == behaviorId.Instance &&
                v.Key == key);

        public Task<bool> HasAnyPrefixedAsync(BehaviorId behaviorId, string prefix)
            => DbContext.Values_v1.AnyAsync(v =>
                v.BehaviorType == behaviorId.Type &&
                v.BehaviorName == behaviorId.Name &&
                v.BehaviorInstance == behaviorId.Instance &&
                v.Key.StartsWith(prefix));

        public async Task<(bool Success, T Value)> TryGetAsync<T>(BehaviorId behaviorId, string key)
        {
            (bool Success, T Value) result = (false, default(T));
            
            var entry = await DbContext.Values_v1.Where(v =>
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
            
            var entry = await DbContext.Values_v1.Where(v =>
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
            
            var entry = await DbContext.Values_v1.Where(v =>
                v.BehaviorType == behaviorId.Type &&
                v.BehaviorName == behaviorId.Name &&
                v.BehaviorInstance == behaviorId.Instance &&
                v.Key == key).FirstOrDefaultAsync();

            if (entry == null)
            {
                entry = new Value_v1(behaviorId.Type, behaviorId.Name, behaviorId.Instance, key, string.Empty);
                DbContext.Values_v1.Add(entry);
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

            await DbContext.SaveChangesAsync();
        }

        public Task RemoveAsync(BehaviorId behaviorId, string key)
            => DbContext.Values_v1.Where(v =>
                v.BehaviorType == behaviorId.Type &&
                v.BehaviorName == behaviorId.Name &&
                v.BehaviorInstance == behaviorId.Instance &&
                v.Key == key).ExecuteDeleteAsync();

        public Task RemovePrefixedAsync(BehaviorId behaviorId, string prefix)
            => DbContext.Values_v1.Where(v =>
                v.BehaviorType == behaviorId.Type &&
                v.BehaviorName == behaviorId.Name &&
                v.BehaviorInstance == behaviorId.Instance &&
                v.Key.StartsWith(prefix)).ExecuteDeleteAsync();

        public Task ClearAsync(BehaviorId behaviorId)
            => DbContext.Values_v1.Where(v =>
                v.BehaviorType == behaviorId.Type &&
                v.BehaviorName == behaviorId.Name &&
                v.BehaviorInstance == behaviorId.Instance).ExecuteDeleteAsync();
    }
}
