using System;
using System.Linq;
using System.Reflection;
using System.Diagnostics;
using System.Collections.Generic;
using System.Threading.Tasks;
using Stateflows.Common;
using Stateflows.Common.Classes;
using Stateflows.Common.Context;
using Stateflows.Common.Interfaces;
using Stateflows.Common.Registration.Builders;
using Stateflows.Entities.Models;
using Stateflows.Entities.Registration.Builders;
using Stateflows.Entities.Registration.Interfaces;

namespace Stateflows.Entities.Registration
{
    internal class EntitiesRegister(StateflowsBuilder stateflowsBuilder) : IEntitiesRegister, IOwnedRegistration
    {
        public readonly Dictionary<string, EntityRegistration> Entities = [];

        private readonly Dictionary<string, int> CurrentVersions = [];

        public readonly List<EntityObserverFactoryAsync> GlobalObserverFactories = [];

        public readonly List<EntityInterceptorFactoryAsync> GlobalInterceptorFactories = [];

        public readonly List<EntityExceptionHandlerFactoryAsync> GlobalExceptionHandlerFactories = [];

        private readonly MethodInfo EntityTypeAddedAsyncMethod =
            typeof(IEntityVisitor).GetMethod(nameof(IEntityVisitor.EntityTypeAddedAsync));

        public BehaviorClass? OwnerClass { get; set; }

        public BehaviorClass? ParentClass { get; set; }

        private static void RegisterEntity<TTemplate>(Type entityType, IEntityBuilder<TTemplate> entityBuilder)
            where TTemplate : class
        {
            var staticBuildMethod = entityType.GetMethod(
                nameof(IEntity<TTemplate>.Build),
                BindingFlags.Public | BindingFlags.Static,
                binder: null,
                types: [typeof(IEntityBuilder<TTemplate>)],
                modifiers: null
            );

            staticBuildMethod?.Invoke(null, [entityBuilder]);
        }

        private bool IsNewestVersion(string entityName, int version)
        {
            var result = false;

            if (CurrentVersions.TryGetValue(entityName, out var currentVersion))
            {
                if (currentVersion < version)
                {
                    result = true;
                    CurrentVersions[entityName] = version;
                }
            }
            else
            {
                result = true;
                CurrentVersions[entityName] = version;
            }

            return result;
        }

        [DebuggerHidden]
        public void AddEntity<TTemplate>(string entityName, int version, EntityBuildAction<TTemplate> buildAction = null)
            where TTemplate : class
        {
            var key = $"{entityName}.{version}";
            var currentKey = $"{entityName}.current";

            if (Entities.ContainsKey(key))
            {
                throw new InvalidOperationException($"Entity '{entityName}' with version '{version}' is already registered");
            }

            var entityRegistration = new EntityRegistration()
            {
                Name = entityName,
                Version = version,
                OwnerClass = OwnerClass,
                ParentClass = ParentClass,
                Model = new EntityModel<TTemplate>(),
                StateflowsBuilder = stateflowsBuilder
            };

            entityRegistration.VisitingTasks.Add(async visitor =>
            {
                await visitor.EntityAddingAsync<TTemplate>(entityName, version, entityRegistration.OwnerClass, entityRegistration.ParentClass);
            });

            buildAction?.Invoke(new EntityBuilder<TTemplate>(entityRegistration));

            entityRegistration.VisitingTasks.Add(async visitor =>
            {
                await visitor.EntityAddedAsync<TTemplate>(entityName, version);
            });

            Entities.Add(key, entityRegistration);

            if (IsNewestVersion(entityName, version))
            {
                Entities[currentKey] = entityRegistration;
            }
        }

        [DebuggerHidden]
        public void AddEntity<TTemplate>(string entityName, int version, Type entityType, EntityBuildAction<TTemplate>? buildAction = null)
            where TTemplate : class
        {
            if (!typeof(IEntity<TTemplate>).IsAssignableFrom(entityType))
            {
                throw new InvalidOperationException($"Type '{entityType.FullName}' does not implement '{typeof(IEntity<TTemplate>).FullName}'");
            }

            var key = $"{entityName}.{version}";
            var currentKey = $"{entityName}.current";

            if (Entities.ContainsKey(key))
            {
                throw new InvalidOperationException($"Entity '{entityName}' with version '{version}' is already registered");
            }

            var entityRegistration = new EntityRegistration()
            {
                Name = entityName,
                Version = version,
                EntityType = entityType,
                OwnerClass = OwnerClass,
                ParentClass = ParentClass,
                Model = new EntityModel<TTemplate>(),
                StateflowsBuilder = stateflowsBuilder
            };

            entityRegistration.VisitingTasks.Add(async visitor =>
            {
                await visitor.EntityAddingAsync<TTemplate>(entityName, version, entityRegistration.OwnerClass, entityRegistration.ParentClass);
            });
            
            var builder = new EntityBuilder<TTemplate>(entityRegistration);
            RegisterEntity(entityType, builder);
            buildAction?.Invoke(builder);

            var method = EntityTypeAddedAsyncMethod.MakeGenericMethod(typeof(TTemplate), entityType);

            entityRegistration.VisitingTasks.Add(async visitor =>
            {
                await visitor.EntityAddedAsync<TTemplate>(entityName, version);
                await (Task)method.Invoke(visitor, [entityName, version]);
            });

            Entities.Add(key, entityRegistration);

            if (IsNewestVersion(entityName, version))
            {
                Entities[currentKey] = entityRegistration;
            }
        }

        [DebuggerHidden]
        public void AddEntity<TTemplate, TEntity>(string? entityName = null, int version = 1, EntityBuildAction<TTemplate>? buildAction = null)
            where TTemplate : class
            where TEntity : class, IEntity<TTemplate>
            => AddEntity(entityName ?? Entity<TTemplate, TEntity>.Name, version, typeof(TEntity), buildAction);

        [DebuggerHidden]
        public void AddObserver(EntityObserverFactoryAsync observerFactoryAsync)
            => GlobalObserverFactories.Add(observerFactoryAsync);

        [DebuggerHidden]
        public void AddObserver<TObserver>()
            where TObserver : class, IEntityObserver
            => GlobalObserverFactories.Add(async (serviceProvider, context) =>
                await CreateModelElementInstanceAsync<TObserver>(serviceProvider, context, "observer")
            );

        [DebuggerHidden]
        public void AddInterceptor(EntityInterceptorFactoryAsync interceptorFactoryAsync)
            => GlobalInterceptorFactories.Add(interceptorFactoryAsync);

        [DebuggerHidden]
        public void AddInterceptor<TInterceptor>()
            where TInterceptor : class, IEntityInterceptor
            => GlobalInterceptorFactories.Add(async (serviceProvider, context) =>
                await CreateModelElementInstanceAsync<TInterceptor>(serviceProvider, context, "interceptor")
            );

        [DebuggerHidden]
        public void AddExceptionHandler(EntityExceptionHandlerFactoryAsync exceptionHandlerFactoryAsync)
            => GlobalExceptionHandlerFactories.Add(exceptionHandlerFactoryAsync);

        [DebuggerHidden]
        public void AddExceptionHandler<THandler>()
            where THandler : class, IEntityExceptionHandler
            => GlobalExceptionHandlerFactories.Add(async (serviceProvider, context) =>
                await CreateModelElementInstanceAsync<THandler>(serviceProvider, context, "exception handler")
            );

        internal static async Task<T> CreateModelElementInstanceAsync<T>(IServiceProvider serviceProvider, IBehaviorActionContext context, string serviceKind)
        {
            // AsyncLocal values set here do not leak back to the caller once this async method completes.
            ContextValues.GlobalValuesHolder.Value = context.Behavior.Values;
            ContextValues.StateValuesHolder.Value = null;
            ContextValues.ParentStateValuesHolder.Value = null;
            ContextValues.SourceStateValuesHolder.Value = null;
            ContextValues.TargetStateValuesHolder.Value = null;

            CommonContextHolder.BehaviorContext.Value = context.Behavior;
            CommonContextHolder.ParentBehaviorContext.Value = context.TryGetParentBehaviorContext(out var parentBehaviorContext)
                ? parentBehaviorContext
                : null;
            CommonContextHolder.OwnerBehaviorContext.Value = context.TryGetOwnerBehaviorContext(out var ownerBehaviorContext)
                ? ownerBehaviorContext
                : null;
            CommonContextHolder.ExecutionContext.Value = context;

            return await StateflowsActivator.CreateModelElementInstanceAsync<T>(serviceProvider, serviceKind);
        }

        public async Task VisitEntitiesAsync(IEntityVisitor visitor)
        {
            var tasks = Entities
                .Where((item, index) => !item.Key.EndsWith(".current"))
                .Select(item => item.Value)
                .SelectMany(graph => graph.VisitingTasks);

            foreach (var task in tasks)
            {
                await task(visitor);
            }
            
            // foreach (var entity in Entities.Where(item => !item.Key.EndsWith(".current")).Select(item => item.Value))
            // {
            //     await entity.VisitingTasks(visitor);
            // }
        }

        public async Task VisitEntityAsync(string entityName, int version, IEntityVisitor visitor)
        {
            var tasks = Entities
                .Where((item, index) => item.Key == $"{entityName}.{version}")
                .Select(item => item.Value)
                .SelectMany(graph => graph.VisitingTasks);

            foreach (var task in tasks)
            {
                await task(visitor);
            }
            
            // foreach (var entity in Entities.Where(item => item.Key == $"{entityName}.{version}").Select(item => item.Value))
            // {
            //     await entity.VisitingTasks(visitor);
            // }
        }
    }
}

