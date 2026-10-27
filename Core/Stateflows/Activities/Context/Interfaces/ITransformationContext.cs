namespace Stateflows.Activities.Context.Interfaces
{
    /// <summary>Context supplied to a flow transformation for the input token being transformed.</summary>
    /// <typeparam name="TToken">The input token type.</typeparam>
    public interface ITransformationContext<out TToken> : IActivityFlowContext<TToken>
    { }
    
    /// <summary>Transformation context that also exposes the value produced by the transformation.</summary>
    /// <typeparam name="TToken">The input token type.</typeparam>
    /// <typeparam name="TTransformedToken">The transformed token type.</typeparam>
    public interface ITransformationContext<out TToken, out TTransformedToken> : IActivityFlowContext<TToken>
    {
        /// <summary>The value produced for the current transformation operation.</summary>
        TTransformedToken TransformedToken { get; }
    }
}
