namespace Stateflows.Activities.Registration.Interfaces.Base;

/// <summary>Adds publishing output boundaries to a root Activity.</summary>
/// <typeparam name="TReturn">The fluent builder type.</typeparam>
public interface IActivityOutputBase<out TReturn>
{
    /// <summary>Adds an output pin that publishes each token, with optional publication settings.</summary>
    /// <remarks>When <paramref name="name"/> is omitted, the pin is targeted with OutputPin&lt;TToken&gt;.</remarks>
    /// <param name="name">Optional globally unique node name; defaults to OutputPin&lt;TToken&gt;.Name.</param>
    /// <param name="buildAction">Optional callback configuring publication settings.</param>
    TReturn AddOutputPin<TToken>(string? name = null, PublishEventActionBuildAction buildAction = null);

    /// <summary>Adds an output pin that publishes each token, with optional publication settings.</summary>
    /// <param name="buildAction">Callback configuring publication settings.</param>
    TReturn AddOutputPin<TToken>(PublishEventActionBuildAction buildAction);
}
