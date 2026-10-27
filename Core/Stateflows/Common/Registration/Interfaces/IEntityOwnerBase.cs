using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Stateflows.Common.Classes;
using Stateflows.Entities;
using Stateflows.Entities.Registration.Interfaces;

namespace Stateflows.Common.Registration.Interfaces
{
    internal class PropertyProjectionInterceptor : BehaviorInterceptor
    {
        private static Dictionary<BehaviorClass, Func<IBehaviorActionContext, BehaviorInfo, Task>> InterceptionTasks { get; } = new();
        
        private static async Task InterceptBehaviorInfo<TEvent>(IBehaviorActionContext context, TEvent @event)
        {
            if (@event is BehaviorInfo behaviorInfo)
            {
                if (InterceptionTasks.TryGetValue(context.Behavior.Id.BehaviorClass, out var interceptionTask))
                {
                    await interceptionTask(context, behaviorInfo);
                }
            }
        }

        internal static void AddPropertyProjection<TProjection>(BehaviorClass behaviorClass, string entityName)
        {
            InterceptionTasks[behaviorClass] = (context, behaviorInfo) => EnhanceBehaviorInfo<TProjection>(context, behaviorInfo, entityName);
        }
        
        private static async Task EnhanceBehaviorInfo<TProjection>(IBehaviorActionContext context, BehaviorInfo behaviorInfo, string entityName)
        {
            var (success, projection) = await context.Behavior.TryGetProjectionAsync<TProjection>(entityName);
            
            if (success && projection != null)
            {
                var projectionType = projection.GetType();

                foreach (var property in projectionType.GetProperties(BindingFlags.Instance | BindingFlags.Public))
                {
                    if (property.GetMethod != null && property.GetIndexParameters().Length == 0)
                    {
                        behaviorInfo.Properties.Add(property.Name, property.GetValue(projection)!);
                    }
                }

                foreach (var field in projectionType.GetFields(BindingFlags.Instance | BindingFlags.Public))
                {
                    behaviorInfo.Properties.Add(field.Name, field.GetValue(projection)!);
                }
            }
        }
        
        public override Task NotificationPublishedAsync<TNotification>(IBehaviorActionContext context, TNotification notification,
            IDictionary<string, EventHeader> headers)
            => InterceptBehaviorInfo(context, notification);

        public override Task RequestRespondedAsync<TRequest, TResponse>(IBehaviorActionContext context, TRequest request, TResponse response)
            => InterceptBehaviorInfo(context, response);
    }
    
    /// <summary>Provides fluent operations for registering entities owned by a behavior.</summary>
    /// <typeparam name="TReturn">The owner builder type returned for continued configuration.</typeparam>
    public interface IEntityOwnerBase<out TReturn>
    {
        /// <summary>Registers an entity template under an explicit entity name.</summary>
        /// <typeparam name="TTemplate">The entity data template type.</typeparam>
        /// <param name="entityName">The entity name used to identify this registration.</param>
        /// <param name="buildAction">Optional callback that configures the entity.</param>
        /// <returns>The owner builder for fluent registration.</returns>
        TReturn AddEntity<TTemplate>(string entityName, EntityBuildAction<TTemplate>? buildAction = null)
            where TTemplate : class;

        /// <summary>Registers an entity template under its default entity name.</summary>
        /// <typeparam name="TTemplate">The entity data template type.</typeparam>
        /// <param name="buildAction">Optional callback that configures the entity.</param>
        /// <returns>The owner builder for fluent registration.</returns>
        TReturn AddEntity<TTemplate>(EntityBuildAction<TTemplate>? buildAction = null)
            where TTemplate : class
            => AddEntity<TTemplate>(Entity<TTemplate>.Name, buildAction);

        /// <summary>Registers a custom entity implementation under an explicit entity name.</summary>
        /// <typeparam name="TTemplate">The entity data template type.</typeparam>
        /// <typeparam name="TEntity">The entity implementation type.</typeparam>
        /// <param name="entityName">The entity name used to identify this registration.</param>
        /// <param name="buildAction">Optional callback that configures the entity.</param>
        /// <returns>The owner builder for fluent registration.</returns>
        TReturn AddEntity<TTemplate, TEntity>(string entityName, EntityBuildAction<TTemplate>? buildAction = null)
            where TTemplate : class
            where TEntity : class, IEntity<TTemplate>;

        /// <summary>Registers a custom entity implementation under its default entity name.</summary>
        /// <typeparam name="TTemplate">The entity data template type.</typeparam>
        /// <typeparam name="TEntity">The entity implementation type.</typeparam>
        /// <param name="buildAction">Optional callback that configures the entity.</param>
        /// <returns>The owner builder for fluent registration.</returns>
        TReturn AddEntity<TTemplate, TEntity>(EntityBuildAction<TTemplate>? buildAction = null)
            where TTemplate : class
            where TEntity : class, IEntity<TTemplate>
            => AddEntity<TTemplate, TEntity>(Entity<TTemplate, TEntity>.Name, buildAction);
        
        /// <summary>Uses a projection's public members as properties in behavior information.</summary>
        /// <typeparam name="TProjection">The projection type.</typeparam>
        /// <param name="entityName">The entity whose projection is exposed.</param>
        /// <returns>The owner builder for fluent registration.</returns>
        TReturn SetPropertiesProjection<TProjection>(string entityName)
            where TProjection : class;
        
        /// <summary>Uses a template's default entity projection as behavior information properties.</summary>
        /// <typeparam name="TTemplate">The entity data template type.</typeparam>
        /// <typeparam name="TProjection">The projection type.</typeparam>
        /// <returns>The owner builder for fluent registration.</returns>
        TReturn SetPropertiesProjection<TTemplate, TProjection>()
            where TTemplate : class
            where TProjection : class
            => SetPropertiesProjection<TProjection>(Entity<TTemplate>.Name);
        
        /// <summary>Uses the custom entity's default projection as behavior information properties.</summary>
        /// <typeparam name="TTemplate">The entity data template type.</typeparam>
        /// <typeparam name="TEntity">The entity implementation type.</typeparam>
        /// <typeparam name="TProjection">The projection type.</typeparam>
        /// <returns>The owner builder for fluent registration.</returns>
        TReturn SetPropertiesProjection<TTemplate, TEntity, TProjection>()
            where TTemplate : class
            where TEntity : class, IEntity<TTemplate>
            where TProjection : class
            => SetPropertiesProjection<TProjection>(Entity<TTemplate, TEntity>.Name);
    }
}
