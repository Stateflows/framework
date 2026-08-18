using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Stateflows.Common;
using Stateflows.Common.Context;
using Stateflows.Common.Context.Classes;
using Stateflows.Common.Extensions;
using Stateflows.Common.Utilities;
using Stateflows.Entities.Attributes;
using Stateflows.Entities.Registration;

namespace Stateflows.Entities.Engine
{
    internal class Executor(EntityRegistration registration, StateflowsContext context, IServiceProvider serviceProvider)
    {
        public bool Initialized => context.Status == BehaviorStatus.Initialized;

        public BehaviorStatus BehaviorStatus => context.Status;

        private Dictionary<string, object?> GetTrackedFieldValuesSnapshot()
            => registration.Model.Fields.Values.ToDictionary(
                field => field.Name,
                field => context.Values.TryGetValue(field.Name.GetFieldKey(), out var value)
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

        private void InitializeStoredFieldValues()
            => registration.Model.Fields.Values
                .Where(field => !field.IsComputed && field.HasDefaultValue)
                .ToList()
                .ForEach(field =>
                {
                    var fieldKey = field.Name.GetFieldKey();
                    if (!context.Values.ContainsKey(fieldKey))
                    {
                        context.Values[fieldKey] = field.DefaultValue!;
                    }
                });

        public void EnsureInitialized()
        {
            if (Initialized)
            {
                return;
            }

            InitializeStoredFieldValues();
            
            foreach (var defaultInitializer in registration.Model.DefaultInitializerInvoke)
            {
                defaultInitializer.Invoke(context.Values);
            }
            
            context.Status = BehaviorStatus.Initialized;
            InitializeComputedFieldValues();
            InitializeProjectionValues();
        }

        public void InitializeComputedFieldValues()
        {
            var oldValues = GetTrackedFieldValuesSnapshot();

            registration.Model.Fields.Values.Where(f => f.IsComputed).ToList().ForEach(f => f.Compute(context.Values));

            var newValues = GetTrackedFieldValuesSnapshot();
            var changedFieldNames = GetChangedFieldNames(oldValues, newValues);
            EntityContextValues.StabilizeComputedFields(registration.Model, context.Values, changedFieldNames);
        }

        public void InitializeProjectionValues()
        {
            var behaviorContext = new BehaviorContext(context, serviceProvider);
            foreach (var projection in registration.Model.Projections.Values)
            {
                projection.Invoke(context.Values, behaviorContext);
            }
        }

        [DebuggerHidden]
        public EventStatus TryInitialize<TEvent>(TEvent @event)
        {
            if (Initialized)
                return EventStatus.NotConsumed;

            if (@event is Initialize)
            {
                EnsureInitialized();
                return EventStatus.Initialized;
            }

            var eventType = typeof(TEvent);
            if (registration.Model.Initializers.TryGetValue(eventType, out var initModel) && initModel.Invoke != null)
            {
                InitializeStoredFieldValues();
                initModel.Invoke(context.Values, @event);
                context.Status = BehaviorStatus.Initialized;
                InitializeComputedFieldValues();
                InitializeProjectionValues();
                return EventStatus.Initialized;
            }

            return EventStatus.NotInitialized;
        }
        
        public EventStatus DoProcessAsync<TEvent>(EventHolder<TEvent> eventHolder)
        {
            var eventType = typeof(TEvent);
            var behaviorContext = new BehaviorContext(context, serviceProvider);
            
            if (eventHolder is EventHolder<Subscribe> subscribeHolder)
            {
                Trace.WriteLine($"⦗→s⦘ Entity '{context.Id.Name}:{context.Id.Instance}': subscription for {string.Join(", ", subscribeHolder.Payload.NotificationNames.Select(n => $"'{n}'"))} sent from '{subscribeHolder.Payload.BehaviorId.Name}:{subscribeHolder.Payload.BehaviorId.Instance}'");
                var subscribe = subscribeHolder.Payload;
                if (context.AddSubscribers(subscribe.BehaviorId, subscribe.NotificationNames))
                {
                    return EventStatus.Consumed;
                }
            }
            else
            if (eventHolder is EventHolder<Unsubscribe> unsubscribeHolder)
            {
                Trace.WriteLine($"⦗→s⦘ Entity '{context.Id.Name}:{context.Id.Instance}': unsubscription for {string.Join(", ", unsubscribeHolder.Payload.NotificationNames.Select(n => $"'{n}'"))} sent from '{unsubscribeHolder.Payload.BehaviorId.Name}:{unsubscribeHolder.Payload.BehaviorId.Instance}'");
                var unsubscribe = unsubscribeHolder.Payload;
                if (context.RemoveSubscribers(unsubscribe.BehaviorId, unsubscribe.NotificationNames))
                {
                    return EventStatus.Consumed;
                }
            }
            else
            if (registration.Model.Mutations.TryGetValue(eventType, out var mutationModel) && mutationModel != null)
            {   
                Trace.WriteLine($"⦗→s⦘ Entity '{context.Id.Name}:{context.Id.Instance}': mutation '{Event.GetName(eventType)}' received, processing");
                mutationModel.Invoke(context.Values, behaviorContext, eventHolder.Payload);
                
                return EventStatus.Consumed;
            }
            else
            if (eventType.IsSubclassOfRawGeneric(typeof(ProjectionRequest<>)))
            {
                var projectionType = eventType.GenericTypeArguments[0];
                Trace.WriteLine($"⦗→s⦘ Entity '{context.Id.Name}:{context.Id.Instance}': projection '{Event.GetName(projectionType)}' request received, processing");
                if (registration.Model.Projections.TryGetValue(projectionType, out var projectionModel) && projectionModel != null)
                {
                    var projection = projectionModel.Invoke(context.Values, behaviorContext);
                    eventHolder.Respond(projection.ToTypedEventHolder());
                    
                    return EventStatus.Consumed;
                }
            }


            else
            if (eventType.IsGenericType && eventType.GetGenericTypeDefinition() == typeof(FieldState<>))
            {
                var fieldValueType = eventType.GenericTypeArguments[0];
                var payload = eventHolder.BoxedPayload;
                var name = (string)eventType.GetProperty(nameof(FieldState<object>.Name))!.GetValue(payload)!;
                var value = eventType.GetProperty(nameof(FieldState<object>.Value))!.GetValue(payload);

                Trace.WriteLine($"⦗→s⦘ Entity '{context.Id.Name}:{context.Id.Instance}': FieldState write for field '{name}' received");

                var field = registration.Model.Fields.Values
                    .FirstOrDefault(f => f.Name == name && f.ValueType == fieldValueType);

                if (field == null)
                    return EventStatus.Failed;

                if (!field.Access.HasFlag(FieldAccess.Set))
                    return EventStatus.Rejected;

                context.Values[field.Name.GetFieldKey()] = value!;
                InitializeComputedFieldValues();

                return EventStatus.Consumed;
            }
            else
            if (eventType.IsGenericType && eventType.GetGenericTypeDefinition() == typeof(FieldStateRequest<>))
            {
                var fieldValueType = eventType.GenericTypeArguments[0];
                var payload = eventHolder.BoxedPayload;
                var name = (string)eventType.GetProperty(nameof(FieldStateRequest<object>.Name))!.GetValue(payload)!;

                Trace.WriteLine($"⦗→s⦘ Entity '{context.Id.Name}:{context.Id.Instance}': FieldStateRequest read for field '{name}' received");

                var field = registration.Model.Fields.Values
                    .FirstOrDefault(f => f.Name == name && f.ValueType == fieldValueType);

                if (field == null)
                    return EventStatus.Failed;

                if (!field.Access.HasFlag(FieldAccess.Get))
                    return EventStatus.Rejected;

                var fieldKey = field.Name.GetFieldKey();
                var fieldValue = context.Values.TryGetValue(fieldKey, out var stored) ? stored : null;

                // Coerce the stored value to the declared field type (e.g. Int64→Int32 after JSON round-trip)
                if (fieldValue != null && fieldValue.GetType() != fieldValueType)
                {
                    try { fieldValue = Convert.ChangeType(fieldValue, fieldValueType); }
                    catch { /* leave as-is; SetValue will surface a clear error if incompatible */ }
                }

                var responseType = typeof(FieldState<>).MakeGenericType(fieldValueType);
                var response = Activator.CreateInstance(responseType)!;
                responseType.GetProperty(nameof(FieldState<object>.Name))!.SetValue(response, name);
                responseType.GetProperty(nameof(FieldState<object>.Value))!.SetValue(response, fieldValue);

                eventHolder.Respond(response.ToTypedEventHolder());

                return EventStatus.Consumed;
            }

            return EventStatus.NotConsumed;
        }
    }
}
