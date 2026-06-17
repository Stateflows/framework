using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Stateflows.Entities.Engine;
using Stateflows.Entities.Models;
using Stateflows.Entities.Registration.Interfaces;

namespace Stateflows.Entities.Registration.Builders
{
    internal class EntityBuilder<TTemplate>(EntityRegistration registration) : IEntityBuilder<TTemplate>
        where TTemplate : class
    {
        public EntityRegistration Registration { get; } = registration;

        public IEntityBuilder<TTemplate> AddDefaultInitializer(DefaultInitializerAction<TTemplate> action)
        {
            Registration.Model.DefaultInitializer = action;
            Registration.Model.DefaultInitializerInvoke = values =>
            {
                var (_, entity) = EntityProxy<TTemplate>.Create(values, Registration.Model);
                action(new DefaultEntityInitializationContext<TTemplate>(entity));
            };
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

            var fieldModel = new FieldModel<TTemplate, TField>(Registration.Model, fieldName);
            
            Registration.Model.Fields.Add(fieldName, fieldModel);
            
            buildAction?.Invoke(new FieldBuilder<TTemplate, TField>(fieldModel, this));

            return this;
        }

        /// <summary>
        /// Adds a computation trigger to a registered field identified by its interface property name.
        /// Called internally when analysing computation expressions in <see cref="FieldBuilder{TTemplate,TField}"/>.
        /// </summary>
        internal void AddComputationTrigger(string fieldName, string computedFieldName, Action<Dictionary<string, object>> trigger)
        {
            var depField = Registration.Model.Fields.Values.FirstOrDefault(f => f.Name == fieldName);
            if (depField != null)
            {
                depField.ComputationTriggers[computedFieldName] = trigger;
            }
        }

        internal void RemoveComputationTrigger(string fieldName, string computedFieldName)
        {
            var depField = Registration.Model.Fields.Values.FirstOrDefault(f => f.Name == fieldName);
            depField?.ComputationTriggers.Remove(computedFieldName);
        }

        public IEntityBuilder<TTemplate> AddMutation<TMutationEvent>(MutationAction<TTemplate, TMutationEvent> mutationAction)
        {
            Registration.Model.Mutations[typeof(TMutationEvent)] = new MutationModel
            {
                MutationType = typeof(TMutationEvent),
                MutationAction = mutationAction,
                Invoke = (values, eventObj) =>
                {
                    var oldValues = values.ToDictionary();

                    var (_, entity) = EntityProxy<TTemplate>.Create(values, Registration.Model);
                    mutationAction(new MutationContext<TTemplate, TMutationEvent>(
                        entity,
                        (TMutationEvent)eventObj
                    ));
                    
                    var hit = false;
                    do
                    {
                        hit = false;
                        var newValues = values.ToDictionary();
                        foreach (var value in newValues)
                        {
                            if (oldValues.TryGetValue(value.Key, out var oldValue) && Equals(oldValue, value.Value))
                            {
                                continue;
                            }

                            if (Registration.Model.Fields.TryGetValue(value.Key.StripFieldKey(), out var fieldModel))
                            {
                                fieldModel.ComputationTriggers.Values.ToList().ForEach(trigger => trigger(values));
                                hit = true;
                            }
                        }

                        oldValues = newValues;
                    } while (hit);
                }
            };
            return this;
        }

        public IEntityBuilder<TTemplate> AddProjection<TProjection>(ProjectionAction<TTemplate, TProjection> projectionAction)
        {
            Registration.Model.Projections[typeof(TProjection)] = new ProjectionModel
            {
                ProjectionType = typeof(TProjection),
                ProjectionAction = projectionAction,
                Invoke = values =>
                {
                    var (_, entity) = EntityProxy<TTemplate>.Create(values, Registration.Model);
                    return projectionAction(entity);
                }
            };
            return this;
        }
    }
}
