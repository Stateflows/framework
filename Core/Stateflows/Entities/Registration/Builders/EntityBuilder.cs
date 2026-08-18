using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
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
            return this;
        }

        public IEntityBuilder<TTemplate> AddField<TField>(Expression<Func<TTemplate, TField>> fieldSelector, FieldBuildAction<TTemplate, TField> buildAction = null)
        {
            if (fieldSelector.Body is not MemberExpression memberExpression)
                throw new ArgumentException("Field selector must be a simple member access expression (e.g. t => t.MyField)", nameof(fieldSelector));

            var fieldName = memberExpression.Member.Name;

            if (Registration.Model.Fields.ContainsKey(fieldName))
                throw new InvalidOperationException($"Field '{fieldName}' is already registered in entity '{Registration.Name}'");

            var fieldModel = new FieldModel<TTemplate, TField>(Registration.Model, fieldName, FieldAccess.None);
            
            Registration.Model.Fields.Add(fieldName, fieldModel);
            
            buildAction?.Invoke(new FieldBuilder<TTemplate, TField>(fieldModel));
            
            Registration.VisitingTasks.Add(async visitor =>
            {
                // await visitor.FieldAddedAsync<TTemplate, TField>(Registration.Name, Registration.Version, fieldName, (TField)fieldModel.DefaultValue, fieldModel.IsComputed);
            });

            return this;
        }

        public IEntityBuilder<TTemplate> AddMutation<TMutationEvent>(MutationAction<TTemplate, TMutationEvent> mutationAction)
        {
            Registration.Model.Mutations[typeof(TMutationEvent)] = new MutationModel
            {
                MutationType = typeof(TMutationEvent),
                MutationAction = mutationAction,
                Invoke = (values, behaviorContext, eventObj) =>
                {
                    var oldValues = GetTrackedFieldValuesSnapshot(values);

                    var (_, entity) = EntityProxy<TTemplate>.Create(values, Registration.Model);
                    mutationAction(new MutationContext<TTemplate, TMutationEvent>(
                        entity,
                        (TMutationEvent)eventObj
                    ));

                    var newValues = GetTrackedFieldValuesSnapshot(values);
                    var changedFieldNames = GetChangedFieldNames(oldValues, newValues);
                    if (changedFieldNames.Count != 0)
                    {
                        var allChangedFields = EntityContextValues.StabilizeComputedFields(Registration.Model, values, changedFieldNames);
                        EntityContextValues.RefreshDependentProjections(Registration.Model, values, behaviorContext, allChangedFields);
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
                Invoke = (values, _) =>
                {
                    var (proxy, entity) = EntityProxy<TTemplate>.Create(values, Registration.Model);
                    var projection = projectionAction(entity);
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
    }
}
