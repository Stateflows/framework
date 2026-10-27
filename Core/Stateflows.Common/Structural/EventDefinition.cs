using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;

namespace Stateflows.Common
{
    /// <summary>Describes a field in a registered structural event schema.</summary>
    public sealed class FieldDefinition
    {
        /// <summary>Gets the field's registered name.</summary>
        public string Name { get; }
        /// <summary>Gets the scalar type, or <see langword="null"/> for an object field.</summary>
        public FieldType? Type { get; }
        /// <summary>Gets whether this field contains a collection.</summary>
        public bool IsCollection { get; }
        /// <summary>Gets the nested field definitions for an object or object-collection field.</summary>
        public IReadOnlyDictionary<string, FieldDefinition> Fields { get; }
        /// <summary>Gets validation rules applied to the field value.</summary>
        public IReadOnlyList<ValidationAttribute> Validation { get; }
        /// <summary>Gets validation rules applied to each item of a collection.</summary>
        public IReadOnlyList<ValidationAttribute> ItemValidation { get; }
        /// <summary>Gets the allowed values for an enum field.</summary>
        public IReadOnlyList<string> EnumValues { get; }

        /// <summary>Creates an immutable field schema.</summary>
        /// <param name="name">The field name.</param>
        /// <param name="type">Scalar field type, or <see langword="null"/> for object fields.</param>
        /// <param name="isCollection">Whether the field is a collection.</param>
        /// <param name="fields">Nested field definitions for object fields.</param>
        /// <param name="validation">Validation rules for the field.</param>
        /// <param name="itemValidation">Validation rules for collection items.</param>
        /// <param name="enumValues">Allowed values for enum fields.</param>
        public FieldDefinition(string name, FieldType? type, bool isCollection,
            IDictionary<string, FieldDefinition> fields, IEnumerable<ValidationAttribute> validation,
            IEnumerable<ValidationAttribute> itemValidation, IEnumerable<string> enumValues)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(name);
            Name = name;
            Type = type;
            IsCollection = isCollection;
            Fields = new ReadOnlyDictionary<string, FieldDefinition>(new Dictionary<string, FieldDefinition>(fields, StringComparer.Ordinal));
            Validation = new List<ValidationAttribute>(validation).AsReadOnly();
            ItemValidation = new List<ValidationAttribute>(itemValidation).AsReadOnly();
            EnumValues = new List<string>(enumValues).AsReadOnly();
            if (type == FieldType.Enum && EnumValues.Count == 0)
                throw new ArgumentException($"Enum field '{name}' must declare allowed values.");
        }
    }

    /// <summary>Describes a registered structural event and its request-response contract, if any.</summary>
    public sealed class EventDefinition
    {
        /// <summary>Gets the registered event name.</summary>
        public string Name { get; }
        /// <summary>Gets the event's declared fields.</summary>
        public IReadOnlyDictionary<string, FieldDefinition> Fields { get; }
        /// <summary>Gets the registered structural response event name, when applicable.</summary>
        public string ResponseEventName { get; }
        /// <summary>Gets the CLR response type, when the request targets a CLR response.</summary>
        public Type ResponseType { get; }
        /// <summary>Gets whether the event defines a request-response contract.</summary>
        public bool IsRequest => ResponseEventName != null || ResponseType != null;

        /// <summary>Creates an event definition and optional typed response contract.</summary>
        /// <param name="name">The registered event name.</param>
        /// <param name="fields">The event's declared fields.</param>
        /// <param name="responseEventName">Name of a registered structural response event.</param>
        /// <param name="responseType">CLR response type for a typed request.</param>
        public EventDefinition(string name, IDictionary<string, FieldDefinition> fields,
            string responseEventName = null, Type responseType = null)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(name);
            Name = name;
            Fields = new ReadOnlyDictionary<string, FieldDefinition>(new Dictionary<string, FieldDefinition>(fields, StringComparer.Ordinal));
            ResponseEventName = responseEventName;
            ResponseType = responseType;
        }
    }
}
