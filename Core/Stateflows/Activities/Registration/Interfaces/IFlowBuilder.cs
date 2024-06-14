namespace Stateflows.Activities.Registration.Interfaces
{
    public interface IObjectFlowBuilder<out TToken> :
        IFlowWeight<IObjectFlowBuilderWithWeight<TToken>>,
        IObjectFlowGuardBuilderBase<TToken, IObjectFlowBuilder<TToken>>,
        IObjectFlowTransformationBuilderBase<TToken, IObjectFlowBuilder<TToken>>
    { }

    public interface IObjectFlowBuilderWithWeight<out TToken> :
        IObjectFlowGuardBuilderBase<TToken, IObjectFlowBuilderWithWeight<TToken>>,
        IObjectFlowTransformationBuilderBase<TToken, IObjectFlowBuilderWithWeight<TToken>>
    { }

    public interface IElseObjectFlowBuilderWithWeight<out TToken> :
        IObjectFlowTransformationBuilderBase<TToken, IElseObjectFlowBuilderWithWeight<TToken>>
    { }

    public interface IElseObjectFlowBuilder<out TToken> :
        IFlowWeight<IElseObjectFlowBuilderWithWeight<TToken>>,
        IObjectFlowTransformationBuilderBase<TToken, IElseObjectFlowBuilder<TToken>>
    { }

    public interface IObjectFlowGuardBuilderBase<out TToken, out TReturn>
    {
        TReturn AddGuard(GuardDelegateAsync<TToken> guardAsync);
    }

    public interface IObjectFlowTransformationBuilderBase<out TToken, out TReturn>
    {
        TReturn AddTransformation<TTransformedToken>(TransformationDelegateAsync<TToken, TTransformedToken> transformationAsync);
    }

    public interface IControlFlowBuilderBase<out TReturn>
    {
        TReturn AddGuard(GuardDelegateAsync guardAsync);
    }

    public interface IControlFlowBuilder :
        IControlFlowBuilderBase<IControlFlowBuilderWithWeight>,
        IFlowWeight<IControlFlowBuilderWithWeight>
    { }

    public interface IControlFlowBuilderWithWeight : 
        IControlFlowBuilderBase<IControlFlowBuilderWithWeight>
    { }

    public interface IElseControlFlowBuilder :
        IFlowWeight
    { }

    public interface IElseControlFlowBuilderWithWeight
    { }

    public interface IFlowWeight<out TReturn>
    {
        TReturn SetWeight(int weight);
    }

    public interface IFlowWeight
    {
        void SetWeight(int weight);
    }
}
