using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Stateflows.Entities.Attributes;
using Stateflows.Entities.Engine;
using Stateflows.Entities.Enums;
using Stateflows.Entities.Models;
using Stateflows.Entities.Registration.Interfaces;

namespace Stateflows.Entities.Registration.Builders
{
    internal class EntityBuilder<TTemplate>(EntityRegistration registration) : IEntityBuilder<TTemplate>
        where TTemplate : class
    {
        public EntityRegistration Registration { get; } = registration;

        private Dictionary<string, object?> GetTrackedFieldValuesSnapshot(Dictionary<string, object> values)
            => Registration.Model.Fields.Values.ToDictionary(
                field => field.Name,
                field => values.TryGetValue(field.Name.GetFieldKey(), out var value)
                    ? value
                    : null
            );

        private static IReadOnlyCollection<string> GetChangedFieldNames(
            IReadOnlyDictionary<string, object?> oldValues,
            IReadOnlyDictionary<string, object?> newValues)
            => newValues
                .Where(value =>
                    !oldValues.TryGetValue(value.Key, out var oldValue) ||
                    !Equals(oldValue, value.Value)
                )
                .Select(value => value.Key)
                .ToArray();

        public IEntityBuilder<TTemplate> AddDefaultInitializer(DefaultInitializerAction<TTemplate> action)
        {
            // Registration.Model.DefaultInitializer.Add(action);
            Registration.Model.DefaultInitializerInvoke.Add(values =>
            {
                var (_, entity) = EntityProxy<TTemplate>.Create(values, Registration.Model);
                action(new DefaultEntityInitializationContext<TTemplate>(entity));
            });
            
            return this;
        }

        public IEntityBuilder<TTemplate> AddInitializer<TInitializationEvent>(InitializerAction<TTemplate, TInitializationEvent> action)
        {
            Registration.Model.Initializers[typeof(TInitializationEvent)] = new InitializerModel
            {
                InitializationEventType = typeof(TInitializationEvent),
                InitializerAction = action,
                Invoke = (values, eventObj) =>
                {
                    var (_, entity) = EntityProxy<TTemplate>.Create(values, Registration.Model);
                    action(new EntityInitializationContext<TTemplate, TInitializationEvent>(
                        entity,
                        (TInitializationEvent)eventObj
                    ));
                }
            };

            Registration.VisitingTasks.Add(async visitor =>
            {
                await visitor.InitializerAddedAsync<TTemplate, TInitializationEvent>(Registration.Name, Registration.Version);
            });

            return this;
        }

        public IEntityBuilder<TTemplate> AddField<TField>(Expression<Func<TTemplate, TField>> fieldSelector, FieldBuildAction<TTemplate, TField> buildAction = null)
        {
            if (fieldSelector.Body is not MemberExpression memberExpression)
                throw new ArgumentException("Field selector must be a simple member access expression (e.g. t => t.MyField)", nameof(fieldSelector));

            var fieldName = memberExpression.Member.Name;

            if (Registration.Model.Fields.ContainsKey(fieldName))
                throw new InvalidOperationException($"Field '{fieldName}' is already registered in entity '{Registration.Name}'");

            var fieldModel = new FieldModel<TTemplate, TField>(Registration.Model, fieldName, FieldAccess.None, memberExpression.Member);
            
            Registration.Model.Fields.Add(fieldName, fieldModel);
            
            buildAction?.Invoke(new FieldBuilder<TTemplate, TField>(fieldModel));
            
            // Access may be configured after the field is added (attributes, FieldBuilder), so it is read at visit time.
            Registration.VisitingTasks.Add(async visitor =>
            {
                await visitor.FieldAddedAsync<TTemplate, TField>(
                    Registration.Name,
                    Registration.Version,
                    fieldName,
                    fieldModel.HasDefaultValue && fieldModel.DefaultValue is TField defaultValue ? defaultValue : default!,
                    fieldModel.IsComputed,
                    fieldModel.Access
                );
            });

            return this;
        }

        public IEntityBuilder<TTemplate> AddMutation<TMutationEvent>(MutationAction<TTemplate, TMutationEvent> mutationAction)
        {
            Registration.Model.Mutations[typeof(TMutationEvent)] = new MutationModel
            {
                MutationType = typeof(TMutationEvent),
                MutationAction = mutationAction,
                InvokeAsync = async (values, behaviorContext, inspection, eventObj) =>
                {
                    var oldValues = GetTrackedFieldValuesSnapshot(values);

                    var (_, entity) = EntityProxy<TTemplate>.Create(values, Registration.Model);
                    await inspection.MutationAsync(entity, (TMutationEvent)eventObj, () =>
                    {
                        mutationAction(new MutationContext<TTemplate, TMutationEvent>(entity, (TMutationEvent)eventObj));
                        return Task.CompletedTask;
                    });

                    var newValues = GetTrackedFieldValuesSnapshot(values);
                    var changedFieldNames = GetChangedFieldNames(oldValues, newValues);
                    if (changedFieldNames.Count != 0)
                    {
                        var allChangedFields = await EntityContextValues.StabilizeComputedFieldsAsync(Registration.Model, values, inspection, changedFieldNames);
                        await EntityContextValues.RefreshDependentProjectionsAsync(Registration.Model, values, behaviorContext, inspection, allChangedFields);
                    }
                }
            };
            
            Registration.VisitingTasks.Add(async visitor =>
            {
                await visitor.MutationAddedAsync<TTemplate, TMutationEvent>(Registration.Name, Registration.Version);
            });
            
            return this;
        }

        public IEntityBuilder<TTemplate> AddProjection<TProjectionTemplate>(ProjectionAction<TTemplate, TProjectionTemplate> projectionAction, PublishScope publishScope = PublishScope.None)
        {
            Registration.Model.Projections[typeof(TProjectionTemplate)] = new ProjectionModel
            {
                ProjectionType = typeof(TProjectionTemplate),
                ProjectionAction = projectionAction,
                InvokeAsync = async (values, inspection) =>
                {
                    var (proxy, entity) = EntityProxy<TTemplate>.Create(values, Registration.Model);
                    var observedEntity = inspection.HasOperationContexts
                        ? EntityProxy<TTemplate>.Create(values, Registration.Model).Item2
                        : entity;
                    var projection = await inspection.ProjectionAsync(
                        observedEntity, publishScope, () => Task.FromResult(projectionAction(entity)));
                    EntityContextValues.SetProjectionDependencies(values, typeof(TProjectionTemplate), proxy.ReadFields);
                    EntityContextValues.SetProjectionValue(values, typeof(TProjectionTemplate), projection);
                    return projection;
                },
                PublishScope = publishScope
            };
            
            Registration.VisitingTasks.Add(async visitor =>
            {
                await visitor.ProjectionAddedAsync<TTemplate, TProjectionTemplate>(Registration.Name, Registration.Version, publishScope);
            });
            
            return this;
        }

        public IEntityBuilder<TTemplate> SetResourceName(string resourceName)
        {
            Registration.Model.ResourceName = resourceName;
            var entityClass = new EntityClass(Registration.Name);
            
            if (Registration.StateflowsBuilder.ResourceNames.TryGetValue(resourceName ?? string.Empty, out var existingResourceName))
            {
                Registration.StateflowsBuilder.ResourcesByBehaviorClass[entityClass] = existingResourceName;
            }
            else
            {
                throw new InvalidOperationException($"Resource group {resourceName ?? string.Empty} does not exist");
            }

            return this;
        }

        public IEntityBuilder<TTemplate> AddObserver(EntityObserverFactoryAsync observerFactoryAsync)
        {
            Registration.ObserverFactories.Add(observerFactoryAsync);

            return this;
        }

        public IEntityBuilder<TTemplate> AddObserver<TObserver>()
            where TObserver : class, IEntityObserver
            => AddObserver(async (serviceProvider, context) =>
                await EntitiesRegister.CreateModelElementInstanceAsync<TObserver>(serviceProvider, context, "observer")
            );

        public IEntityBuilder<TTemplate> AddExceptionHandler(EntityExceptionHandlerFactoryAsync exceptionHandlerFactoryAsync)
        {
            Registration.ExceptionHandlerFactories.Add(exceptionHandlerFactoryAsync);

            return this;
        }

        public IEntityBuilder<TTemplate> AddExceptionHandler<THandler>()
            where THandler : class, IEntityExceptionHandler
            => AddExceptionHandler(async (serviceProvider, context) =>
                await EntitiesRegister.CreateModelElementInstanceAsync<THandler>(serviceProvider, context, "exception handler")
            );

        public IEntityBuilder<TTemplate> AddInterceptor(EntityInterceptorFactoryAsync interceptorFactoryAsync)
        {
            Registration.InterceptorFactories.Add(interceptorFactoryAsync);

            return this;
        }

        public IEntityBuilder<TTemplate> AddInterceptor<TInterceptor>()
            where TInterceptor : class, IEntityInterceptor
            => AddInterceptor(async (serviceProvider, context) =>
                await EntitiesRegister.CreateModelElementInstanceAsync<TInterceptor>(serviceProvider, context, "interceptor")
            );
    }
}
