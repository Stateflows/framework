using System;
using System.Threading.Tasks;
using Stateflows.Activities.Context.Interfaces;

namespace Stateflows.Activities.Registration.Interfaces
{
    /// <summary>Configures a set of activity registrations.</summary>
    /// <param name="builder">Builder scoped to this Activity definition or element.</param>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/activities" />
    public delegate void ActivitiesBuildAction(IActivitiesBuilder builder);

    /// <summary>Evaluates whether an object-token flow is eligible.</summary>
    /// <typeparam name="TToken">Token type carried by the object flow.</typeparam>
    /// <param name="context">Context for evaluating this flow guard, including the candidate token when the flow
    /// carries object tokens.</param>
    /// <returns><see langword="true" /> when the flow may activate; otherwise, <see langword="false" />.</returns>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/activity-flows" />
    public delegate Task<bool> GuardDelegateAsync<in TToken>(IGuardContext<TToken> context);

    /// <summary>Evaluates whether a control flow is eligible.</summary>
    /// <param name="context">Context for evaluating this flow guard, including the candidate token when the flow
    /// carries object tokens.</param>
    /// <returns><see langword="true" /> when the flow may activate; otherwise, <see langword="false" />.</returns>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/activity-flows" />
    public delegate Task<bool> GuardDelegateAsync(IGuardContext context);

    /// <summary>Transforms a token as it passes through an object flow.</summary>
    /// <typeparam name="TToken">Token type carried by the object flow.</typeparam>
    /// <typeparam name="TTransformedToken">Token type produced by the object-flow transformation.</typeparam>
    /// <param name="context">Context containing the input token and cancellation token for this flow transformation.
    /// </param>
    /// <returns>The transformed token.</returns>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/activity-flows" />
    public delegate Task<TTransformedToken> TransformationDelegateAsync<in TToken, TTransformedToken>(ITransformationContext<TToken> context);

    /// <summary>Executes the body of an activity action node.</summary>
    /// <param name="context">Action context for the current node activation, including its inputs, outputs, and
    /// cancellation token.</param>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/activity-action-node" />
    public delegate Task ActionDelegateAsync(IActionContext context);

    /// <summary>Asynchronously handles an exception raised during activity-node execution.</summary>
    /// <typeparam name="TException">Exception type matched by this Activity handler; it must derive from
    /// <see cref="Exception" />.</typeparam>
    /// <param name="context">Context containing the exception, node execution details, cancellation token, and token
    /// output API.</param>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/activity-exception-handling" />
    public delegate Task ExceptionHandlerDelegateAsync<in TException>(IExceptionHandlerContext<TException> context)
        where TException : Exception;
    
    /// <summary>Synchronously handles an exception raised during activity-node execution.</summary>
    /// <typeparam name="TException">Exception type matched by this Activity handler; it must derive from
    /// <see cref="Exception" />.</typeparam>
    /// <param name="context">Context containing the exception, node execution details, cancellation token, and token
    /// output API.</param>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/activity-exception-handling" />
    public delegate void ExceptionHandlerDelegate<in TException>(IExceptionHandlerContext<TException> context)
        where TException : Exception;

    /// <summary>Creates an event to send from an activity node.</summary>
    /// <typeparam name="TEvent">Event payload type accepted by this event action node.</typeparam>
    /// <param name="context">Action context for the current node activation, including its inputs, outputs, and
    /// cancellation token.</param>
    /// <returns>The event to send.</returns>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/activity-send-event-action-node" />
    public delegate Task<TEvent> SendEventActionDelegateAsync<TEvent>(IActionContext context)
;

    /// <summary>Creates an event to publish from an activity node.</summary>
    /// <typeparam name="TEvent">Event payload type accepted by this event action node.</typeparam>
    /// <param name="context">Action context for the current node activation, including its inputs, outputs, and
    /// cancellation token.</param>
    /// <returns>The event to publish.</returns>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/activity-publish-event-action-node" />
    public delegate Task<TEvent> PublishEventActionDelegateAsync<TEvent>(IActionContext context)
;

    /// <summary>Selects the behavior instance that receives an event sent by an activity node.</summary>
    /// <param name="context">Action context for the current node activation, including its inputs, outputs, and
    /// cancellation token.</param>
    /// <returns>The target behavior identifier.</returns>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/activity-send-event-action-node" />
    public delegate Task<BehaviorId> BehaviorIdSelectorAsync(IActionContext context);

    /// <summary>Handles an event accepted by an activity node.</summary>
    /// <typeparam name="TEvent">Event payload type accepted by this event action node.</typeparam>
    /// <param name="context">Context containing the accepted event and the Activity node's action context.</param>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/activity-accept-event-action-node" />
    public delegate Task AcceptEventActionDelegateAsync<in TEvent>(IAcceptEventActionContext<TEvent> context)
;

    /// <summary>Executes an activity action in response to a time event.</summary>
    /// <param name="context">Action context for the current node activation, including its inputs, outputs, and
    /// cancellation token.</param>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/activity-time-event-action-node" />
    public delegate Task TimeEventActionDelegateAsync(IActionContext context);

    /// <summary>Selects a decision path based on the activity action context.</summary>
    /// <param name="context">Action context for the current node activation, including its inputs, outputs, and
    /// cancellation token.</param>
    /// <returns><see langword="true" /> when the associated decision path is selected.</returns>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/activity-decision-node" />
    public delegate bool DecisionDelegate(IActionContext context);

    /// <summary>Configures a typed object flow.</summary>
    /// <typeparam name="TToken">Token type carried by the object flow.</typeparam>
    /// <param name="builder">Builder scoped to this Activity definition or element.</param>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/activity-flows" />
    public delegate void ObjectFlowBuildAction<in TToken>(IObjectFlowBuilder<TToken> builder);

    /// <summary>Configures the fallback typed object flow of a decision.</summary>
    /// <typeparam name="TToken">Token type carried by the object flow.</typeparam>
    /// <param name="builder">Builder scoped to this Activity definition or element.</param>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/activity-flows" />
    public delegate void ElseObjectFlowBuildAction<in TToken>(IElseObjectFlowBuilder<TToken> builder);

    /// <summary>Configures a control flow.</summary>
    /// <param name="builder">Builder scoped to this Activity definition or element.</param>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/activity-flows" />
    public delegate void ControlFlowBuildAction(IControlFlowBuilder builder);

    /// <summary>Configures the fallback control flow of a decision.</summary>
    /// <param name="builder">Builder scoped to this Activity definition or element.</param>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/activity-flows" />
    public delegate void ElseControlFlowBuildAction(IElseControlFlowBuilder builder);

    /// <summary>Configures a reactive activity graph.</summary>
    /// <param name="builder">Builder scoped to this Activity definition or element.</param>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/activities" />
    public delegate void ReactiveActivityBuildAction(IActivityBuilder builder);
    
    /// <summary>Configures activity-wide utility options.</summary>
    /// <param name="builder">Builder scoped to this Activity definition or element.</param>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/observability" />
    public delegate void ActivityUtilsBuildAction(IActivityUtilsBuilder builder);
    
    /// <summary>Configures overrides for an inherited activity definition.</summary>
    /// <param name="elementsBuilder">The inherited activity override builder.</param>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/activity-action-node" />
    /// <seealso href="https://github.com/Stateflows/framework/wiki/behaviors#definition-reuse-and-versions" />
    public delegate void OverridenActivityBuildAction(IOverridenActivityBuilder elementsBuilder);

    internal delegate void NodeBuildAction(NodeBuilder builder);

    /// <summary>Configures an action node in an activity graph.</summary>
    /// <param name="builder">Builder scoped to this Activity definition or element.</param>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/activity-action-node" />
    public delegate void ActionBuildAction(IActionBuilder builder);
    
    /// <summary>Configures overrides for an inherited action node.</summary>
    /// <param name="builder">Builder scoped to this Activity definition or element.</param>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/activity-action-node" />
    /// <seealso href="https://github.com/Stateflows/framework/wiki/behaviors#definition-reuse-and-versions" />
    public delegate void OverridenActionBuildAction(IOverridenActionBuilder builder);

    /// <summary>Configures a typed action node.</summary>
    /// <typeparam name="TActionNode">Typed node implementation implementing <see cref="IActionNode" />.</typeparam>
    /// <param name="builder">Builder scoped to this Activity definition or element.</param>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/activity-action-node" />
    public delegate void TypedActionBuildAction<in TActionNode>(ITypedActionBuilder<TActionNode> builder)
        where TActionNode : class, IActionNode;

    /// <summary>Configures overrides for a typed action node.</summary>
    /// <typeparam name="TActionNode">Typed node implementation implementing <see cref="IActionNode" />.</typeparam>
    /// <param name="builder">Builder scoped to this Activity definition or element.</param>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/activity-action-node" />
    /// <seealso href="https://github.com/Stateflows/framework/wiki/behaviors#definition-reuse-and-versions" />
    public delegate void OverridenTypedActionBuildAction<in TActionNode>(IOverridenTypedActionBuilder<TActionNode> builder)
        where TActionNode : class, IActionNode;

    /// <summary>Configures a join node.</summary>
    /// <param name="builder">Builder scoped to this Activity definition or element.</param>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/activity-join-node" />
    public delegate void JoinBuildAction(IJoinBuilder builder);
    
    /// <summary>Configures overrides for a join node.</summary>
    /// <param name="builder">Builder scoped to this Activity definition or element.</param>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/behaviors#definition-reuse-and-versions" />
    /// <seealso href="https://github.com/Stateflows/framework/wiki/activity-join-node" />
    public delegate void OverridenJoinBuildAction(IOverridenJoinBuilder builder);

    /// <summary>Configures an initial node.</summary>
    /// <param name="builder">Builder scoped to this Activity definition or element.</param>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/activity-initial-node" />
    public delegate void InitialBuildAction(IInitialBuilder builder);
    
    /// <summary>Configures overrides for an initial node.</summary>
    /// <param name="builder">Builder scoped to this Activity definition or element.</param>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/behaviors#definition-reuse-and-versions" />
    /// <seealso href="https://github.com/Stateflows/framework/wiki/activity-initial-node" />
    public delegate void OverridenInitialBuildAction(IOverridenInitialBuilder builder);

    /// <summary>Configures an input node.</summary>
    /// <param name="builder">Builder scoped to this Activity definition or element.</param>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/activity-input-node" />
    public delegate void InputBuildAction(IInputBuilder builder);
    
    /// <summary>Configures overrides for an input node.</summary>
    /// <param name="builder">Builder scoped to this Activity definition or element.</param>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/behaviors#definition-reuse-and-versions" />
    /// <seealso href="https://github.com/Stateflows/framework/wiki/activity-input-node" />
    public delegate void OverridenInputBuildAction(IOverridenInputBuilder builder);

    /// <summary>Configures a reactive structured activity node.</summary>
    /// <param name="builder">Builder scoped to this Activity definition or element.</param>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/activity-structured-activity-node" />
    public delegate void ReactiveStructuredActivityBuildAction(IReactiveStructuredActivityBuilder builder);

    /// <summary>Configures the external inputs and outputs of a reactive structured activity.</summary>
    /// <param name="builder">Builder scoped to this Activity definition or element.</param>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/activity-structured-activity-node" />
    public delegate void ReactiveStructuredActivityExternalsBuildAction(IReactiveStructuredActivityExternalsBuilder builder);
    
    /// <summary>Configures overrides for a reactive structured activity.</summary>
    /// <param name="builder">Builder scoped to this Activity definition or element.</param>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/activity-structured-activity-node" />
    /// <seealso href="https://github.com/Stateflows/framework/wiki/behaviors#definition-reuse-and-versions" />
    public delegate void OverridenReactiveStructuredActivityBuildAction(IOverridenReactiveStructuredActivityBuilder builder);
    
    /// <summary>Configures overrides for a reactive structured activity's external flows.</summary>
    /// <param name="builder">Builder scoped to this Activity definition or element.</param>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/activity-structured-activity-node" />
    /// <seealso href="https://github.com/Stateflows/framework/wiki/behaviors#definition-reuse-and-versions" />
    public delegate void OverridenReactiveStructuredActivityExternalsBuildAction(IOverridenReactiveStructuredActivityExternalsBuilder builder);

    /// <summary>Configures a structured activity node.</summary>
    /// <param name="builder">Builder scoped to this Activity definition or element.</param>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/activity-structured-activity-node" />
    public delegate void StructuredActivityBuildAction(IStructuredActivityBuilder builder);
    
    /// <summary>Configures overrides for a structured activity node.</summary>
    /// <param name="builder">Builder scoped to this Activity definition or element.</param>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/activity-structured-activity-node" />
    /// <seealso href="https://github.com/Stateflows/framework/wiki/behaviors#definition-reuse-and-versions" />
    public delegate void OverridenStructuredActivityBuildAction(IOverridenStructuredActivityBuilder builder);

    /// <summary>Configures the child graph of a parallel activity node.</summary>
    /// <param name="builder">Builder scoped to this Activity definition or element.</param>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/activity-parallel-activity-node" />
    public delegate void ParallelActivityBuildAction(IStructuredActivityBuilder builder);
    
    /// <summary>Configures overrides for the child graph of a parallel activity node.</summary>
    /// <param name="builder">Builder scoped to this Activity definition or element.</param>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/behaviors#definition-reuse-and-versions" />
    /// <seealso href="https://github.com/Stateflows/framework/wiki/activity-parallel-activity-node" />
    public delegate void OverridenParallelActivityBuildAction(IOverridenStructuredActivityBuilder builder);

    /// <summary>Configures the child graph of an iterative activity node.</summary>
    /// <param name="builder">Builder scoped to this Activity definition or element.</param>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/activity-iterative-activity-node" />
    public delegate void IterativeActivityBuildAction(IStructuredActivityBuilder builder);
    
    /// <summary>Configures overrides for the child graph of an iterative activity node.</summary>
    /// <param name="builder">Builder scoped to this Activity definition or element.</param>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/behaviors#definition-reuse-and-versions" />
    /// <seealso href="https://github.com/Stateflows/framework/wiki/activity-iterative-activity-node" />
    public delegate void OverridenIterativeActivityBuildAction(IOverridenStructuredActivityBuilder builder);

    /// <summary>Configures a fork node's outgoing flows.</summary>
    /// <param name="builder">Builder scoped to this Activity definition or element.</param>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/activity-fork-node" />
    public delegate void ForkBuildAction(IForkBuilder builder);
    
    /// <summary>Configures overrides for a fork node's outgoing flows.</summary>
    /// <param name="builder">Builder scoped to this Activity definition or element.</param>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/behaviors#definition-reuse-and-versions" />
    /// <seealso href="https://github.com/Stateflows/framework/wiki/activity-fork-node" />
    public delegate void OverridenForkBuildAction(IOverridenForkBuilder builder);

    /// <summary>Configures a merge node's outgoing flows.</summary>
    /// <param name="builder">Builder scoped to this Activity definition or element.</param>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/activity-merge-node" />
    public delegate void MergeBuildAction(IMergeBuilder builder);
    
    /// <summary>Configures overrides for a merge node's outgoing flows.</summary>
    /// <param name="builder">Builder scoped to this Activity definition or element.</param>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/behaviors#definition-reuse-and-versions" />
    /// <seealso href="https://github.com/Stateflows/framework/wiki/activity-merge-node" />
    public delegate void OverridenMergeBuildAction(IOverridenMergeBuilder builder);

    /// <summary>Configures a control decision node.</summary>
    /// <param name="builder">Builder scoped to this Activity definition or element.</param>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/activity-decision-node" />
    public delegate void DecisionBuildAction(IDecisionBuilder builder);
    
    /// <summary>Configures overrides for a control decision node.</summary>
    /// <param name="builder">Builder scoped to this Activity definition or element.</param>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/activity-decision-node" />
    /// <seealso href="https://github.com/Stateflows/framework/wiki/behaviors#definition-reuse-and-versions" />
    public delegate void OverridenDecisionBuildAction(IOverridenDecisionBuilder builder);

    /// <summary>Configures a decision node over typed object flows.</summary>
    /// <typeparam name="TToken">Token type carried by the object flow.</typeparam>
    /// <param name="builder">Builder scoped to this Activity definition or element.</param>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/activity-decision-node" />
    public delegate void DecisionBuildAction<in TToken>(IDecisionBuilder<TToken> builder);
    
    /// <summary>Configures overrides for a typed decision node.</summary>
    /// <typeparam name="TToken">Token type carried by the object flow.</typeparam>
    /// <param name="builder">Builder scoped to this Activity definition or element.</param>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/activity-decision-node" />
    /// <seealso href="https://github.com/Stateflows/framework/wiki/behaviors#definition-reuse-and-versions" />
    public delegate void OverridenDecisionBuildAction<in TToken>(IOverridenDecisionBuilder<TToken> builder);

    /// <summary>Configures a data-store node's object flows.</summary>
    /// <param name="builder">Builder scoped to this Activity definition or element.</param>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/activity-data-store-node" />
    public delegate void DataStoreBuildAction(IDataStoreBuilder builder);
    
    /// <summary>Configures overrides for a data-store node's object flows.</summary>
    /// <param name="builder">Builder scoped to this Activity definition or element.</param>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/activity-action-node" />
    /// <seealso href="https://github.com/Stateflows/framework/wiki/behaviors#definition-reuse-and-versions" />
    public delegate void OverridenDataStoreBuildAction(IOverridenDataStoreBuilder builder);

    /// <summary>Configures a time-event action node.</summary>
    /// <param name="builder">Builder scoped to this Activity definition or element.</param>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/activity-time-event-action-node" />
    public delegate void TimeEventNodeBuildAction(ITimeEventActionBuilder builder);
    
    /// <summary>Configures overrides for a time-event action node.</summary>
    /// <param name="builder">Builder scoped to this Activity definition or element.</param>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/activity-time-event-action-node" />
    /// <seealso href="https://github.com/Stateflows/framework/wiki/behaviors#definition-reuse-and-versions" />
    public delegate void OverridenTimeEventNodeBuildAction(IOverridenTimeEventActionBuilder builder);

    /// <summary>Configures a typed time-event action node.</summary>
    /// <typeparam name="TTimeEventNode">Typed node implementation implementing <see cref="ITimeEventActionNode" />.
    /// </typeparam>
    /// <param name="builder">Builder scoped to this Activity definition or element.</param>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/activity-time-event-action-node" />
    public delegate void TimeEventNodeBuildAction<in TTimeEventNode>(ITimeEventActionBuilder<TTimeEventNode> builder)
        where TTimeEventNode : class, ITimeEventActionNode;

    /// <summary>Configures overrides for a typed time-event action node.</summary>
    /// <typeparam name="TTimeEventNode">Typed node implementation implementing <see cref="ITimeEventActionNode" />.
    /// </typeparam>
    /// <param name="builder">Builder scoped to this Activity definition or element.</param>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/activity-time-event-action-node" />
    /// <seealso href="https://github.com/Stateflows/framework/wiki/behaviors#definition-reuse-and-versions" />
    public delegate void OverridenTimeEventNodeBuildAction<in TTimeEventNode>(IOverridenTimeEventActionBuilder<TTimeEventNode> builder)
        where TTimeEventNode : class, ITimeEventActionNode;

    /// <summary>Configures an event-accepting action node.</summary>
    /// <typeparam name="TEvent">Event payload type accepted by this event action node.</typeparam>
    /// <param name="builder">Builder scoped to this Activity definition or element.</param>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/activity-accept-event-action-node" />
    public delegate void AcceptEventActionBuildAction<TEvent>(IAcceptEventActionBuilder<TEvent> builder);
    
    /// <summary>Configures overrides for an event-accepting action node.</summary>
    /// <typeparam name="TEvent">Event payload type accepted by this event action node.</typeparam>
    /// <param name="builder">Builder scoped to this Activity definition or element.</param>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/activity-accept-event-action-node" />
    /// <seealso href="https://github.com/Stateflows/framework/wiki/behaviors#definition-reuse-and-versions" />
    public delegate void OverridenAcceptEventActionBuildAction<out TEvent>(IOverridenAcceptEventActionBuilder<TEvent> builder);
    
    /// <summary>Configures a typed event-accepting action node.</summary>
    /// <typeparam name="TEvent">Event payload type accepted by this event action node.</typeparam>
    /// <typeparam name="TAcceptEventAction">Typed node implementation implementing
    /// <see cref="IAcceptEventActionNode&lt;TEvent&gt;" />.</typeparam>
    /// <param name="builder">Builder scoped to this Activity definition or element.</param>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/activity-accept-event-action-node" />
    public delegate void AcceptEventActionBuildAction<out TEvent, in TAcceptEventAction>(IAcceptEventActionBuilder<TEvent, TAcceptEventAction> builder)
        where TAcceptEventAction : class, IAcceptEventActionNode<TEvent>;
    
    /// <summary>Configures overrides for a typed event-accepting action node.</summary>
    /// <typeparam name="TEvent">Event payload type accepted by this event action node.</typeparam>
    /// <typeparam name="TAcceptEventAction">Typed node implementation implementing
    /// <see cref="IAcceptEventActionNode&lt;TEvent&gt;" />.</typeparam>
    /// <param name="builder">Builder scoped to this Activity definition or element.</param>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/activity-accept-event-action-node" />
    /// <seealso href="https://github.com/Stateflows/framework/wiki/behaviors#definition-reuse-and-versions" />
    public delegate void OverridenAcceptEventActionBuildAction<out TEvent, in TAcceptEventAction>(IOverridenAcceptEventActionBuilder<TEvent, TAcceptEventAction> builder)
        where TAcceptEventAction : class, IAcceptEventActionNode<TEvent>;

    /// <summary>Configures a send-event action node.</summary>
    /// <param name="builder">Builder scoped to this Activity definition or element.</param>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/activity-send-event-action-node" />
    public delegate void SendEventActionBuildAction(ISendEventActionBuilder builder);
    
    /// <summary>Configures overrides for a send-event action node.</summary>
    /// <param name="builder">Builder scoped to this Activity definition or element.</param>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/activity-send-event-action-node" />
    /// <seealso href="https://github.com/Stateflows/framework/wiki/behaviors#definition-reuse-and-versions" />
    public delegate void OverridenSendEventActionBuildAction(IOverridenSendEventActionBuilder builder);

    /// <summary>Configures a publish-event action node.</summary>
    /// <param name="builder">Builder scoped to this Activity definition or element.</param>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/activity-publish-event-action-node" />
    public delegate void PublishEventActionBuildAction(IPublishEventActionBuilder builder);

    /// <summary>Configures overrides for a publish-event action node.</summary>
    /// <param name="builder">Builder scoped to this Activity definition or element.</param>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/activity-publish-event-action-node" />
    /// <seealso href="https://github.com/Stateflows/framework/wiki/behaviors#definition-reuse-and-versions" />
    public delegate void OverridenPublishEventActionBuildAction(IOverridenPublishEventActionBuilder builder);

    internal delegate void NodeValidationAction();

    /// <summary>Creates an activity observer using the current service provider.</summary>
    /// <param name="serviceProvider">The service provider available to the activity runtime.</param>
    /// <returns>The observer instance.</returns>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/activities" />
    public delegate IActivityObserver ActivityObserverFactory(IServiceProvider serviceProvider);

    /// <summary>Asynchronously creates an activity observer using the current service provider.</summary>
    /// <param name="serviceProvider">The service provider available to the activity runtime.</param>
    /// <returns>A task containing the observer instance.</returns>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/activities" />
    public delegate Task<IActivityObserver> ActivityObserverFactoryAsync(IServiceProvider serviceProvider);

    /// <summary>Creates an activity interceptor using the current service provider.</summary>
    /// <param name="serviceProvider">The service provider available to the activity runtime.</param>
    /// <returns>The interceptor instance.</returns>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/activities" />
    public delegate IActivityInterceptor ActivityInterceptorFactory(IServiceProvider serviceProvider);

    /// <summary>Asynchronously creates an activity interceptor using the current service provider.</summary>
    /// <param name="serviceProvider">The service provider available to the activity runtime.</param>
    /// <returns>A task containing the interceptor instance.</returns>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/activities" />
    public delegate Task<IActivityInterceptor> ActivityInterceptorFactoryAsync(IServiceProvider serviceProvider);

    /// <summary>Creates an activity exception handler using the current service provider.</summary>
    /// <param name="serviceProvider">The service provider available to the activity runtime.</param>
    /// <returns>The exception-handler instance.</returns>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/activity-exception-handling" />
    public delegate IActivityExceptionHandler ActivityExceptionHandlerFactory(IServiceProvider serviceProvider);

    /// <summary>Asynchronously creates an activity exception handler using the current service provider.</summary>
    /// <param name="serviceProvider">The service provider available to the activity runtime.</param>
    /// <returns>A task containing the exception-handler instance.</returns>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/activity-exception-handling" />
    public delegate Task<IActivityExceptionHandler> ActivityExceptionHandlerFactoryAsync(IServiceProvider serviceProvider);
}
