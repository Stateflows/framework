using System;
using System.Threading.Tasks;
using Stateflows.Common;
using Stateflows.Activities.Context.Interfaces;

namespace Stateflows.Activities.Registration.Interfaces
{
    public delegate void ActivitiesBuilderAction(IActivitiesBuilder builder);

    public delegate Task<bool> GuardDelegateAsync<in TToken>(IGuardContext<TToken> context)
        where TToken : Token, new();

    public delegate Task<bool> GuardDelegateAsync(IGuardContext context);

    public delegate Task<TTargetToken> TransformationDelegateAsync<in TSourceToken, TTargetToken>(ITransformationContext<TSourceToken> context)
        where TSourceToken : Token, new()
        where TTargetToken : Token, new();

    public delegate Task ActionDelegateAsync(IActionContext context);

    public delegate Task ExceptionHandlerDelegateAsync<in TException>(IExceptionHandlerContext<TException> context)
        where TException : Exception;

    public delegate Task<TEvent> SendEventActionDelegateAsync<TEvent>(IActionContext context)
        where TEvent : Event, new();

    public delegate Task<TEvent> PublishEventActionDelegateAsync<TEvent>(IActionContext context)
        where TEvent : Event, new();

    public delegate Task<BehaviorId> BehaviorIdSelectorAsync(IActionContext context);

    public delegate Task AcceptEventActionDelegateAsync<in TEvent>(IAcceptEventActionContext<TEvent> context)
        where TEvent : Event, new();

    public delegate bool DecisionDelegate(IActionContext context);

    public delegate void ObjectFlowBuilderAction<in TToken>(IObjectFlowBuilder<TToken> builder)
        where TToken : Token, new();

    public delegate void ElseObjectFlowBuilderAction<in TToken>(IElseObjectFlowBuilder<TToken> builder)
        where TToken : Token, new();

    public delegate void ControlFlowBuilderAction(IControlFlowBuilder builder);

    public delegate void ElseControlFlowBuilderAction(IElseControlFlowBuilder builder);

    public delegate void ReactiveActivityBuilderAction(IActivityBuilder builder);

    internal delegate void NodeBuilderAction(NodeBuilder builder);

    public delegate void ActionBuilderAction(IActionBuilder builder);

    public delegate void TypedActionBuilderAction(ITypedActionBuilder builder);

    public delegate void JoinBuilderAction(IJoinBuilder builder);

    public delegate void InitialBuilderAction(IInitialBuilder builder);

    public delegate void InputBuilderAction(IInputBuilder builder);

    public delegate void ReactiveStructuredActivityBuilderAction(IReactiveStructuredActivityBuilder builder);

    public delegate void StructuredActivityBuilderAction(IStructuredActivityBuilder builder);

    public delegate void ParallelActivityBuilderAction(IStructuredActivityBuilder builder);

    public delegate void IterativeActivityBuilderAction(IStructuredActivityBuilder builder);

    public delegate void ForkBuilderAction(IForkBuilder builder);

    public delegate void MergeBuilderAction(IMergeBuilder builder);

    public delegate void DecisionBuilderAction(IDecisionBuilder builder);

    public delegate void DecisionBuilderAction<TToken>(IDecisionBuilder<TToken> builder)
        where TToken : Token, new();

    public delegate void DataStoreBuilderAction(IDataStoreBuilder builder);

    public delegate void TimeEventBuilderAction(ITimeEventBuilder builder);

    public delegate void AcceptEventActionBuilderAction(IAcceptEventActionBuilder builder);

    public delegate void SendEventActionBuilderAction(ISendEventActionBuilder builder);

    public delegate void PublishEventActionBuilderAction(IPublishEventActionBuilder builder);

    internal delegate void NodeValidationAction();

    public delegate IActivityObserver ActivityObserverFactory(IServiceProvider serviceProvider);

    public delegate IActivityInterceptor ActivityInterceptorFactory(IServiceProvider serviceProvider);

    public delegate IActivityExceptionHandler ActivityExceptionHandlerFactory(IServiceProvider serviceProvider);
}
