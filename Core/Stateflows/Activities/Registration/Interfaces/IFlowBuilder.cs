namespace Stateflows.Activities.Registration.Interfaces
{
    public interface IFlowBuilder<out TToken> : IFlowWeight<IFlowBuilderWithWeight<TToken>>
        where TToken : Token, new()
    {
        IFlowBuilderWithWeight<TToken> AddGuard(GuardDelegateAsync<TToken> guardAsync);

        IFlowBuilderWithWeight<TTransformedToken> AddTransformation<TTransformedToken>(TransformationDelegateAsync<TToken, TTransformedToken> transformationAsync)
            where TTransformedToken : Token, new();
    }

    public interface IFlowBuilderWithWeight<out TToken>
        where TToken : Token, new()
    {
        IFlowBuilderWithWeight<TToken> AddGuard(GuardDelegateAsync<TToken> guardAsync);

        IFlowBuilderWithWeight<TTransformedToken> AddTransformation<TTransformedToken>(TransformationDelegateAsync<TToken, TTransformedToken> transformationAsync)
            where TTransformedToken : Token, new();
    }

    //public interface IFlowWeight<out TToken, out TReturn>
    //    where TToken : Token, new()
    //{
    //    IFlowBuilder<TToken> SetWeight(int weight);
    //}

    public interface IControlFlowBuilderBase<out TReturn>
    {
        TReturn AddGuard(GuardDelegateAsync guardAsync);
    }

    public interface IFlowBuilder :
        IControlFlowBuilderBase<IFlowBuilderWithWeight>,
        IFlowWeight<IFlowBuilderWithWeight>
    { }

    public interface IFlowBuilderWithWeight : 
        IControlFlowBuilderBase<IFlowBuilderWithWeight>
    { }

    public interface IFlowWeight<out TReturn>
    {
        TReturn SetWeight(int weight);
    }
}
