namespace Stateflows.Extensions.MinimalAPIs;

/// <summary>A batch of payloads for a typed Activity input pin.</summary>
/// <typeparam name="TToken">The pin's declared token type.</typeparam>
public sealed class ActivityInputBody<TToken>
{
    /// <summary>Payloads to inject together into the Activity network.</summary>
    public required TToken[] Tokens { get; set; }
}
