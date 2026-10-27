namespace Stateflows.Actions.Registration.Interfaces.Base;

/// <summary>Configures action resource metadata and declared event/token dependencies.</summary>
/// <typeparam name="TReturn">The builder type returned for fluent configuration.</typeparam>
public interface IActionUtils<out TReturn>
{
    /// <summary>Sets the resource name associated with the action.</summary>
    /// <param name="resourceName">The resource identifier.</param>
    /// <returns>The current builder.</returns>
    TReturn SetResourceName(string resourceName);

    /// <summary>Sets the custom behavior class type exposed by this action.</summary>
    /// <param name="behaviorClassType">The custom behavior type name.</param>
    /// <returns>The current builder.</returns>
    TReturn SetCustomBehaviorClassType(string behaviorClassType);

    /// <summary>Declares whether action execution is stateless.</summary>
    /// <param name="isStateless"><see langword="true"/> when the action does not maintain instance state.</param>
    /// <returns>The current builder.</returns>
    TReturn SetIsStateless(bool isStateless);

    /// <summary>Declares that the action consumes events of the specified type.</summary>
    /// <typeparam name="TEvent">The event type consumed.</typeparam>
    /// <returns>The current builder.</returns>
    TReturn AddConsumedEvent<TEvent>();

    /// <summary>Declares that the action consumes events with the specified name.</summary>
    /// <param name="eventName">The event name.</param>
    /// <returns>The current builder.</returns>
    TReturn AddConsumedEvent(string eventName);

    /// <summary>Declares that the action produces events of the specified type.</summary>
    /// <typeparam name="TEvent">The event type produced.</typeparam>
    /// <returns>The current builder.</returns>
    TReturn AddProducedEvent<TEvent>();

    /// <summary>Declares that the action consumes tokens of the specified type.</summary>
    /// <typeparam name="TToken">The token type consumed.</typeparam>
    /// <returns>The current builder.</returns>
    TReturn AddConsumedToken<TToken>();

    /// <summary>Declares that the action produces tokens of the specified type.</summary>
    /// <typeparam name="TToken">The token type produced.</typeparam>
    /// <returns>The current builder.</returns>
    TReturn AddProducedToken<TToken>();
}
