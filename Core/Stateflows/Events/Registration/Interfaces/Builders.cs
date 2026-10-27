using System.ComponentModel.DataAnnotations;
using Stateflows.Common;

namespace Stateflows.Events
{
    /// <summary>Configures registered structural events and requests.</summary>
    /// <remarks>See the <see href="https://github.com/Stateflows/framework/wiki/events">events documentation</see>.</remarks>
    /// <param name="builder">The structural-event registry builder.</param>
    public delegate void EventsBuildAction(IEventsBuilder builder);
    /// <summary>Configures the fields of an event or request definition.</summary>
    /// <param name="builder">The event field builder.</param>
    public delegate void EventBuildAction(IEventBuilder builder);
    /// <summary>Configures validation and allowed values for a scalar field.</summary>
    /// <param name="builder">The scalar field builder.</param>
    public delegate void FieldBuildAction(IFieldBuilder builder);
    /// <summary>Configures nested fields and validation for an object field.</summary>
    /// <param name="builder">The object field builder.</param>
    public delegate void ObjectFieldBuildAction(IObjectFieldBuilder builder);
    /// <summary>Configures validation for a collection of scalar values.</summary>
    /// <param name="builder">The scalar collection field builder.</param>
    public delegate void CollectionFieldBuildAction(ICollectionFieldBuilder builder);
    /// <summary>Configures nested fields and validation for a collection of objects.</summary>
    /// <param name="builder">The object collection field builder.</param>
    public delegate void ObjectCollectionFieldBuildAction(IObjectCollectionFieldBuilder builder);

    /// <summary>Registers structural event and request schemas.</summary>
    public interface IEventsBuilder
    {
        /// <summary>Registers a one-way structural event with the specified fields.</summary>
        /// <param name="eventName">Unique event name.</param>
        /// <param name="buildAction">Declares the event fields and their validation.</param>
        /// <returns>This builder for fluent registration.</returns>
        IEventsBuilder AddEvent(string eventName, EventBuildAction buildAction);
        /// <summary>Registers a structural request whose response is another registered structural event.</summary>
        /// <param name="eventName">Unique request event name.</param>
        /// <param name="responseEventName">Name of the registered structural response event.</param>
        /// <param name="buildAction">Declares request fields and their validation.</param>
        /// <returns>This builder for fluent registration.</returns>
        IEventsBuilder AddRequest(string eventName, string responseEventName, EventBuildAction buildAction);
        /// <summary>Registers a structural request whose response is a CLR event type.</summary>
        /// <typeparam name="TResponse">The CLR response type.</typeparam>
        /// <param name="eventName">Unique request event name.</param>
        /// <param name="buildAction">Declares request fields and their validation.</param>
        /// <returns>This builder for fluent registration.</returns>
        IEventsBuilder AddRequest<TResponse>(string eventName, EventBuildAction buildAction);
    }

    /// <summary>Provides fluent registration of fields on a structural value.</summary>
    /// <typeparam name="TReturn">The builder returned by each field operation.</typeparam>
    public interface IFieldsBuilder<out TReturn>
    {
        /// <summary>Adds a scalar field.</summary>
        /// <param name="fieldName">Unique field name within the containing object.</param>
        /// <param name="fieldType">The field's scalar data type.</param>
        /// <param name="buildAction">Optional field validation and enum-value configuration.</param>
        /// <returns>The enclosing builder.</returns>
        TReturn AddField(string fieldName, FieldType fieldType, FieldBuildAction buildAction = null);
        /// <summary>Adds a nested object field.</summary>
        /// <param name="fieldName">Unique field name within the containing object.</param>
        /// <param name="buildAction">Declares nested fields and validation.</param>
        /// <returns>The enclosing builder.</returns>
        TReturn AddObjectField(string fieldName, ObjectFieldBuildAction buildAction);
        /// <summary>Adds a collection of scalar values.</summary>
        /// <param name="fieldName">Unique field name within the containing object.</param>
        /// <param name="itemType">The scalar type of each collection item.</param>
        /// <param name="buildAction">Optional collection and item validation configuration.</param>
        /// <returns>The enclosing builder.</returns>
        TReturn AddCollectionField(string fieldName, FieldType itemType, CollectionFieldBuildAction buildAction = null);
        /// <summary>Adds a collection of nested objects.</summary>
        /// <param name="fieldName">Unique field name within the containing object.</param>
        /// <param name="buildAction">Declares the nested fields and validation for each item.</param>
        /// <returns>The enclosing builder.</returns>
        TReturn AddCollectionField(string fieldName, ObjectCollectionFieldBuildAction buildAction);
    }

    /// <summary>Configures fields on a structural event definition.</summary>
    public interface IEventBuilder : IFieldsBuilder<IEventBuilder>;

    /// <summary>Configures validation and allowed values for a scalar field.</summary>
    public interface IFieldBuilder
    {
        /// <summary>Adds a validation rule for the field value.</summary>
        /// <param name="attribute">The data-annotation validation attribute.</param>
        /// <returns>This builder for fluent configuration.</returns>
        IFieldBuilder AddValidation(ValidationAttribute attribute);
        /// <summary>Declares the allowed string values for an enum field.</summary>
        /// <param name="values">Allowed enum values.</param>
        /// <returns>This builder for fluent configuration.</returns>
        IFieldBuilder AddEnumValues(params string[] values);
    }

    /// <summary>Configures validation for a nested object field.</summary>
    public interface IObjectFieldBuilder : IFieldsBuilder<IObjectFieldBuilder>
    {
        /// <summary>Adds a validation rule for the object field.</summary>
        /// <param name="attribute">The data-annotation validation attribute.</param>
        /// <returns>This builder for fluent configuration.</returns>
        IObjectFieldBuilder AddValidation(ValidationAttribute attribute);
    }

    /// <summary>Configures validation for a collection of scalar values.</summary>
    public interface ICollectionFieldBuilder
    {
        /// <summary>Adds a validation rule for the collection itself.</summary>
        /// <param name="attribute">The data-annotation validation attribute.</param>
        /// <returns>This builder for fluent configuration.</returns>
        ICollectionFieldBuilder AddValidation(ValidationAttribute attribute);
        /// <summary>Adds a validation rule applied separately to each item.</summary>
        /// <param name="attribute">The data-annotation validation attribute.</param>
        /// <returns>This builder for fluent configuration.</returns>
        ICollectionFieldBuilder AddItemValidation(ValidationAttribute attribute);
        /// <summary>Declares allowed string values for each item of an enum collection.</summary>
        /// <param name="values">Allowed enum values.</param>
        /// <returns>This builder for fluent configuration.</returns>
        ICollectionFieldBuilder AddEnumValues(params string[] values);
    }

    /// <summary>Configures validation for a collection of nested objects.</summary>
    public interface IObjectCollectionFieldBuilder : IFieldsBuilder<IObjectCollectionFieldBuilder>
    {
        /// <summary>Adds a validation rule for the collection itself.</summary>
        /// <param name="attribute">The data-annotation validation attribute.</param>
        /// <returns>This builder for fluent configuration.</returns>
        IObjectCollectionFieldBuilder AddValidation(ValidationAttribute attribute);
    }
}
