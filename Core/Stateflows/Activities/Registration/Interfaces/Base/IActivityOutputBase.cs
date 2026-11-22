using System.Collections.Generic;
using Stateflows.Activities.Registration.Builders;
using Stateflows.Common;
using Stateflows.Payloads;

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

    /// <summary>Adds a publishing output pin for payload tokens with the specified names.</summary>
    /// <param name="tokenNames">Payload token names emitted by the pin.</param>
    /// <param name="pinName">Optional globally unique node name.</param>
    /// <param name="buildAction">Optional callback configuring publication settings.</param>
    TReturn AddOutputPin(IReadOnlyList<string> tokenNames, string? pinName, PublishEventActionBuildAction buildAction)
        => PayloadFlowScope.With(tokenNames, nameof(tokenNames), () => AddOutputPin<Payload>(pinName, buildAction));

    /// <summary>Adds a publishing output pin for payload tokens with the specified names.</summary>
    /// <param name="tokenNames">Payload token names emitted by the pin.</param>
    /// <param name="buildAction">Optional callback configuring publication settings.</param>
    TReturn AddOutputPin(IReadOnlyList<string> tokenNames, PublishEventActionBuildAction buildAction)
        => AddOutputPin(tokenNames, null, buildAction);

    /// <summary>Adds a publishing output pin for payload tokens with the specified name.</summary>
    /// <param name="tokenName">Payload token name emitted by the pin.</param>
    /// <param name="pinName">Optional globally unique node name.</param>
    /// <param name="buildAction">Optional callback configuring publication settings.</param>
    TReturn AddOutputPin(string tokenName, string? pinName, PublishEventActionBuildAction buildAction)
        => AddOutputPin(new[] { tokenName }, pinName, buildAction);

    /// <summary>Adds a publishing output pin for payload tokens with the specified name.</summary>
    /// <param name="tokenName">Payload token name emitted by the pin.</param>
    /// <param name="buildAction">Optional callback configuring publication settings.</param>
    TReturn AddOutputPin(string tokenName, PublishEventActionBuildAction buildAction)
        => AddOutputPin(new[] { tokenName }, buildAction);
}
