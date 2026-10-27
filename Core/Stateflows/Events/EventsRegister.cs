using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Stateflows.Common;
using Stateflows.StateMachines;
using Stateflows.Activities;

namespace Stateflows.Events
{
    internal sealed class EventsRegister : IEventsBuilder, IEventsRegister, IEventFactory, IStateflowsValidator
    {
        private readonly Dictionary<string, EventDefinition> events = new(StringComparer.Ordinal);
        private readonly HashSet<string> clrNames = new(StringComparer.Ordinal);
        private bool built;
        public IReadOnlyDictionary<string, EventDefinition> Events => new ReadOnlyDictionary<string, EventDefinition>(events);

        public EventDefinition GetEvent(string eventName)
            => events.TryGetValue(eventName, out var definition) ? definition :
                throw new ArgumentException($"Structural event '{eventName}' is not registered.", nameof(eventName));

        public IEventsBuilder AddEvent(string eventName, EventBuildAction buildAction)
            => Add(eventName, buildAction);
        public IEventsBuilder AddRequest(string eventName, string responseEventName, EventBuildAction buildAction)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(responseEventName);
            return Add(eventName, buildAction, responseEventName);
        }
        public IEventsBuilder AddRequest<TResponse>(string eventName, EventBuildAction buildAction)
        {
            if (typeof(StructuralEvent).IsAssignableFrom(typeof(TResponse)))
                throw new ArgumentException("A structural response must be referenced by its registered event name.", nameof(TResponse));
            return Add(eventName, buildAction, responseType: typeof(TResponse));
        }

        private IEventsBuilder Add(string name, EventBuildAction action, string responseName = null, Type responseType = null)
        {
            if (built) throw new InvalidOperationException("Event definitions are fixed after Stateflows configuration.");
            ArgumentNullException.ThrowIfNull(action);
            var builder = new EventBuilder();
            action(builder);
            ValidateDefinitionFields(builder.Fields);
            if (responseType != null && Event.GetName(responseType) == name)
                throw new ArgumentException($"Request '{name}' collides with its CLR response event name.");
            if (clrNames.Contains(name)) throw new ArgumentException($"Event name '{name}' is already used by a CLR event.");
            if (responseType != null) RegisterClr(responseType);
            events.Add(name, new EventDefinition(name, builder.Fields, responseName, responseType));
            return this;
        }

        internal void RegisterClr(Type type)
        {
            ArgumentNullException.ThrowIfNull(type);
            if (typeof(StructuralEvent).IsAssignableFrom(type)) return;
            var name = Event.GetName(type);
            if (events.ContainsKey(name)) throw new ArgumentException($"Event name '{name}' is already used by a structural event.");
            clrNames.Add(name);
        }

        internal void Build()
        {
            foreach (var type in new[] { typeof(Initialize), typeof(Finalize), typeof(Reset),
                typeof(Startup), typeof(Subscribe), typeof(Unsubscribe),
                typeof(NotificationsRequest), typeof(NotificationsResponse),
                typeof(TokensInput), typeof(TokensOutput), typeof(TokensOutputRequest),
                typeof(BehaviorInfoRequest), typeof(BehaviorInfo), typeof(StateMachineInfoRequest),
                typeof(StateMachineInfo), typeof(ActivityInfoRequest), typeof(ActivityInfo) })
                RegisterClr(type);
            foreach (var definition in events.Values.Where(e => e.ResponseEventName != null))
                GetEvent(definition.ResponseEventName);
            built = true;
        }

        private static void ValidateDefinitionFields(IReadOnlyDictionary<string, FieldDefinition> fields)
        {
            foreach (var field in fields.Values)
            {
                foreach (var attribute in field.Validation.Concat(field.ItemValidation))
                    if (attribute is CompareAttribute compare && !fields.ContainsKey(compare.OtherProperty))
                        throw new ArgumentException($"Field '{field.Name}' compares with undeclared field '{compare.OtherProperty}'.");
                foreach (var attribute in field.Validation)
                {
                    if (attribute is StringLengthAttribute && (field.IsCollection ||
                        field.Type is not (FieldType.String or FieldType.Enum)))
                        throw new ArgumentException($"StringLength cannot validate field '{field.Name}'.");
                    if (attribute is MinLengthAttribute or MaxLengthAttribute or LengthAttribute &&
                        !field.IsCollection && field.Type is not (FieldType.String or FieldType.Enum or FieldType.Binary))
                        throw new ArgumentException($"Length validation requires a string, binary or collection field: '{field.Name}'.");
                }
                foreach (var attribute in field.ItemValidation)
                    if (attribute is StringLengthAttribute or MinLengthAttribute or MaxLengthAttribute or LengthAttribute &&
                        field.Type is not (FieldType.String or FieldType.Enum or FieldType.Binary))
                        throw new ArgumentException($"Length validation cannot validate elements of '{field.Name}'.");
                ValidateDefinitionFields(field.Fields);
            }
        }

        public StructuralEvent Create(string eventName, EventValueBuildAction buildAction)
        {
            ArgumentNullException.ThrowIfNull(buildAction);
            var builder = new EventValueBuilder();
            buildAction(builder);
            return Create(eventName, builder.Values);
        }

        public StructuralEvent Create(string eventName, IReadOnlyDictionary<string, object> fields)
        {
            ArgumentNullException.ThrowIfNull(fields);
            var definition = GetEvent(eventName);
            var values = NormalizeFields(definition.Fields, fields, eventName);
            var result = definition.IsRequest
                ? StructuralRequest.Create(definition, values)
                : new StructuralEvent(eventName, values);
            if (result is StructuralRequest request) BindResponseValidation(request);
            return result;
        }

        private static Dictionary<string, object> NormalizeFields(IReadOnlyDictionary<string, FieldDefinition> fields,
            IReadOnlyDictionary<string, object> input, string path)
        {
            var result = new Dictionary<string, object>(StringComparer.Ordinal);
            foreach (var field in fields.Values)
                if (input.TryGetValue(field.Name, out var value))
                    result.Add(field.Name, Normalize(field, value, $"{path}.{field.Name}"));
            return result;
        }

        private static object Normalize(FieldDefinition field, object value, string path)
        {
            if (value == null) return null;
            if (field.IsCollection)
            {
                if (value is not IEnumerable sequence || value is string || value is IDictionary)
                    throw new ArgumentException($"Field '{path}' must be a collection.");
                var values = new List<object>();
                foreach (var item in sequence) values.Add(NormalizeItem(field, item, $"{path}[{values.Count}]"));
                return values.ToArray();
            }
            return NormalizeItem(field, value, path);
        }

        private static object NormalizeItem(FieldDefinition field, object value, string path)
        {
            if (value == null) return null;
            if (field.Type == null)
            {
                var fields = value is StructuralValue structural ? structural.Fields :
                    value as IReadOnlyDictionary<string, object>;
                if (fields == null) throw new ArgumentException($"Field '{path}' must be a structural object.");
                return new StructuralValue(NormalizeFields(field.Fields, fields, path));
            }
            var type = field.Type.Value switch
            {
                FieldType.String or FieldType.Enum => typeof(string),
                FieldType.Boolean => typeof(bool), FieldType.Int32 => typeof(int),
                FieldType.Int64 => typeof(long), FieldType.Decimal => typeof(decimal),
                FieldType.Double => typeof(double), FieldType.Guid => typeof(Guid),
                FieldType.DateTime => typeof(DateTime), FieldType.DateTimeOffset => typeof(DateTimeOffset),
                FieldType.TimeSpan => typeof(TimeSpan), FieldType.DateOnly => typeof(DateOnly),
                FieldType.TimeOnly => typeof(TimeOnly), FieldType.Binary => typeof(byte[]),
                _ => throw new ArgumentOutOfRangeException(nameof(field))
            };
            if (!type.IsInstanceOfType(value))
                throw new ArgumentException($"Field '{path}' requires '{type.Name}', not '{value.GetType().Name}'.");
            if (field.Type == FieldType.Enum && !field.EnumValues.Contains((string)value))
                throw new ArgumentException($"Field '{path}' has an undeclared enum value.");
            if (value is double number && !double.IsFinite(number))
                throw new ArgumentException($"Field '{path}' must be finite.");
            return value is byte[] bytes ? bytes.ToArray() : value;
        }

        public Task<bool> ValidateAsync<TEvent>(TEvent value, List<ValidationResult> validationResults)
        {
            if (value is not StructuralEvent structural) return Task.FromResult(true);
            if (string.IsNullOrWhiteSpace(structural.Name) || !events.TryGetValue(structural.Name, out var definition))
            {
                validationResults.Add(new ValidationResult($"Structural event '{structural.Name}' is not registered."));
                return Task.FromResult(false);
            }
            try { structural.ReplaceFields(NormalizeFields(definition.Fields, structural.Fields, structural.Name)); }
            catch (ArgumentException exception)
            {
                validationResults.Add(new ValidationResult(exception.Message));
                return Task.FromResult(false);
            }
            if (structural is StructuralRequest request)
            {
                if (!definition.IsRequest || request.ResponseEventName != definition.ResponseEventName ||
                    request.ResponseType != definition.ResponseType)
                {
                    validationResults.Add(new ValidationResult($"Request '{structural.Name}' does not match its registered response contract."));
                    return Task.FromResult(false);
                }
                BindResponseValidation(request);
            }
            else if (definition.IsRequest)
            {
                validationResults.Add(new ValidationResult($"Event '{structural.Name}' must be created as a request."));
                return Task.FromResult(false);
            }
            ValidateFields(definition.Fields, structural, "", validationResults);
            return Task.FromResult(validationResults.Count == 0);
        }

        private void BindResponseValidation(StructuralRequest request)
            => request.ValidateResponse = response =>
            {
                var responseDefinition = GetEvent(response.Name);
                response.ReplaceFields(NormalizeFields(responseDefinition.Fields, response.Fields, response.Name));
                var errors = new List<ValidationResult>();
                ValidateFields(responseDefinition.Fields, response, "", errors);
                if (errors.Count != 0) throw new ValidationException(string.Join("; ", errors.Select(e => e.ErrorMessage)));
            };

        private static void ValidateFields(IReadOnlyDictionary<string, FieldDefinition> fields, StructuralValue value,
            string path, List<ValidationResult> results)
        {
            foreach (var field in fields.Values)
            {
                var member = string.IsNullOrEmpty(path) ? field.Name : $"{path}.{field.Name}";
                value.Fields.TryGetValue(field.Name, out var fieldValue);
                ValidateAttributes(field.Validation, fieldValue, value, field.Name, member, results);
                if (fieldValue == null) continue;
                if (field.IsCollection)
                {
                    var index = 0;
                    foreach (var item in (IEnumerable)fieldValue)
                    {
                        var itemPath = $"{member}[{index++}]";
                        ValidateAttributes(field.ItemValidation, item, value, field.Name, itemPath, results);
                        if (item is StructuralValue child) ValidateFields(field.Fields, child, itemPath, results);
                    }
                }
                else if (fieldValue is StructuralValue child) ValidateFields(field.Fields, child, member, results);
            }
        }

        private static void ValidateAttributes(IEnumerable<ValidationAttribute> attributes, object value,
            StructuralValue owner, string name, string path, List<ValidationResult> results)
        {
            foreach (var attribute in attributes)
            {
                ValidationResult result;
                if (attribute is CompareAttribute compare)
                {
                    owner.Fields.TryGetValue(compare.OtherProperty, out var other);
                    result = Equals(value, other) ? ValidationResult.Success :
                        new ValidationResult(compare.FormatErrorMessage(name));
                }
                else result = attribute.GetValidationResult(value, new ValidationContext(owner) { MemberName = name, DisplayName = name });
                if (result != ValidationResult.Success)
                    results.Add(new ValidationResult(result.ErrorMessage, [path]));
            }
        }
    }

    internal sealed class EventValueBuilder : IEventValueBuilder
    {
        internal readonly Dictionary<string, object> Values = new(StringComparer.Ordinal);
        public IEventValueBuilder Set<T>(string fieldName, T value) { Values.Add(fieldName, value); return this; }
        public IEventValueBuilder SetObject(string fieldName, EventValueBuildAction buildAction)
        {
            ArgumentNullException.ThrowIfNull(buildAction);
            var builder = new EventValueBuilder();
            buildAction(builder);
            return Set(fieldName, builder.Values);
        }
        public IEventValueBuilder SetObjectCollection(string fieldName, params EventValueBuildAction[] buildActions)
        {
            ArgumentNullException.ThrowIfNull(buildActions);
            return Set(fieldName, buildActions.Select(action =>
            {
                ArgumentNullException.ThrowIfNull(action);
                var builder = new EventValueBuilder();
                action(builder);
                return builder.Values;
            }).ToArray());
        }
    }
}
