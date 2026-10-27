using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace Stateflows.Common
{
    /// <summary>Provides typed access to the field values of a structural event or nested object.</summary>
    public class StructuralValue
    {
        [JsonProperty]
        [JsonConverter(typeof(StructuralFieldsConverter))]
        private Dictionary<string, object> values;

        [JsonConstructor]
        internal StructuralValue(Dictionary<string, object> values)
            => this.values = new Dictionary<string, object>(values, StringComparer.Ordinal);

        /// <summary>Gets a read-only view of the values present on this structural value.</summary>
        [JsonIgnore]
        public IReadOnlyDictionary<string, object> Fields => new System.Collections.ObjectModel.ReadOnlyDictionary<string, object>(values);

        /// <summary>Determines whether a field value was supplied.</summary>
        /// <param name="fieldName">The registered field name.</param>
        /// <returns><see langword="true"/> when the field is present.</returns>
        public bool ContainsField(string fieldName) => values.ContainsKey(fieldName);

        internal void ReplaceFields(Dictionary<string, object> fields) => values = fields;

        /// <summary>Gets a required field using the requested CLR type.</summary>
        /// <typeparam name="T">The expected value type.</typeparam>
        /// <param name="fieldName">The registered field name.</param>
        /// <returns>The field value, or its default when the stored value is null and <typeparamref name="T"/> allows null.</returns>
        /// <exception cref="KeyNotFoundException">The field was not supplied.</exception>
        /// <exception cref="InvalidOperationException">The stored value cannot be read as <typeparamref name="T"/>.</exception>
        public T Get<T>(string fieldName)
        {
            if (!values.TryGetValue(fieldName, out var value))
                throw new KeyNotFoundException($"Field '{fieldName}' is missing.");
            if (value is T typed) return typed;
            if (value == null && default(T) == null) return default;
            throw new InvalidOperationException($"Field '{fieldName}' cannot be read as '{typeof(T)}'.");
        }

        /// <summary>Attempts to read a field using the requested CLR type.</summary>
        /// <typeparam name="T">The expected value type.</typeparam>
        /// <param name="fieldName">The registered field name.</param>
        /// <param name="value">Receives the converted value when present and compatible; otherwise receives the default.</param>
        /// <returns><see langword="true"/> when the field is present and compatible with <typeparamref name="T"/>.</returns>
        public bool TryGet<T>(string fieldName, out T value)
        {
            if (values.TryGetValue(fieldName, out var boxed))
            {
                if (boxed is T typed) { value = typed; return true; }
                if (boxed == null && default(T) == null) { value = default; return true; }
            }
            value = default;
            return false;
        }

        /// <summary>Gets a collection field as a read-only list of the requested item type.</summary>
        /// <typeparam name="T">The expected item type.</typeparam>
        /// <param name="fieldName">The registered collection field name.</param>
        /// <returns>A read-only snapshot of the collection items.</returns>
        /// <exception cref="KeyNotFoundException">The field was not supplied.</exception>
        /// <exception cref="InvalidOperationException">A collection item cannot be read as <typeparamref name="T"/>.</exception>
        public IReadOnlyList<T> GetCollection<T>(string fieldName)
        {
            var collection = Get<IList>(fieldName);
            return collection.Cast<object>().Select(value =>
                value is T typed ? typed :
                value == null && default(T) == null ? default :
                throw new InvalidOperationException($"An element of '{fieldName}' cannot be read as '{typeof(T)}'."))
                .ToList().AsReadOnly();
        }
    }

    /// <summary>A named instance of a registered structural event.</summary>
    public class StructuralEvent : StructuralValue
    {
        /// <summary>Gets the registered event name.</summary>
        public string Name { get; }

        [JsonConstructor]
        internal StructuralEvent(string name, Dictionary<string, object> values) : base(values)
            => Name = name;
    }

    /// <summary>A structural event with a registered response type or structural response event.</summary>
    public class StructuralRequest : StructuralEvent
    {
        [JsonIgnore]
        internal Action<StructuralEvent> ValidateResponse { get; set; }
        /// <summary>Gets the name of the registered structural response event, when the response is structural.</summary>
        public string ResponseEventName { get; }
        /// <summary>Gets the CLR response type, when this request expects a CLR response.</summary>
        public Type ResponseType { get; }

        [JsonConstructor]
        internal StructuralRequest(string name, Dictionary<string, object> values,
            string responseEventName, Type responseType) : base(name, values)
        {
            ResponseEventName = responseEventName;
            ResponseType = responseType;
        }

        /// <summary>Completes this request with a structural response event.</summary>
        /// <param name="response">The registered structural response.</param>
        /// <exception cref="InvalidOperationException">The response name is not accepted or the request is not bound to its registered contract.</exception>
        public void Respond(StructuralEvent response)
        {
            ArgumentNullException.ThrowIfNull(response);
            if (ResponseEventName != response.Name || ResponseType != null)
                throw new InvalidOperationException($"Request '{Name}' does not accept structural response '{response.Name}'.");
            if (ValidateResponse == null)
                throw new InvalidOperationException($"Request '{Name}' must be bound to its registered contract before responding.");
            ValidateResponse(response);
            ResponseHolder.Respond(this, Utilities.ObjectExtensions.ToTypedEventHolder(response));
        }

        /// <summary>Completes this request with a CLR response value.</summary>
        /// <typeparam name="TResponse">The response value type.</typeparam>
        /// <param name="response">The response value.</param>
        /// <exception cref="InvalidOperationException">The response type does not match the request contract.</exception>
        public void Respond<TResponse>(TResponse response)
        {
            if (ResponseType == null || !ResponseType.IsInstanceOfType(response))
                throw new InvalidOperationException($"Request '{Name}' expects response '{ResponseEventName ?? ResponseType?.FullName}'.");
            System.ComponentModel.DataAnnotations.Validator.ValidateObject(response,
                new System.ComponentModel.DataAnnotations.ValidationContext(response), true);
            ResponseHolder.Respond(this, Utilities.ObjectExtensions.ToTypedEventHolder(response));
        }

        internal static StructuralRequest Create(EventDefinition definition, Dictionary<string, object> values)
            => (StructuralRequest)Activator.CreateInstance(
                typeof(StructuralRequest<>).MakeGenericType(definition.ResponseType ?? typeof(StructuralEvent)),
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic,
                null, [definition.Name, values, definition.ResponseEventName], null);
    }

    /// <summary>A structural request typed to the CLR response contract.</summary>
    /// <typeparam name="TResponse">The response payload type.</typeparam>
    public sealed class StructuralRequest<TResponse> : StructuralRequest, IRequest<TResponse>
    {
        [JsonConstructor]
        internal StructuralRequest(string name, Dictionary<string, object> values, string responseEventName)
            : base(name, values, responseEventName, responseEventName == null ? typeof(TResponse) : null) { }
    }
}
