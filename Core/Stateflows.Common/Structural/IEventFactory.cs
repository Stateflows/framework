using System;
using System.Collections.Generic;

namespace Stateflows.Common
{
    /// <summary>Configures the values of a structural event instance.</summary>
    /// <param name="builder">The event value builder.</param>
    public delegate void EventValueBuildAction(IEventValueBuilder builder);

    /// <summary>Builds top-level and nested values for a structural event.</summary>
    public interface IEventValueBuilder
    {
        /// <summary>Sets a scalar field value.</summary>
        /// <typeparam name="T">The value type.</typeparam>
        /// <param name="fieldName">The registered field name.</param>
        /// <param name="value">The field value.</param>
        /// <returns>This builder for fluent construction.</returns>
        IEventValueBuilder Set<T>(string fieldName, T value);
        /// <summary>Sets a nested object field using a nested value-building callback.</summary>
        /// <param name="fieldName">The registered object field name.</param>
        /// <param name="buildAction">Builds the nested object's values.</param>
        /// <returns>This builder for fluent construction.</returns>
        IEventValueBuilder SetObject(string fieldName, EventValueBuildAction buildAction);
        /// <summary>Sets a collection of nested objects.</summary>
        /// <param name="fieldName">The registered object-collection field name.</param>
        /// <param name="buildActions">One value-building callback per object in the collection.</param>
        /// <returns>This builder for fluent construction.</returns>
        IEventValueBuilder SetObjectCollection(string fieldName, params EventValueBuildAction[] buildActions);
    }

    /// <summary>Creates structural event instances from registered event definitions.</summary>
    public interface IEventFactory
    {
        /// <summary>Creates an event or request using a fluent value-building callback.</summary>
        /// <param name="eventName">The registered event name.</param>
        /// <param name="buildAction">Builds the event's field values.</param>
        /// <returns>The created structural event or request.</returns>
        StructuralEvent Create(string eventName, EventValueBuildAction buildAction);
        /// <summary>Creates an event or request from a field-value dictionary.</summary>
        /// <param name="eventName">The registered event name.</param>
        /// <param name="fields">Field values to include in the event.</param>
        /// <returns>The created structural event or request.</returns>
        StructuralEvent Create(string eventName, IReadOnlyDictionary<string, object> fields);
    }

    /// <summary>Exposes registered structural event definitions.</summary>
    public interface IEventsRegister
    {
        /// <summary>Gets all event definitions keyed by their registered names.</summary>
        IReadOnlyDictionary<string, EventDefinition> Events { get; }
        /// <summary>Gets a registered definition by name.</summary>
        /// <param name="eventName">The event name to look up.</param>
        /// <returns>The registered definition.</returns>
        EventDefinition GetEvent(string eventName);
    }
}
