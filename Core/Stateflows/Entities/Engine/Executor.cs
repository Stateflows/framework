using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using Stateflows.Common;
using Stateflows.Common.Context;
using Stateflows.Common.Context.Classes;
using Stateflows.Common.Extensions;
using Stateflows.Common.Utilities;
using Stateflows.Entities.Attributes;
using Stateflows.Entities.Registration;

namespace Stateflows.Entities.Engine
{
    internal class Executor(EntityRegistration registration, StateflowsContext context, IServiceProvider serviceProvider, EntityInspection inspection)
    {
        // ── FieldState<T> / FieldStateRequest<T> reflection caches ───────────────

        // Read Name + Value from an incoming FieldState<T> write event.
        private static readonly ConcurrentDictionary<Type, (PropertyInfo NameProp, PropertyInfo ValueProp)>
            FieldStateReadAccessors = new();

        // Read Name from an incoming FieldStateRequest<T> event.
        private static readonly ConcurrentDictionary<Type, PropertyInfo>
            FieldStateRequestNameProps = new();

        // Build a FieldState<T> response: closed type + compiled factory + Name/Value setters.
        private sealed record FieldStateResponseInfo(
            Func<object> Factory,
            PropertyInfo NameProp,
            PropertyInfo ValueProp
        );

        private static readonly ConcurrentDictionary<Type, FieldStateResponseInfo>
            FieldStateResponseBuilders = new();

        private static FieldStateResponseInfo BuildFieldStateResponseInfo(Type fieldValueType)
        {
            var responseType = typeof(FieldState<>).MakeGenericType(fieldValueType);
            var ctor = responseType.GetConstructor(Type.EmptyTypes)!;
            var factory = Expression.Lambda<Func<object>>(
                Expression.Convert(Expression.New(ctor), typeof(object))
            ).Compile();
            return new FieldStateResponseInfo(
                factory,
                responseType.GetProperty(nameof(FieldState<object>.Name))!,
                responseType.GetProperty(nameof(FieldState<object>.Value))!
            );
        }

        // ─────────────────────────────────────────────────────────────────────────
        public bool Initialized => context.Status == BehaviorStatus.Initialized;

        public BehaviorStatus BehaviorStatus => context.Status;

        public async Task<bool> ValidateFieldWriteAsync<TEvent>(EventHolder<TEvent> eventHolder)
        {
            var type = eventHolder.PayloadType;
            if (!type.IsGenericType || type.GetGenericTypeDefinition() != typeof(FieldState<>))
            {
                return true;
            }

            var (nameProperty, valueProperty) = FieldStateReadAccessors.GetOrAdd(type, t => (
                t.GetProperty(nameof(FieldState<object>.Name))!,
                t.GetProperty(nameof(FieldState<object>.Value))!));
            var name = (string)nameProperty.GetValue(eventHolder.BoxedPayload)!;
            var field = registration.Model.Fields.Values
                .FirstOrDefault(field => field.Name == name && field.ValueType == type.GenericTypeArguments[0]);
            if (field == null || !field.Access.HasFlag(FieldAccess.Set))
            {
                return true;
            }

            var validation = await field.ValidateAsync(valueProperty.GetValue(eventHolder.BoxedPayload), context.Values, serviceProvider);
            if (!validation.IsValid)
            {
                eventHolder.Validation = validation;
                Trace.WriteLine($"Entity '{context.Id.Name}:{context.Id.Instance}': invalid field '{name}': {string.Join("; ", validation.ValidationResults.Select(result => result.ErrorMessage))}");
            }
            return validation.IsValid;
        }

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

        public async Task EnsureInitializedAsync(bool implicitInitialization = true)
        {
            if (Initialized)
            {
                return;
            }

            await inspection.InitializeAsync(implicitInitialization, async () =>
            {
                InitializeStoredFieldValues();

                foreach (var defaultInitializer in registration.Model.DefaultInitializerInvoke)
                {
                    try
                    {
                        defaultInitializer.Invoke(context.Values);
                    }
                    catch (Exception ex)
                    {
                        context.ExecutionExceptions.Enqueue(ex);
                        Trace.WriteLine($"⦗→s⦘ Entity '{context.Id.Name}:{context.Id.Instance}': default initializer threw exception: {ex}");
                        throw;
                    }
                }

                await InitializeComputedFieldValuesAsync();
                await InitializeProjectionValuesAsync();
                context.Status = BehaviorStatus.Initialized;
            });
        }

        public async Task InitializeComputedFieldValuesAsync()
        {
            var oldValues = GetTrackedFieldValuesSnapshot();

            foreach (var field in registration.Model.Fields.Values.Where(f => f.IsComputed).ToList())
            {
                try
                {
                    await field.ComputeAsync(context.Values, inspection);
                }
                catch (Exception ex)
                {
                    context.ExecutionExceptions.Enqueue(ex);
                    Trace.WriteLine($"⦗→s⦘ Entity '{context.Id.Name}:{context.Id.Instance}': computed field '{field.Name}' threw exception: {ex}");
                    throw;
                }
            }

            var newValues = GetTrackedFieldValuesSnapshot();
            var changedFieldNames = GetChangedFieldNames(oldValues, newValues);
            try
            {
                await EntityContextValues.StabilizeComputedFieldsAsync(registration.Model, context.Values, inspection, changedFieldNames);
            }
            catch (Exception ex)
            {
                context.ExecutionExceptions.Enqueue(ex);
                Trace.WriteLine($"⦗→s⦘ Entity '{context.Id.Name}:{context.Id.Instance}': computed field stabilization threw exception: {ex}");
                throw;
            }
        }

        public async Task InitializeProjectionValuesAsync()
        {
            foreach (var projection in registration.Model.Projections.Values)
            {
                try
                {
                    await projection.InvokeAsync(context.Values, inspection);
                }
                catch (Exception ex)
                {
                    context.ExecutionExceptions.Enqueue(ex);
                    Trace.WriteLine($"⦗→s⦘ Entity '{context.Id.Name}:{context.Id.Instance}': projection '{projection.ProjectionType.Name}' threw exception: {ex}");
                    throw;
                }
            }
        }

        public BehaviorStatus ReportedStatus => context.Status == BehaviorStatus.Unknown
            ? BehaviorStatus.NotInitialized
            : context.Status;

        public IEnumerable<string> GetExpectedEventNames()
            => ReportedStatus switch
            {
                BehaviorStatus.NotInitialized => registration.Model.Initializers.Count != 0
                    ? registration.Model.Initializers.Keys.Select(type => Event.GetName(type)).ToArray()
                    : [Event<Initialize>.Name],
                BehaviorStatus.Initialized => registration.Model.Mutations.Keys.Select(type => Event.GetName(type)).ToArray(),
                _ => []
            };

        public EntityInfo GetEntityInfo()
            => new EntityInfo()
            {
                Id = new EntityId(context.Id),
                BehaviorStatus = ReportedStatus,
                ExpectedEvents = GetExpectedEventNames()
            };

        // Events that are handled without initializing the entity.
        public static bool IsProcessedWhenNotInitialized<TEvent>(EventHolder<TEvent> eventHolder)
            => eventHolder is EventHolder<BehaviorInfoRequest>
                or EventHolder<EntityInfoRequest>
                or EventHolder<Reset>
                or EventHolder<Subscribe>
                or EventHolder<Unsubscribe>;

        private void Reset(ResetMode resetMode)
        {
            context.Values.Clear();

            if (resetMode != ResetMode.KeepVersionAndSubscriptions)
            {
                context.Version = 0;

                if (resetMode != ResetMode.KeepSubscriptions)
                {
                    context.Deleted = true;
                }
            }

            context.Status = BehaviorStatus.NotInitialized;
        }

        // [DebuggerHidden]
        public async Task<EventStatus> TryInitializeAsync<TEvent>(TEvent @event)
        {
            if (Initialized)
                return EventStatus.NotConsumed;

            if (@event is Initialize)
            {
                try
                {
                    await EnsureInitializedAsync(implicitInitialization: false);
                }
                catch (Exception ex)
                {
                    context.ExecutionExceptions.Enqueue(ex);
                    Trace.WriteLine($"⦗→s⦘ Entity '{context.Id.Name}:{context.Id.Instance}': initialization threw exception: {ex}");
                    return EventStatus.Failed;
                    // Ensure initialized state is set even on exception
                    // if (context.Status != BehaviorStatus.Initialized)
                    // {
                    //     context.Status = BehaviorStatus.Initialized;
                    // }
                }
                
                return EventStatus.Initialized;
            }

            var eventType = typeof(TEvent);
            if (registration.Model.Initializers.TryGetValue(eventType, out var initModel) && initModel.Invoke != null)
            {
                try
                {
                    await inspection.InitializeAsync(implicitInitialization: false, async () =>
                    {
                        InitializeStoredFieldValues();
                        initModel.Invoke(context.Values, @event);
                        await InitializeComputedFieldValuesAsync();
                        await InitializeProjectionValuesAsync();
                        context.Status = BehaviorStatus.Initialized;
                    });
                }
                catch (Exception ex)
                {
                    context.ExecutionExceptions.Enqueue(ex);
                    Trace.WriteLine($"⦗→s⦘ Entity '{context.Id.Name}:{context.Id.Instance}': initializer for '{Event.GetName(eventType)}' threw exception: {ex}");
                    throw;
                }
                return EventStatus.Initialized;
            }

            return EventStatus.NotInitialized;
        }
        
        public async Task<EventStatus> DoProcessAsync<TEvent>(EventHolder<TEvent> eventHolder)
        {
            var eventType = typeof(TEvent);
            using var behaviorContext = new BehaviorContext(context, serviceProvider);
            
            if (eventHolder is EventHolder<EntityInfoRequest> entityInfoRequestHolder)
            {
                entityInfoRequestHolder.Payload.Respond(GetEntityInfo());
                return EventStatus.Consumed;
            }
            else
            if (eventHolder is EventHolder<BehaviorInfoRequest> behaviorInfoRequestHolder)
            {
                var entityInfo = GetEntityInfo();
                behaviorInfoRequestHolder.Payload.Respond(new BehaviorInfo()
                {
                    Id = entityInfo.Id,
                    BehaviorStatus = entityInfo.BehaviorStatus,
                    ExpectedEvents = entityInfo.ExpectedEvents
                });
                return EventStatus.Consumed;
            }
            else
            if (eventHolder is EventHolder<Reset> resetHolder)
            {
                Trace.WriteLine($"⦗→s⦘ Entity '{context.Id.Name}:{context.Id.Instance}': reset ({resetHolder.Payload.Mode}) received");
                Reset(resetHolder.Payload.Mode);
                return EventStatus.Consumed;
            }
            else
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
                try
                {
                    await mutationModel.InvokeAsync(context.Values, behaviorContext, inspection, eventHolder.Payload);
                }
                catch (Exception ex)
                {
                    context.ExecutionExceptions.Enqueue(ex);
                    Trace.WriteLine($"⦗→s⦘ Entity '{context.Id.Name}:{context.Id.Instance}': mutation '{Event.GetName(eventType)}' threw exception: {ex}");
                    return EventStatus.Failed;
                }

                Trace.WriteLine($"⦗→s⦘ Entity '{context.Id.Name}:{context.Id.Instance}': mutation '{Event.GetName(eventType)}' processed successfully");
                return EventStatus.Consumed;
            }
            else
            if (eventType.IsSubclassOfRawGeneric(typeof(ProjectionRequest<>)))
            {
                var projectionType = eventType.GenericTypeArguments[0];
                Trace.WriteLine($"⦗→s⦘ Entity '{context.Id.Name}:{context.Id.Instance}': projection '{Event.GetName(projectionType)}' request received, processing");
                if (registration.Model.Projections.TryGetValue(projectionType, out var projectionModel) && projectionModel != null)
                {
                    try
                    {
                        var projection = await projectionModel.InvokeAsync(context.Values, inspection);
                        eventHolder.Respond(projection.ToTypedEventHolder());
                        Trace.WriteLine($"⦗→s⦘ Entity '{context.Id.Name}:{context.Id.Instance}': projection '{Event.GetName(projectionType)}' request processed successfully");
                    }
                    catch (Exception ex)
                    {
                        context.ExecutionExceptions.Enqueue(ex);
                        Trace.WriteLine($"⦗→s⦘ Entity '{context.Id.Name}:{context.Id.Instance}': projection '{Event.GetName(projectionType)}' threw exception: {ex}");
                        return EventStatus.Failed;
                        // Respond with null to indicate error
                        // eventHolder.Respond(((object?)null).ToTypedEventHolder());
                    }
                    
                    return EventStatus.Consumed;
                }
            }


            else
            if (eventType.IsGenericType && eventType.GetGenericTypeDefinition() == typeof(FieldState<>))
            {
                var fieldValueType = eventType.GenericTypeArguments[0];
                var payload = eventHolder.BoxedPayload;

                var (fsNameProp, fsValueProp) = FieldStateReadAccessors.GetOrAdd(eventType, t => (
                    t.GetProperty(nameof(FieldState<object>.Name))!,
                    t.GetProperty(nameof(FieldState<object>.Value))!
                ));
                var name = (string)fsNameProp.GetValue(payload)!;
                var value = fsValueProp.GetValue(payload);

                Trace.WriteLine($"⦗→s⦘ Entity '{context.Id.Name}:{context.Id.Instance}': FieldState write for field '{name}' received");

                var field = registration.Model.Fields.Values
                    .FirstOrDefault(f => f.Name == name && f.ValueType == fieldValueType);

                if (field == null)
                    return EventStatus.Failed;

                if (!field.Access.HasFlag(FieldAccess.Set))
                    return EventStatus.Rejected;

                try
                {
                    var oldValues = GetTrackedFieldValuesSnapshot();
                    context.Values[field.Name.GetFieldKey()] = value!;
                    var newValues = GetTrackedFieldValuesSnapshot();
                    var changedFieldNames = GetChangedFieldNames(oldValues, newValues);
                    if (changedFieldNames.Count != 0)
                    {
                        var allChangedFields = await EntityContextValues.StabilizeComputedFieldsAsync(registration.Model, context.Values, inspection, changedFieldNames);
                        await EntityContextValues.RefreshDependentProjectionsAsync(registration.Model, context.Values, behaviorContext, inspection, allChangedFields);
                    }

                    Trace.WriteLine($"⦗→s⦘ Entity '{context.Id.Name}:{context.Id.Instance}': FieldState write for field '{name}' processed successfully");
                }
                catch (Exception ex)
                {
                    context.ExecutionExceptions.Enqueue(ex);
                    Trace.WriteLine($"⦗→s⦘ Entity '{context.Id.Name}:{context.Id.Instance}': FieldState write for field '{name}' threw exception: {ex}");
                    return EventStatus.Failed;
                }

                return EventStatus.Consumed;
            }
            else
            if (eventType.IsGenericType && eventType.GetGenericTypeDefinition() == typeof(FieldStateRequest<>))
            {
                var fieldValueType = eventType.GenericTypeArguments[0];
                var payload = eventHolder.BoxedPayload;

                var fsrNameProp = FieldStateRequestNameProps.GetOrAdd(
                    eventType,
                    t => t.GetProperty(nameof(FieldStateRequest<object>.Name))!);
                var name = (string)fsrNameProp.GetValue(payload)!;

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

                var responseInfo = FieldStateResponseBuilders.GetOrAdd(fieldValueType, BuildFieldStateResponseInfo);
                var response = responseInfo.Factory();
                responseInfo.NameProp.SetValue(response, name);
                responseInfo.ValueProp.SetValue(response, fieldValue);

                eventHolder.Respond(response.ToTypedEventHolder());
                Trace.WriteLine($"⦗→s⦘ Entity '{context.Id.Name}:{context.Id.Instance}': FieldStateRequest read for field '{name}' processed successfully");

                return EventStatus.Consumed;
            }

            return EventStatus.NotConsumed;
        }
    }
}
