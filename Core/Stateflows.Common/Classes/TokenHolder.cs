using System;
using System.IO.Pipes;
using Newtonsoft.Json;
using Stateflows.Common;
using Stateflows.Common.Extensions;

namespace Stateflows.Common;

/// <summary>Provides a transport envelope for a token payload.</summary>
public abstract class TokenHolder
{
    /// <summary>Gets or sets the unique identifier of this token envelope.</summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>Stores the token name after it has been resolved.</summary>
    protected string name;
    /// <summary>Gets the token name used for structural delivery and identification.</summary>
    public virtual string Name => name ??= GetType().GetTokenName();
    
    private object boxedPayload;
    /// <summary>Gets the token payload boxed as an object.</summary>
    [JsonIgnore]
    public object BoxedPayload => boxedPayload ??= GetBoxedPayload();

    /// <summary>Returns the payload boxed as an object.</summary>
    /// <returns>The payload value.</returns>
    protected abstract object GetBoxedPayload();
    
    /// <summary>Gets the declared CLR type of the token payload.</summary>
    [JsonIgnore]
    public Type PayloadType => GetPayloadType();

    /// <summary>Returns the declared CLR type of the token payload.</summary>
    /// <returns>The payload type.</returns>
    protected abstract Type GetPayloadType();
    
    /// <summary>Gets the runtime CLR type of the payload, or its declared type when the payload is null.</summary>
    [JsonIgnore]
    public Type ActualPayloadType => GetActualPayloadType();

    /// <summary>Returns the runtime CLR type of the payload.</summary>
    /// <returns>The actual payload type, falling back to the declared type for a null payload.</returns>
    protected abstract Type GetActualPayloadType();
}

/// <summary>A token envelope that retains the payload's compile-time type.</summary>
/// <typeparam name="TToken">The token payload type.</typeparam>
public class TokenHolder<TToken> : TokenHolder
{
    /// <summary>Initializes an envelope with the default value of its payload type.</summary>
    public TokenHolder()
    {
        Payload = default;
    }

    /// <inheritdoc />
    public override string Name => name ??= typeof(TToken).GetReadableName(TypedElements.Tokens);

    /// <summary>Gets or sets the token payload.</summary>
    public TToken Payload { get; set; }

    /// <inheritdoc />
    protected override object GetBoxedPayload()
        => Payload;

    /// <inheritdoc />
    protected override Type GetPayloadType()
        => typeof(TToken);

    /// <inheritdoc />
    protected override Type GetActualPayloadType()
        => BoxedPayload?.GetType() ?? typeof(TToken);

    /// <inheritdoc />
    public override bool Equals(object obj)
        => obj is TokenHolder holder && holder.Id == Id;

    /// <inheritdoc />
    public override int GetHashCode()
        => Id.GetHashCode();
}
