namespace Stateflows.Activities.Context.Interfaces
{
    public interface ITransformationContext<out TToken> : IActivityFlowContext<TToken>
    { }
    
    public interface ITransformationContext<out TToken, out TTransformedToken> : IActivityFlowContext<TToken>
    {
        TTransformedToken TransformedToken { get; }
    }
}
