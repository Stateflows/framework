using System;

namespace Stateflows.Common
{
    /// <summary>Provides event-name lookup for CLR and structural event payloads.</summary>
    public static class Event
    {
        /// <summary>Gets the registered name associated with an event type.</summary>
        /// <param name="type">The CLR event type.</param>
        /// <returns>The event name used by Stateflows messaging.</returns>
        public static string GetName(Type @type)
            => @type.GetEventName();

        /// <summary>Gets the name of an event payload, honoring a structural event's registered name.</summary>
        /// <param name="value">The event payload.</param>
        /// <returns>The event name used by Stateflows messaging.</returns>
        public static string GetName(object value)
            => value is StructuralEvent structural ? structural.Name : GetName(value.GetType());
    }

    /// <summary>Provides the Stateflows event name for a particular CLR event type.</summary>
    /// <typeparam name="TEvent">The event payload type.</typeparam>
    public static class Event<TEvent>
    {
        /// <summary>Gets the name used to identify <typeparamref name="TEvent"/> in messaging.</summary>
        public static string Name => Event.GetName(typeof(TEvent));
    }
}
