using System;
using System.Threading.Tasks;
using Stateflows.Common;
using Stateflows.Activities.Context.Interfaces;

namespace Stateflows.Activities.Registration.Interfaces
{
    public delegate void ActivitiesBuildAction(IActivitiesBuilder builder);

    public delegate Task<bool> GuardDelegateAsync<in TToken>(IGuardContext<TToken> context);

    public delegate Task<bool> GuardDelegateAsync(IGuardContext context);

    public delegate Task<TTargetToken> TransformationDelegateAsync<in TSourceToken, TTargetToken>(ITransformationContext<TSourceToken> context);

    public delegate Task ActionDelegateAsync(IActionContext context);

    public delegate Task ExceptionHandlerDelegateAsync<in TException>(IExceptionHandlerContext<TException> context)
        where TException : Exception;

    public delegate Task<TEvent> SendEventActionDelegateAsync<TEvent>(IActionContext context);

    public delegate Task<TEvent> PublishEventActionDelegateAsync<TEvent>(IActionContext context);

    public delegate Task<BehaviorId> BehaviorIdSelectorAsync(IActionContext context);

    public delegate Task AcceptEventActionDelegateAsync<in TEvent>(IAcceptEventActionContext<TEvent> context);

    public delegate Task TimeEventActionDelegateAsync(IActionContext context);

    public delegate bool DecisionDelegate(IActionContext context);

    public delegate void ObjectFlowBuildAction<in TToken>(IObjectFlowBuilder<TToken> builder);

    public delegate void ElseObjectFlowBuildAction<in TToken>(IElseObjectFlowBuilder<TToken> builder);

    public delegate void ControlFlowBuildAction(IControlFlowBuilder builder);

    public delegate void ElseControlFlowBuildAction(IElseControlFlowBuilder builder);

    public delegate void ReactiveActivityBuildAction(IActivityBuilder builder);

    internal delegate void NodeBuildAction(NodeBuilder builder);

    public delegate void ActionBuildAction(IActionBuilder builder);

    public delegate void TypedActionBuildAction(ITypedActionBuilder builder);

    public delegate void JoinBuildAction(IJoinBuilder builder);

    public delegate void InitialBuildAction(IInitialBuilder builder);

    public delegate void InputBuildAction(IInputBuilder builder);

    public delegate void ReactiveStructuredActivityBuildAction(IReactiveStructuredActivityBuilder builder);

    public delegate void StructuredActivityBuildAction(IStructuredActivityBuilder builder);

    public delegate void ParallelActivityBuildAction(IStructuredActivityBuilder builder);

    public delegate void IterativeActivityBuildAction(IStructuredActivityBuilder builder);

    public delegate void ForkBuildAction(IForkBuilder builder);

    public delegate void MergeBuildAction(IMergeBuilder builder);

    public delegate void DecisionBuildAction(IDecisionBuilder builder);

    public delegate void DecisionBuildAction<TToken>(IDecisionBuilder<TToken> builder);

    public delegate void DataStoreBuildAction(IDataStoreBuilder builder);

    public delegate void TimeEventBuildAction(ITimeEventBuilder builder);

    public delegate void AcceptEventActionBuildAction(IAcceptEventActionBuilder builder);

    public delegate void SendEventActionBuildAction(ISendEventActionBuilder builder);

    public delegate void PublishEventActionBuildAction(IPublishEventActionBuilder builder);

    internal delegate void NodeValidationAction();

    public delegate IActivityObserver ActivityObserverFactory(IServiceProvider serviceProvider);

    public delegate IActivityInterceptor ActivityInterceptorFactory(IServiceProvider serviceProvider);

    public delegate IActivityExceptionHandler ActivityExceptionHandlerFactory(IServiceProvider serviceProvider);
}
