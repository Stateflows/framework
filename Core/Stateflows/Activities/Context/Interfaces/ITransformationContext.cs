using System;

namespace Stateflows.Activities.Context.Interfaces
{
    /// <summary>Context supplied to a flow transformation for the input token being transformed.</summary>
    /// <typeparam name="TToken">The input token type.</typeparam>
    public interface ITransformationContext<out TToken> : IActivityFlowContext<TToken>
    {
        /// <summary>Gets the canonical identifier of this transformation registration, matching visitor metadata.</summary>
        /// <remarks>This identifies the stage; the inherited flow identifier identifies its containing flow.</remarks>
        string TransformationId { get; }

        /// <summary>Gets the zero-based stage position in the flow pipeline, counting both guards and transformations.</summary>
        int TransformationOrder { get; }

        /// <summary>Gets the identity of the input token envelope at this stage, before a new output envelope is created.</summary>
        Guid TokenId { get; }

        /// <summary>Gets the declared input type of this stage, which can differ from the flow's original token type.</summary>
        Type InputTokenType { get; }

        /// <summary>Gets the declared output type of this stage, which can differ from the flow's final target token type.</summary>
        Type OutputTokenType { get; }
    }
    
    /// <summary>Transformation context that also exposes the value produced by the transformation.</summary>
    /// <typeparam name="TToken">The input token type.</typeparam>
    /// <typeparam name="TTransformedToken">The transformed token type.</typeparam>
    public interface ITransformationContext<out TToken, out TTransformedToken> : IActivityFlowContext<TToken>
    {
        /// <summary>The value produced for the current transformation operation.</summary>
        TTransformedToken TransformedToken { get; }
    }
}
