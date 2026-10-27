using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Stateflows.Common;

namespace Stateflows.Events
{
    internal class FieldsBuilder<TReturn> : IFieldsBuilder<TReturn> where TReturn : class
    {
        internal readonly Dictionary<string, FieldDefinition> Fields = new(StringComparer.Ordinal);
        private TReturn Self => (TReturn)(object)this;

        public TReturn AddField(string fieldName, FieldType fieldType, FieldBuildAction buildAction = null)
        {
            if (!Enum.IsDefined(fieldType)) throw new ArgumentOutOfRangeException(nameof(fieldType));
            var builder = new FieldBuilder();
            buildAction?.Invoke(builder);
            Fields.Add(fieldName, builder.Build(fieldName, fieldType, false));
            return Self;
        }

        public TReturn AddObjectField(string fieldName, ObjectFieldBuildAction buildAction)
        {
            ArgumentNullException.ThrowIfNull(buildAction);
            var builder = new ObjectFieldBuilder();
            buildAction(builder);
            Fields.Add(fieldName, new FieldDefinition(fieldName, null, false, builder.Fields,
                builder.Validation, [], []));
            return Self;
        }

        public TReturn AddCollectionField(string fieldName, FieldType itemType, CollectionFieldBuildAction buildAction = null)
        {
            if (!Enum.IsDefined(itemType)) throw new ArgumentOutOfRangeException(nameof(itemType));
            var builder = new FieldBuilder();
            buildAction?.Invoke(builder);
            Fields.Add(fieldName, builder.Build(fieldName, itemType, true));
            return Self;
        }

        public TReturn AddCollectionField(string fieldName, ObjectCollectionFieldBuildAction buildAction)
        {
            ArgumentNullException.ThrowIfNull(buildAction);
            var builder = new ObjectCollectionFieldBuilder();
            buildAction(builder);
            Fields.Add(fieldName, new FieldDefinition(fieldName, null, true, builder.Fields,
                builder.Validation, [], []));
            return Self;
        }
    }

    internal sealed class EventBuilder : FieldsBuilder<IEventBuilder>, IEventBuilder;

    internal sealed class ObjectFieldBuilder : FieldsBuilder<IObjectFieldBuilder>, IObjectFieldBuilder
    {
        internal readonly List<ValidationAttribute> Validation = [];
        public IObjectFieldBuilder AddValidation(ValidationAttribute attribute)
        {
            ArgumentNullException.ThrowIfNull(attribute);
            Validation.Add(attribute);
            return this;
        }
    }

    internal sealed class ObjectCollectionFieldBuilder : FieldsBuilder<IObjectCollectionFieldBuilder>, IObjectCollectionFieldBuilder
    {
        internal readonly List<ValidationAttribute> Validation = [];
        public IObjectCollectionFieldBuilder AddValidation(ValidationAttribute attribute)
        {
            ArgumentNullException.ThrowIfNull(attribute);
            Validation.Add(attribute);
            return this;
        }
    }

    internal sealed class FieldBuilder : IFieldBuilder, ICollectionFieldBuilder
    {
        private readonly List<ValidationAttribute> validation = [];
        private readonly List<ValidationAttribute> itemValidation = [];
        private readonly HashSet<string> enumValues = new(StringComparer.Ordinal);

        public IFieldBuilder AddValidation(ValidationAttribute attribute)
        {
            ArgumentNullException.ThrowIfNull(attribute);
            validation.Add(attribute);
            return this;
        }
        ICollectionFieldBuilder ICollectionFieldBuilder.AddValidation(ValidationAttribute attribute)
        {
            AddValidation(attribute);
            return this;
        }
        public ICollectionFieldBuilder AddItemValidation(ValidationAttribute attribute)
        {
            ArgumentNullException.ThrowIfNull(attribute);
            itemValidation.Add(attribute);
            return this;
        }
        public IFieldBuilder AddEnumValues(params string[] values)
        {
            ArgumentNullException.ThrowIfNull(values);
            foreach (var value in values)
            {
                ArgumentException.ThrowIfNullOrWhiteSpace(value);
                if (!enumValues.Add(value)) throw new ArgumentException($"Enum value '{value}' is duplicated.");
            }
            return this;
        }
        ICollectionFieldBuilder ICollectionFieldBuilder.AddEnumValues(params string[] values)
        {
            AddEnumValues(values);
            return this;
        }
        internal FieldDefinition Build(string name, FieldType type, bool collection)
        {
            if (type != FieldType.Enum && enumValues.Count != 0)
                throw new ArgumentException($"Only enum fields can declare enum values: '{name}'.");
            return new(name, type, collection, new Dictionary<string, FieldDefinition>(), validation, itemValidation, enumValues);
        }
    }
}
