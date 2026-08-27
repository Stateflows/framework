using System;
using System.Threading.Tasks;
using Stateflows.Activities.Context.Interfaces;

namespace Stateflows.Activities.Registration.Interfaces
{
    public delegate void ActivitiesBuildAction(IActivitiesBuilder builder);

    public delegate Task<bool> GuardDelegateAsync<in TToken>(IGuardContext<TToken> context);

    public delegate Task<bool> GuardDelegateAsync(IGuardContext context);

    public delegate Task<TTransformedToken> TransformationDelegateAsync<in TToken, TTransformedToken>(ITransformationContext<TToken> context);

    public delegate Task ActionDelegateAsync(IActionContext context);

    public delegate Task ExceptionHandlerDelegateAsync<in TException>(IExceptionHandlerContext<TException> context)
        where TException : Exception;
    
    public delegate void ExceptionHandlerDelegate<in TException>(IExceptionHandlerContext<TException> context)
        where TException : Exception;

    public delegate Task<TEvent> SendEventActionDelegateAsync<TEvent>(IActionContext context)
;

    public delegate Task<TEvent> PublishEventActionDelegateAsync<TEvent>(IActionContext context)
;

    public delegate Task<BehaviorId> BehaviorIdSelectorAsync(IActionContext context);

    public delegate Task AcceptEventActionDelegateAsync<in TEvent>(IAcceptEventActionContext<TEvent> context)
;

    public delegate Task TimeEventActionDelegateAsync(IActionContext context);

    public delegate bool DecisionDelegate(IActionContext context);

    public delegate void ObjectFlowBuildAction<in TToken>(IObjectFlowBuilder<TToken> builder);

    public delegate void ElseObjectFlowBuildAction<in TToken>(IElseObjectFlowBuilder<TToken> builder);

    public delegate void ControlFlowBuildAction(IControlFlowBuilder builder);

    public delegate void ElseControlFlowBuildAction(IElseControlFlowBuilder builder);

    public delegate void ReactiveActivityBuildAction(IActivityBuilder builder);
    
    public delegate void ActivityUtilsBuildAction(IActivityUtilsBuilder builder);
    
    public delegate void OverridenActivityBuildAction(IOverridenActivityBuilder elementsBuilder);

    internal delegate void NodeBuildAction(NodeBuilder builder);

    public delegate void ActionBuildAction(IActionBuilder builder);
    
    public delegate void OverridenActionBuildAction(IOverridenActionBuilder builder);

    public delegate void TypedActionBuildAction<in TActionNode>(ITypedActionBuilder<TActionNode> builder)
        where TActionNode : class, IActionNode;

    public delegate void OverridenTypedActionBuildAction<in TActionNode>(IOverridenTypedActionBuilder<TActionNode> builder)
        where TActionNode : class, IActionNode;

    public delegate void JoinBuildAction(IJoinBuilder builder);
    
    public delegate void OverridenJoinBuildAction(IOverridenJoinBuilder builder);

    public delegate void InitialBuildAction(IInitialBuilder builder);
    
    public delegate void OverridenInitialBuildAction(IOverridenInitialBuilder builder);

    public delegate void InputBuildAction(IInputBuilder builder);
    
    public delegate void OverridenInputBuildAction(IOverridenInputBuilder builder);

    public delegate void ReactiveStructuredActivityBuildAction(IReactiveStructuredActivityBuilder builder);

    public delegate void ReactiveStructuredActivityExternalsBuildAction(IReactiveStructuredActivityExternalsBuilder builder);
    
    public delegate void OverridenReactiveStructuredActivityBuildAction(IOverridenReactiveStructuredActivityBuilder builder);
    
    public delegate void OverridenReactiveStructuredActivityExternalsBuildAction(IOverridenReactiveStructuredActivityExternalsBuilder builder);

    public delegate void StructuredActivityBuildAction(IStructuredActivityBuilder builder);
    
    public delegate void OverridenStructuredActivityBuildAction(IOverridenStructuredActivityBuilder builder);

    public delegate void ParallelActivityBuildAction(IStructuredActivityBuilder builder);
    
    public delegate void OverridenParallelActivityBuildAction(IOverridenStructuredActivityBuilder builder);

    public delegate void IterativeActivityBuildAction(IStructuredActivityBuilder builder);
    
    public delegate void OverridenIterativeActivityBuildAction(IOverridenStructuredActivityBuilder builder);

    public delegate void ForkBuildAction(IForkBuilder builder);
    
    public delegate void OverridenForkBuildAction(IOverridenForkBuilder builder);

    public delegate void MergeBuildAction(IMergeBuilder builder);
    
    public delegate void OverridenMergeBuildAction(IOverridenMergeBuilder builder);

    public delegate void DecisionBuildAction(IDecisionBuilder builder);
    
    public delegate void OverridenDecisionBuildAction(IOverridenDecisionBuilder builder);

    public delegate void DecisionBuildAction<in TToken>(IDecisionBuilder<TToken> builder);
    
    public delegate void OverridenDecisionBuildAction<in TToken>(IOverridenDecisionBuilder<TToken> builder);

    public delegate void DataStoreBuildAction(IDataStoreBuilder builder);
    
    public delegate void OverridenDataStoreBuildAction(IOverridenDataStoreBuilder builder);

    public delegate void TimeEventNodeBuildAction(ITimeEventActionBuilder builder);
    
    public delegate void OverridenTimeEventNodeBuildAction(IOverridenTimeEventActionBuilder builder);

    public delegate void TimeEventNodeBuildAction<in TTimeEventNode>(ITimeEventActionBuilder<TTimeEventNode> builder)
        where TTimeEventNode : class, ITimeEventActionNode;

    public delegate void OverridenTimeEventNodeBuildAction<in TTimeEventNode>(IOverridenTimeEventActionBuilder<TTimeEventNode> builder)
        where TTimeEventNode : class, ITimeEventActionNode;

    public delegate void AcceptEventActionBuildAction<TEvent>(IAcceptEventActionBuilder<TEvent> builder);
    
    public delegate void OverridenAcceptEventActionBuildAction<out TEvent>(IOverridenAcceptEventActionBuilder<TEvent> builder);
    
    public delegate void AcceptEventActionBuildAction<out TEvent, in TAcceptEventAction>(IAcceptEventActionBuilder<TEvent, TAcceptEventAction> builder)
        where TAcceptEventAction : class, IAcceptEventActionNode<TEvent>;
    
    public delegate void OverridenAcceptEventActionBuildAction<out TEvent, in TAcceptEventAction>(IOverridenAcceptEventActionBuilder<TEvent, TAcceptEventAction> builder)
        where TAcceptEventAction : class, IAcceptEventActionNode<TEvent>;

    public delegate void SendEventActionBuildAction(ISendEventActionBuilder builder);
    
    public delegate void OverridenSendEventActionBuildAction(IOverridenSendEventActionBuilder builder);

    public delegate void PublishEventActionBuildAction(IPublishEventActionBuilder builder);

    public delegate void OverridenPublishEventActionBuildAction(IOverridenPublishEventActionBuilder builder);

    internal delegate void NodeValidationAction();

    public delegate IActivityObserver ActivityObserverFactory(IServiceProvider serviceProvider);
    public delegate Task<IActivityObserver> ActivityObserverFactoryAsync(IServiceProvider serviceProvider);

    public delegate IActivityInterceptor ActivityInterceptorFactory(IServiceProvider serviceProvider);
    public delegate Task<IActivityInterceptor> ActivityInterceptorFactoryAsync(IServiceProvider serviceProvider);

    public delegate IActivityExceptionHandler ActivityExceptionHandlerFactory(IServiceProvider serviceProvider);
    public delegate Task<IActivityExceptionHandler> ActivityExceptionHandlerFactoryAsync(IServiceProvider serviceProvider);
}
