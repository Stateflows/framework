using System.Diagnostics;
using System.Linq;
using Stateflows.Common;
using Stateflows.Common.Classes;
using Stateflows.Common.Context;
using Stateflows.Common.Extensions;
using Stateflows.Common.Utilities;
using Stateflows.Entities.Registration;

namespace Stateflows.Entities.Engine
{
    internal class Executor(EntityRegistration registration, StateflowsContext context)
    {
        public bool Initialized => context.Status == BehaviorStatus.Initialized;

        public BehaviorStatus BehaviorStatus => context.Status;

        private void InitializeFieldValues()
        {
            // registration.Model.Fields.Keys
            //     .Where(fieldName => !context.Values.ContainsKey(fieldName.GetFieldKey()))
            //     .ToList()
            //     .ForEach(fieldName =>
            //     {
            //         var property = registration.Model.TemplateType.GetProperty(fieldName);
            //         context.Values[fieldName] = property?.GetDefaultValueForProperty();
            //     });
            
            registration.Model.Fields.Values.Where(f => f.IsComputed).ToList().ForEach(f => f.Compute(context.Values));
        }

        // [DebuggerHidden]
        public EventStatus TryInitialize<TEvent>(TEvent @event)
        {
            if (Initialized)
                return EventStatus.NotConsumed;

            if (@event is Initialize)
            {
                registration.Model.DefaultInitializerInvoke?.Invoke(context.Values);
                context.Status = BehaviorStatus.Initialized;
                InitializeFieldValues();
                return EventStatus.Initialized;
            }

            var eventType = typeof(TEvent);
            if (registration.Model.Initializers.TryGetValue(eventType, out var initModel) && initModel.Invoke != null)
            {
                initModel.Invoke(context.Values, @event);
                context.Status = BehaviorStatus.Initialized;
                InitializeFieldValues();
                return EventStatus.Initialized;
            }

            return EventStatus.NotInitialized;
        }
        
        public EventStatus DoProcessAsync<TEvent>(EventHolder<TEvent> eventHolder)
        {
            var eventType = typeof(TEvent);
            
            if (registration.Model.Mutations.TryGetValue(eventType, out var mutationModel) && mutationModel != null)
            {
                mutationModel.Invoke(context.Values, eventHolder.Payload);
                
                if (BehaviorStatus != BehaviorStatus.Initialized)
                {
                    InitializeFieldValues();
                }
                
                return EventStatus.Consumed;
            }

            if (eventType.IsSubclassOfRawGeneric(typeof(ProjectionRequest<>)))
            {
                var projectionType = eventType.GenericTypeArguments[0];
                if (registration.Model.Projections.TryGetValue(projectionType, out var projectionModel) && projectionModel != null)
                {
                    var projection = projectionModel.Invoke(context.Values);
                    // var projectionResponseType = typeof(ProjectionResponse<>).MakeGenericType(projectionType);
                    // var response = StateflowsActivator.CreateUninitializedInstance(projectionResponseType);
                    // projectionResponseType.GetProperty(nameof(ProjectionResponse<object>.Projection)).SetValue(response, projection);
                    eventHolder.Respond(projection.ToTypedEventHolder());
                    
                    return EventStatus.Consumed;
                }
            }

            if (eventHolder.Payload is FieldValueEvent fieldValueEvent && eventType.IsSubclassOfRawGeneric(typeof(FieldValueRequest<>)))
            {
                var fieldValueType = eventType.GenericTypeArguments[0];
                if (registration.Model.Fields.TryGetValue(fieldValueEvent.FieldName, out var fieldModel) && fieldModel != null)
                {
                    var fieldValue =
                        context.Values.TryGetValue(fieldModel.Name.GetFieldKey(), out var value) &&
                        value != null &&
                        fieldValueType.IsAssignableFrom(value.GetType())
                        ? value
                        : null;
                    // var fieldValueResponseType = typeof(FieldValueResponse<>).MakeGenericType(fieldValueType);
                    // var response = StateflowsActivator.CreateUninitializedInstance(fieldValueResponseType);
                    // fieldValueResponseType.GetProperty(nameof(FieldValueResponse<object>.FieldName)).SetValue(response, fieldValueEvent.FieldName);
                    // fieldValueResponseType.GetProperty(nameof(FieldValueResponse<object>.FieldValue)).SetValue(response, fieldValue);
                    eventHolder.Respond(fieldValue.ToTypedEventHolder());
                    
                    return EventStatus.Consumed;
                }
            }

            return EventStatus.NotConsumed;
        }
    }
}
