using System;
using System.Threading.Tasks;
using Stateflows.Common;
using Stateflows.Activities.Context.Interfaces;

namespace Stateflows.Activities.Registration.Interfaces
{
    public delegate Task<bool> GuardDelegateAsync<in TToken>(IGuardContext<TToken> context)
        where TToken : Token, new();

    public delegate Task<bool> GuardDelegateAsync(IGuardContext context);

    public delegate Task<TTargetToken> TransformationDelegateAsync<in TSourceToken, TTargetToken>(ITransformationContext<TSourceToken> context)
        where TSourceToken : Token, new()
        where TTargetToken : Token, new();

    public delegate Task ActionDelegateAsync(IActionContext context);

    public delegate Task ExceptionHandlerDelegateAsync<in TException>(IExceptionHandlerContext<TException> context)
        where TException : Exception;

    public delegate Task<TEvent> SignalActionDelegateAsync<TEvent>(IActionContext context)
        where TEvent : Event, new();

    public delegate Task<BehaviorId> BehaviorIdSelectorAsync(IActionContext context);

    public delegate Task EventActionDelegateAsync<in TEvent>(IEventContext<TEvent> context)
        where TEvent : Event, new();

    public delegate bool DecisionDelegate(IActionContext context);

    public delegate void FlowBuilderAction<in TToken>(IFlowBuilder<TToken> builder)
        where TToken : Token, new();

    public delegate void ControlFlowBuilderAction(IFlowBuilder builder);

    public delegate void ActivityBuilderAction(IActivityBuilder builder);

    internal delegate void NodeBuilderAction(NodeBuilder builder);

    public delegate void ActionBuilderAction(IActionBuilder builder);

    public delegate void TypedActionBuilderAction(ITypedActionBuilder builder);

    public delegate void JoinBuilderAction(IJoinBuilder builder);

    public delegate void InitialBuilderAction(IInitialBuilder builder);

    public delegate void InputBuilderAction(IInputBuilder builder);

    public delegate void EventActionBuilderAction(IEventActionBuilder builder);

    public delegate void StructuredActivityBuilderAction(IStructuredActivityBuilder builder);

    public delegate void ForkBuilderAction(IForkBuilder builder);

    public delegate void MergeBuilderAction(IMergeBuilder builder);

    public delegate void DecisionBuilderAction(IDecisionBuilder builder);

    public delegate void TimeEventBuilderAction(ITimeEventBuilder builder);

    public delegate void SignalActionBuilderAction(ISignalActionBuilder builder);

    internal delegate void NodeValidationAction();

    public delegate IActivityObserver ObserverFactory(IServiceProvider serviceProvider);

    public delegate IActivityInterceptor InterceptorFactory(IServiceProvider serviceProvider);

    public delegate IActivityExceptionHandler ExceptionHandlerFactory(IServiceProvider serviceProvider);
}
