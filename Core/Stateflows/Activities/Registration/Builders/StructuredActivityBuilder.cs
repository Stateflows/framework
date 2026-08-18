using System;
using System.Threading.Tasks;
using Stateflows.Activities.Models;
using Stateflows.Activities.Registration.Interfaces;
using Stateflows.Activities.Registration.Interfaces.Base;
using Stateflows.Common.Registration;
using IActionContext = Stateflows.Activities.Context.Interfaces.IActionContext;

namespace Stateflows.Activities.Registration.Builders
{
    internal class StructuredActivityBuilder :
        BaseActivityBuilder,
        IActionBuilder,
        IReactiveStructuredActivityBuilder,
        IOverridenReactiveStructuredActivityBuilder,
        IStructuredActivityBuilder,
        IOverridenStructuredActivityBuilder,
        IBehaviorBuilder,
        INodeBuilder
    {
        public NodeBuilder NodeBuilder { get; set; }

        public StructuredActivityBuilder(Node parentNode, BaseActivityBuilder parentActivityBuilder)
            : base(parentNode)
        {
            NodeBuilder = new NodeBuilder(Node, parentActivityBuilder);
        }

        public IActionBuilder AddFlow<TToken>(string targetNodeName, ObjectFlowBuildAction<TToken> buildAction = null)
        {
            NodeBuilder.AddFlow<TToken>(targetNodeName, buildAction);

            return this;
        }

        public IActionBuilder AddElseFlow<TToken>(string targetNodeName, ElseObjectFlowBuildAction<TToken> buildAction = null)
        {
            NodeBuilder.AddElseFlow<TToken>(targetNodeName, buildAction);

            return this;
        }

        public IActionBuilder AddControlFlow(string targetNodeName, ControlFlowBuildAction buildAction = null)
        {
            NodeBuilder.AddControlFlow(targetNodeName, buildAction);

            return this;
        }

        public IActionBuilder AddElseControlFlow(string targetNodeName, ElseControlFlowBuildAction buildAction = null)
        {
            NodeBuilder.AddElseControlFlow(targetNodeName, buildAction);

            return this;
        }

        public IActionBuilder AddExceptionHandler<TException>(ExceptionHandlerDelegateAsync<TException> exceptionHandler)
            where TException : Exception
        {
            NodeBuilder.AddExceptionHandler<TException>(exceptionHandler);

            return this;
        }

        public IActionBuilder SetOptions(NodeOptions nodeOptions)
        {
            NodeBuilder.SetOptions(nodeOptions);

            return this;
        }

        IActionBuilder INodeOptions<IActionBuilder>.UpdateOptions(Func<NodeOptions, NodeOptions> nodeOptionsUpdater)
            => UpdateOptions(nodeOptionsUpdater) as IActionBuilder;

        IOverridenStructuredActivityBuilder INodeOptions<IOverridenStructuredActivityBuilder>.UpdateOptions(Func<NodeOptions, NodeOptions> nodeOptionsUpdater)
            => UpdateOptions(nodeOptionsUpdater) as IOverridenStructuredActivityBuilder;

        IOverridenReactiveStructuredActivityBuilder INodeOptions<IOverridenReactiveStructuredActivityBuilder>.UpdateOptions(Func<NodeOptions, NodeOptions> nodeOptionsUpdater)
            => UpdateOptions(nodeOptionsUpdater) as IOverridenReactiveStructuredActivityBuilder;

        IReactiveStructuredActivityBuilder INodeOptions<IReactiveStructuredActivityBuilder>.UpdateOptions(Func<NodeOptions, NodeOptions> nodeOptionsUpdater)
             => UpdateOptions(nodeOptionsUpdater) as IReactiveStructuredActivityBuilder;

        public IStructuredActivityBuilder UpdateOptions(Func<NodeOptions, NodeOptions> nodeOptionsUpdater)
        {
            Node.Options = nodeOptionsUpdater(Node.Options);

            return this;
        }

        #region IReactiveStructuredActivityBuilder
        IReactiveStructuredActivityBuilder IActivityActionBase<IReactiveStructuredActivityBuilder>.AddAction(string actionNodeName, Func<IActionContext, Task> actionAsync, ActionBuildAction buildAction)
            => AddAction(actionNodeName, actionAsync, b => buildAction?.Invoke(b)) as IReactiveStructuredActivityBuilder;

        IReactiveStructuredActivityBuilder IControlFlowBase<IReactiveStructuredActivityBuilder>.AddControlFlow(string targetNodeName, ControlFlowBuildAction buildAction)
            => AddControlFlow(targetNodeName, buildAction) as IReactiveStructuredActivityBuilder;

        IReactiveStructuredActivityBuilder IFinalBase<IReactiveStructuredActivityBuilder>.AddFinal()
            => AddFinal() as IReactiveStructuredActivityBuilder;

        IReactiveStructuredActivityBuilder IInitialBase<IReactiveStructuredActivityBuilder>.AddInitial(InitialBuildAction buildAction)
            => AddInitial(buildAction) as IReactiveStructuredActivityBuilder;

        IReactiveStructuredActivityBuilder IInputBase<IReactiveStructuredActivityBuilder>.AddInput(InputBuildAction buildAction)
            => AddInput(buildAction) as IReactiveStructuredActivityBuilder;

        IReactiveStructuredActivityBuilder IObjectFlowBase<IReactiveStructuredActivityBuilder>.AddFlow<TToken>(string targetNodeName, ObjectFlowBuildAction<TToken> buildAction)
            => AddFlow<TToken>(targetNodeName, buildAction) as IReactiveStructuredActivityBuilder;

        IOverridenReactiveStructuredActivityBuilder IStructuredActivityEvents<IOverridenReactiveStructuredActivityBuilder>.AddOnInitialize(Func<IActivityNodeContext, Task> actionAsync)
            => AddOnInitialize(actionAsync) as IOverridenReactiveStructuredActivityBuilder;

        IOverridenStructuredActivityBuilder IStructuredActivityEvents<IOverridenStructuredActivityBuilder>.AddOnFinalize(Func<IActivityNodeContext, Task> actionAsync)
            => AddOnFinalize(actionAsync) as IOverridenStructuredActivityBuilder;

        IOverridenStructuredActivityBuilder IStructuredActivityEvents<IOverridenStructuredActivityBuilder>.AddOnInitialize(Func<IActivityNodeContext, Task> actionAsync)
            => AddOnInitialize(actionAsync) as IOverridenStructuredActivityBuilder;

        IOverridenReactiveStructuredActivityBuilder IStructuredActivityEvents<IOverridenReactiveStructuredActivityBuilder>.AddOnFinalize(Func<IActivityNodeContext, Task> actionAsync)
            => AddOnFinalize(actionAsync) as IOverridenReactiveStructuredActivityBuilder;

        IReactiveStructuredActivityBuilder IStructuredActivityEvents<IReactiveStructuredActivityBuilder>.AddOnFinalize(Func<IActivityNodeContext, Task> actionAsync)
            => AddOnFinalize(actionAsync) as IReactiveStructuredActivityBuilder;

        IReactiveStructuredActivityBuilder IStructuredActivityEvents<IReactiveStructuredActivityBuilder>.AddOnInitialize(Func<IActivityNodeContext, Task> actionAsync)
            => AddOnInitialize(actionAsync) as IReactiveStructuredActivityBuilder;

        IReactiveStructuredActivityBuilder IOutputBase<IReactiveStructuredActivityBuilder>.AddOutput()
            => AddOutput() as IReactiveStructuredActivityBuilder;

        IReactiveStructuredActivityBuilder IReactiveActivityBase<IReactiveStructuredActivityBuilder>.AddStructuredActivity(string actionNodeName, ReactiveStructuredActivityBuildAction buildAction)
            => AddStructuredActivity(actionNodeName, buildAction) as IReactiveStructuredActivityBuilder;

        IOverridenStructuredActivityBuilder IActivityBase<IOverridenStructuredActivityBuilder>.AddStructuredActivity(string actionNodeName,
            StructuredActivityBuildAction buildAction)
            => AddStructuredActivity(actionNodeName, b => buildAction?.Invoke(b as IStructuredActivityBuilder)) as IOverridenStructuredActivityBuilder;

        IOverridenStructuredActivityBuilder IActivityBase<IOverridenStructuredActivityBuilder>.AddParallelActivity<TParallelizationToken>(string actionNodeName,
            ParallelActivityBuildAction buildAction, int chunkSize)
            => AddParallelActivity<TParallelizationToken>(actionNodeName, buildAction, chunkSize) as IOverridenStructuredActivityBuilder;

        IOverridenStructuredActivityBuilder IActivityBase<IOverridenStructuredActivityBuilder>.AddIterativeActivity<TIterationToken>(string actionNodeName,
            IterativeActivityBuildAction buildAction, int chunkSize)
            => AddIterativeActivity<TIterationToken>(actionNodeName, buildAction, chunkSize) as IOverridenStructuredActivityBuilder;

        IOverridenReactiveStructuredActivityBuilder IReactiveActivityBase<IOverridenReactiveStructuredActivityBuilder>.AddParallelActivity<TParallelizationToken>(string actionNodeName,
            ParallelActivityBuildAction buildAction, int chunkSize)
            => AddParallelActivity<TParallelizationToken>(actionNodeName, buildAction, chunkSize) as IOverridenReactiveStructuredActivityBuilder;

        IOverridenReactiveStructuredActivityBuilder IReactiveActivityBase<IOverridenReactiveStructuredActivityBuilder>.AddIterativeActivity<TIterationToken>(string actionNodeName,
            IterativeActivityBuildAction buildAction, int chunkSize)
            => AddIterativeActivity<TIterationToken>(actionNodeName, buildAction, chunkSize) as IOverridenReactiveStructuredActivityBuilder;

        IOverridenReactiveStructuredActivityBuilder IReactiveActivityBase<IOverridenReactiveStructuredActivityBuilder>.AddStructuredActivity(string actionNodeName,
            ReactiveStructuredActivityBuildAction buildAction)
            => AddStructuredActivity(actionNodeName, buildAction) as IOverridenReactiveStructuredActivityBuilder;

        IReactiveStructuredActivityBuilder IReactiveActivityBase<IReactiveStructuredActivityBuilder>.AddParallelActivity<TToken>(string actionNodeName, ParallelActivityBuildAction buildAction, int chunkSize)
            => AddParallelActivity<TToken>(actionNodeName, buildAction, chunkSize) as IReactiveStructuredActivityBuilder;

        IReactiveStructuredActivityBuilder IReactiveActivityBase<IReactiveStructuredActivityBuilder>.AddIterativeActivity<TIterationToken>(string actionNodeName, IterativeActivityBuildAction buildAction, int chunkSize)
            => AddIterativeActivity<TIterationToken>(actionNodeName, buildAction, chunkSize) as IReactiveStructuredActivityBuilder;

        IReactiveStructuredActivityBuilder IExceptionHandlerBase<IReactiveStructuredActivityBuilder>.AddExceptionHandler<TException>(ExceptionHandlerDelegateAsync<TException> exceptionHandler)
            => AddExceptionHandler<TException>(exceptionHandler) as IReactiveStructuredActivityBuilder;

        IReactiveStructuredActivityBuilder ISendEventBase<IReactiveStructuredActivityBuilder>.AddSendEventAction<TEvent>(string actionNodeName, SendEventActionDelegateAsync<TEvent> actionAsync, BehaviorIdSelectorAsync targetSelectorAsync, SendEventActionBuildAction buildAction)
            => AddSendEventAction<TEvent>(actionNodeName, actionAsync, targetSelectorAsync, buildAction) as IReactiveStructuredActivityBuilder;

        IReactiveStructuredActivityBuilder IAcceptEventBase<IReactiveStructuredActivityBuilder>.AddAcceptEventAction<TEvent>(string actionNodeName, AcceptEventActionDelegateAsync<TEvent> eventActionAsync, AcceptEventActionBuildAction<TEvent> buildAction)
            => AddAcceptEventAction(actionNodeName, eventActionAsync, buildAction) as IReactiveStructuredActivityBuilder;

        IOverridenReactiveStructuredActivityBuilder IAcceptEventBase<IOverridenReactiveStructuredActivityBuilder>.AddTimeEventAction<TTimeEvent>(string actionNodeName,
            TimeEventActionDelegateAsync eventActionAsync, TimeEventNodeBuildAction buildAction)
            => AddTimeEventAction<TTimeEvent>(actionNodeName, eventActionAsync, buildAction) as IOverridenReactiveStructuredActivityBuilder;

        IOverridenReactiveStructuredActivityBuilder IAcceptEventBase<IOverridenReactiveStructuredActivityBuilder>.AddAcceptEventAction<TEvent>(string actionNodeName,
            AcceptEventActionDelegateAsync<TEvent> eventActionAsync, AcceptEventActionBuildAction<TEvent> buildAction)
            => AddAcceptEventAction<TEvent>(actionNodeName, eventActionAsync, buildAction) as IOverridenReactiveStructuredActivityBuilder;

        IReactiveStructuredActivityBuilder IAcceptEventBase<IReactiveStructuredActivityBuilder>.AddTimeEventAction<TTimeEvent>(string actionNodeName, TimeEventActionDelegateAsync eventActionAsync, TimeEventNodeBuildAction buildAction)
            => AddTimeEventAction<TTimeEvent>(actionNodeName, eventActionAsync, buildAction) as IReactiveStructuredActivityBuilder;
        #endregion

        #region IStructuredActivityBuilder
        IStructuredActivityBuilder IActivityActionBase<IStructuredActivityBuilder>.AddAction(string actionNodeName, Func<IActionContext, Task> actionAsync, ActionBuildAction buildAction)
            => AddAction(actionNodeName, actionAsync, b => buildAction?.Invoke(b)) as IStructuredActivityBuilder;

        IStructuredActivityBuilder IControlFlowBase<IStructuredActivityBuilder>.AddControlFlow(string targetNodeName, ControlFlowBuildAction buildAction)
            => AddControlFlow(targetNodeName, buildAction) as IStructuredActivityBuilder;

        IStructuredActivityBuilder IFinalBase<IStructuredActivityBuilder>.AddFinal()
            => AddFinal() as IStructuredActivityBuilder;

        IStructuredActivityBuilder IInitialBase<IStructuredActivityBuilder>.AddInitial(InitialBuildAction buildAction)
            => AddInitial(buildAction) as IStructuredActivityBuilder;

        IStructuredActivityBuilder IInputBase<IStructuredActivityBuilder>.AddInput(InputBuildAction buildAction)
            => AddInput(buildAction) as IStructuredActivityBuilder;

        IStructuredActivityBuilder IObjectFlowBase<IStructuredActivityBuilder>.AddFlow<TToken>(string targetNodeName, ObjectFlowBuildAction<TToken> buildAction)
            => AddFlow<TToken>(targetNodeName, buildAction) as IStructuredActivityBuilder;

        IStructuredActivityBuilder IStructuredActivityEvents<IStructuredActivityBuilder>.AddOnFinalize(Func<IActivityNodeContext, Task> actionAsync)
            => AddOnFinalize(actionAsync) as IStructuredActivityBuilder;

        IStructuredActivityBuilder IStructuredActivityEvents<IStructuredActivityBuilder>.AddOnInitialize(Func<IActivityNodeContext, Task> actionAsync)
            => AddOnInitialize(actionAsync) as IStructuredActivityBuilder;

        IStructuredActivityBuilder IOutputBase<IStructuredActivityBuilder>.AddOutput()
            => AddOutput() as IStructuredActivityBuilder;

        IStructuredActivityBuilder IActivityBase<IStructuredActivityBuilder>.AddStructuredActivity(string actionNodeName, StructuredActivityBuildAction buildAction)
            => AddStructuredActivity(actionNodeName, b => buildAction?.Invoke(b as IStructuredActivityBuilder)) as IStructuredActivityBuilder;

        IStructuredActivityBuilder IActivityBase<IStructuredActivityBuilder>.AddParallelActivity<TToken>(string actionNodeName, ParallelActivityBuildAction buildAction, int chunkSize)
            => AddParallelActivity<TToken>(actionNodeName, buildAction, chunkSize) as IStructuredActivityBuilder;

        IStructuredActivityBuilder IActivityBase<IStructuredActivityBuilder>.AddIterativeActivity<TIterationToken>(string actionNodeName, IterativeActivityBuildAction buildAction, int chunkSize)
            => AddIterativeActivity<TIterationToken>(actionNodeName, buildAction, chunkSize) as IStructuredActivityBuilder;

        IStructuredActivityBuilder IExceptionHandlerBase<IStructuredActivityBuilder>.AddExceptionHandler<TException>(ExceptionHandlerDelegateAsync<TException> exceptionHandler)
            => AddExceptionHandler<TException>(exceptionHandler) as IStructuredActivityBuilder;

        IStructuredActivityBuilder ISendEventBase<IStructuredActivityBuilder>.AddSendEventAction<TEvent>(string actionNodeName, SendEventActionDelegateAsync<TEvent> actionAsync, BehaviorIdSelectorAsync targetSelectorAsync, SendEventActionBuildAction buildAction)
            => AddSendEventAction<TEvent>(actionNodeName, actionAsync, targetSelectorAsync, buildAction) as IStructuredActivityBuilder;
        #endregion

        public BehaviorClass BehaviorClass => Graph.Class;
        public int BehaviorVersion => Graph.Version;
        public string Name => Node.Name;
        public NodeType Type => Node.Type;

        IOverridenReactiveStructuredActivityBuilder IObjectFlowBase<IOverridenReactiveStructuredActivityBuilder>.AddFlow<TToken>(string targetNodeName,
            ObjectFlowBuildAction<TToken> buildAction)
            => AddFlow<TToken>(targetNodeName, buildAction) as IOverridenReactiveStructuredActivityBuilder;

        IOverridenReactiveStructuredActivityBuilder IControlFlowBase<IOverridenReactiveStructuredActivityBuilder>.AddControlFlow(string targetNodeName,
            ControlFlowBuildAction buildAction)
            => AddControlFlow(targetNodeName, buildAction) as IOverridenReactiveStructuredActivityBuilder;

        IOverridenReactiveStructuredActivityBuilder IActivityActionBase<IOverridenReactiveStructuredActivityBuilder>.AddAction(string actionNodeName, Func<IActionContext, Task> actionAsync,
            ActionBuildAction buildAction)
            => AddAction(actionNodeName, actionAsync, b => buildAction?.Invoke(b)) as IOverridenReactiveStructuredActivityBuilder;

        IOverridenReactiveStructuredActivityBuilder IInitialBase<IOverridenReactiveStructuredActivityBuilder>.AddInitial(InitialBuildAction buildAction)
            => AddInitial(buildAction) as IOverridenReactiveStructuredActivityBuilder;

        IOverridenReactiveStructuredActivityBuilder IFinalBase<IOverridenReactiveStructuredActivityBuilder>.AddFinal()
            => AddFinal() as IOverridenReactiveStructuredActivityBuilder;

        IOverridenReactiveStructuredActivityBuilder IInputBase<IOverridenReactiveStructuredActivityBuilder>.AddInput(InputBuildAction buildAction)
            => AddInput(buildAction) as IOverridenReactiveStructuredActivityBuilder;

        IOverridenReactiveStructuredActivityBuilder IOutputBase<IOverridenReactiveStructuredActivityBuilder>.AddOutput()
            => AddOutput() as IOverridenReactiveStructuredActivityBuilder;

        IOverridenReactiveStructuredActivityBuilder IExceptionHandlerBase<IOverridenReactiveStructuredActivityBuilder>.AddExceptionHandler<TException>(
            ExceptionHandlerDelegateAsync<TException> exceptionHandler)
            => AddExceptionHandler<TException>(exceptionHandler) as IOverridenReactiveStructuredActivityBuilder;

        IOverridenReactiveStructuredActivityBuilder ISendEventBase<IOverridenReactiveStructuredActivityBuilder>.AddSendEventAction<TEvent>(string actionNodeName,
            SendEventActionDelegateAsync<TEvent> actionAsync, BehaviorIdSelectorAsync targetSelectorAsync,
            SendEventActionBuildAction buildAction)
            => AddSendEventAction<TEvent>(actionNodeName, actionAsync, targetSelectorAsync, buildAction) as IOverridenReactiveStructuredActivityBuilder;

        IOverridenReactiveStructuredActivityBuilder IActivitySpecialsOverrides<IOverridenReactiveStructuredActivityBuilder>.UseInitial(OverridenInitialBuildAction buildAction)
            => UseInitial(buildAction) as IOverridenReactiveStructuredActivityBuilder;

        IOverridenStructuredActivityBuilder IActivitySpecialsOverrides<IOverridenStructuredActivityBuilder>.UseInput(OverridenInputBuildAction buildAction)
            => UseInput(buildAction) as IOverridenStructuredActivityBuilder;

        IOverridenStructuredActivityBuilder IActivitySpecialsOverrides<IOverridenStructuredActivityBuilder>.UseJoin(string joinNodeName, OverridenJoinBuildAction buildAction)
            => UseJoin(joinNodeName, buildAction) as IOverridenStructuredActivityBuilder;

        IOverridenStructuredActivityBuilder IActivitySpecialsOverrides<IOverridenStructuredActivityBuilder>.UseFork(string forkNodeName, OverridenForkBuildAction buildAction)
            => UseFork(forkNodeName, buildAction) as IOverridenStructuredActivityBuilder;

        IOverridenStructuredActivityBuilder IActivitySpecialsOverrides<IOverridenStructuredActivityBuilder>.UseMerge(string mergeNodeName, OverridenMergeBuildAction buildAction)
            => UseMerge(mergeNodeName, buildAction) as IOverridenStructuredActivityBuilder;

        IOverridenStructuredActivityBuilder IActivitySpecialsOverrides<IOverridenStructuredActivityBuilder>.UseControlDecision(string decisionNodeName,
            OverridenDecisionBuildAction buildAction)
            => UseControlDecision(decisionNodeName, buildAction) as IOverridenStructuredActivityBuilder;

        IOverridenStructuredActivityBuilder IActivitySpecialsOverrides<IOverridenStructuredActivityBuilder>.UseDecision<TToken>(string decisionNodeName,
            OverridenDecisionBuildAction<TToken> decisionBuildAction)
            => UseDecision(decisionNodeName, decisionBuildAction) as IOverridenStructuredActivityBuilder;

        IOverridenStructuredActivityBuilder IActivitySpecialsOverrides<IOverridenStructuredActivityBuilder>.UseDataStore(string dataStoreNodeName, OverridenDataStoreBuildAction buildAction)
            => UseDataStore(dataStoreNodeName, buildAction) as IOverridenStructuredActivityBuilder;

        IOverridenStructuredActivityBuilder IActivitySpecialsOverrides<IOverridenStructuredActivityBuilder>.UseInitial(OverridenInitialBuildAction buildAction)
            => UseInitial(buildAction) as IOverridenStructuredActivityBuilder;

        IOverridenReactiveStructuredActivityBuilder IActivitySpecialsOverrides<IOverridenReactiveStructuredActivityBuilder>.UseInput(OverridenInputBuildAction buildAction)
            => UseInput(buildAction) as IOverridenReactiveStructuredActivityBuilder;

        IOverridenReactiveStructuredActivityBuilder IAcceptEventOverrides<IOverridenReactiveStructuredActivityBuilder>.UseAcceptEventAction<TEvent>(string actionNodeName,
            OverridenAcceptEventActionBuildAction<TEvent> buildAction)
            => UseAcceptEventAction(actionNodeName, buildAction) as IOverridenReactiveStructuredActivityBuilder;

        IOverridenReactiveStructuredActivityBuilder IAcceptEventOverrides<IOverridenReactiveStructuredActivityBuilder>.UseTimeEventAction<TTimeEvent>(string actionNodeName,
            OverridenTimeEventNodeBuildAction buildAction)
            => UseTimeEventAction<TTimeEvent>(actionNodeName, buildAction) as IOverridenReactiveStructuredActivityBuilder;

        IOverridenReactiveStructuredActivityBuilder IActivitySpecialsOverrides<IOverridenReactiveStructuredActivityBuilder>.UseJoin(string joinNodeName, OverridenJoinBuildAction buildAction)
            => UseJoin(joinNodeName, buildAction) as IOverridenReactiveStructuredActivityBuilder;

        IOverridenReactiveStructuredActivityBuilder IActivitySpecialsOverrides<IOverridenReactiveStructuredActivityBuilder>.UseFork(string forkNodeName, OverridenForkBuildAction buildAction)
            => UseFork(forkNodeName, buildAction) as IOverridenReactiveStructuredActivityBuilder;

        IOverridenReactiveStructuredActivityBuilder IActivitySpecialsOverrides<IOverridenReactiveStructuredActivityBuilder>.UseMerge(string mergeNodeName, OverridenMergeBuildAction buildAction)
            => UseMerge(mergeNodeName, buildAction) as IOverridenReactiveStructuredActivityBuilder;

        IOverridenReactiveStructuredActivityBuilder IActivitySpecialsOverrides<IOverridenReactiveStructuredActivityBuilder>.UseControlDecision(string decisionNodeName,
            OverridenDecisionBuildAction buildAction)
            => UseControlDecision(decisionNodeName, buildAction) as IOverridenReactiveStructuredActivityBuilder;

        IOverridenReactiveStructuredActivityBuilder IActivitySpecialsOverrides<IOverridenReactiveStructuredActivityBuilder>.UseDecision<TToken>(string decisionNodeName,
            OverridenDecisionBuildAction<TToken> decisionBuildAction)
            => UseDecision(decisionNodeName, decisionBuildAction) as IOverridenReactiveStructuredActivityBuilder;

        IOverridenReactiveStructuredActivityBuilder IActivitySpecialsOverrides<IOverridenReactiveStructuredActivityBuilder>.UseDataStore(string dataStoreNodeName, OverridenDataStoreBuildAction buildAction)
            => UseDataStore(dataStoreNodeName, buildAction) as IOverridenReactiveStructuredActivityBuilder;

        IOverridenStructuredActivityBuilder IObjectFlowBase<IOverridenStructuredActivityBuilder>.AddFlow<TToken>(string targetNodeName, ObjectFlowBuildAction<TToken> buildAction)
            => AddFlow<TToken>(targetNodeName, buildAction) as IOverridenStructuredActivityBuilder;

        IOverridenStructuredActivityBuilder IControlFlowBase<IOverridenStructuredActivityBuilder>.AddControlFlow(string targetNodeName, ControlFlowBuildAction buildAction)
            => AddControlFlow(targetNodeName, buildAction) as IOverridenStructuredActivityBuilder;

        IOverridenStructuredActivityBuilder IActivityActionBase<IOverridenStructuredActivityBuilder>.AddAction(string actionNodeName, Func<IActionContext, Task> actionAsync,
            ActionBuildAction buildAction)
            => AddAction(actionNodeName, actionAsync, b => buildAction?.Invoke(b)) as IOverridenStructuredActivityBuilder;

        IOverridenStructuredActivityBuilder IInitialBase<IOverridenStructuredActivityBuilder>.AddInitial(InitialBuildAction buildAction)
            => AddInitial(buildAction) as IOverridenStructuredActivityBuilder;

        IOverridenStructuredActivityBuilder IFinalBase<IOverridenStructuredActivityBuilder>.AddFinal()
            => AddFinal() as IOverridenStructuredActivityBuilder;

        IOverridenStructuredActivityBuilder IInputBase<IOverridenStructuredActivityBuilder>.AddInput(InputBuildAction buildAction)
            => AddInput(buildAction) as IOverridenStructuredActivityBuilder;

        IOverridenStructuredActivityBuilder IOutputBase<IOverridenStructuredActivityBuilder>.AddOutput()
            => AddOutput() as IOverridenStructuredActivityBuilder;

        IOverridenStructuredActivityBuilder IExceptionHandlerBase<IOverridenStructuredActivityBuilder>.AddExceptionHandler<TException>(ExceptionHandlerDelegateAsync<TException> exceptionHandler)
            => AddExceptionHandler<TException>(exceptionHandler) as IOverridenStructuredActivityBuilder;

        IOverridenStructuredActivityBuilder ISendEventBase<IOverridenStructuredActivityBuilder>.AddSendEventAction<TEvent>(string actionNodeName,
            SendEventActionDelegateAsync<TEvent> actionAsync, BehaviorIdSelectorAsync targetSelectorAsync,
            SendEventActionBuildAction buildAction)
            => AddSendEventAction<TEvent>(actionNodeName, actionAsync, targetSelectorAsync, buildAction) as IOverridenStructuredActivityBuilder;

        IOverridenReactiveStructuredActivityBuilder ISendEventOverrides<IOverridenReactiveStructuredActivityBuilder>.UseSendEventAction<TEvent>(string actionNodeName,
            OverridenSendEventActionBuildAction buildAction)
            => UseSendEventAction<TEvent>(actionNodeName, buildAction) as IOverridenReactiveStructuredActivityBuilder;

        IOverridenStructuredActivityBuilder ISendEventOverrides<IOverridenStructuredActivityBuilder>.UseSendEventAction<TEvent>(string actionNodeName,
            OverridenSendEventActionBuildAction buildAction)
            => UseSendEventAction<TEvent>(actionNodeName, buildAction) as IOverridenStructuredActivityBuilder;

        IReactiveStructuredActivityBuilder IPublishEventBase<IReactiveStructuredActivityBuilder>.AddPublishEventAction<TEvent>(string actionNodeName,
            PublishEventActionDelegateAsync<TEvent> actionAsync, PublishEventActionBuildAction buildAction)
            => AddPublishEventAction(actionNodeName, actionAsync, buildAction) as IReactiveStructuredActivityBuilder;

        IOverridenReactiveStructuredActivityBuilder IPublishEventBase<IOverridenReactiveStructuredActivityBuilder>.AddPublishEventAction<TEvent>(string actionNodeName,
            PublishEventActionDelegateAsync<TEvent> actionAsync, PublishEventActionBuildAction buildAction)
            => AddPublishEventAction(actionNodeName, actionAsync, buildAction) as IOverridenReactiveStructuredActivityBuilder;

        IOverridenReactiveStructuredActivityBuilder IPublishEventOverrides<IOverridenReactiveStructuredActivityBuilder>.
            UsePublishEventAction<TEvent>(string actionNodeName,
                OverridenPublishEventActionBuildAction buildAction)
            => UsePublishEventAction<TEvent>(actionNodeName, buildAction) as IOverridenReactiveStructuredActivityBuilder;

        IOverridenStructuredActivityBuilder IActivityOverrides<IOverridenStructuredActivityBuilder>.UseStructuredActivity(string actionNodeName,
            OverridenStructuredActivityBuildAction buildAction)
            => UseStructuredActivity(actionNodeName, buildAction) as IOverridenStructuredActivityBuilder;

        IOverridenStructuredActivityBuilder IActivityOverrides<IOverridenStructuredActivityBuilder>.UseParallelActivity<TParallelizationToken>(string actionNodeName,
            OverridenParallelActivityBuildAction buildAction)
            => UseParallelActivity<TParallelizationToken>(actionNodeName, buildAction) as IOverridenStructuredActivityBuilder;
        
        IOverridenStructuredActivityBuilder IActivityOverrides<IOverridenStructuredActivityBuilder>.UseIterativeActivity<TIterationToken>(string actionNodeName,
            OverridenIterativeActivityBuildAction buildAction)
            => UseIterativeActivity<TIterationToken>(actionNodeName, buildAction) as IOverridenStructuredActivityBuilder;

        IOverridenReactiveStructuredActivityBuilder IActivityActionOverrides<IOverridenReactiveStructuredActivityBuilder>.UseAction(string actionNodeName,
            OverridenActionBuildAction buildAction)
            => UseAction(actionNodeName, buildAction) as IOverridenReactiveStructuredActivityBuilder;

        IOverridenReactiveStructuredActivityBuilder IReactiveActivityOverrides<IOverridenReactiveStructuredActivityBuilder>.UseStructuredActivity(string actionNodeName,
            OverridenReactiveStructuredActivityBuildAction buildAction)
            => UseStructuredActivity(actionNodeName, b => buildAction(b as IOverridenReactiveStructuredActivityBuilder)) as IOverridenReactiveStructuredActivityBuilder;

        IOverridenReactiveStructuredActivityBuilder IReactiveActivityOverrides<IOverridenReactiveStructuredActivityBuilder>.UseParallelActivity<TParallelizationToken>(string actionNodeName,
            OverridenParallelActivityBuildAction buildAction)
            => UseParallelActivity<TParallelizationToken>(actionNodeName, buildAction) as IOverridenReactiveStructuredActivityBuilder;

        IOverridenReactiveStructuredActivityBuilder IReactiveActivityOverrides<IOverridenReactiveStructuredActivityBuilder>.UseIterativeActivity<TIterationToken>(string actionNodeName,
            OverridenIterativeActivityBuildAction buildAction)
            => UseIterativeActivity<TIterationToken>(actionNodeName, buildAction) as IOverridenReactiveStructuredActivityBuilder;

        IOverridenStructuredActivityBuilder IActivityActionOverrides<IOverridenStructuredActivityBuilder>.UseAction(string actionNodeName, OverridenActionBuildAction buildAction)
            => UseAction(actionNodeName, buildAction) as IOverridenStructuredActivityBuilder;

        IReactiveStructuredActivityBuilder INodeOptions<IReactiveStructuredActivityBuilder>.SetOptions(NodeOptions nodeOptions)
            => SetOptions(nodeOptions) as IReactiveStructuredActivityBuilder;

        IOverridenReactiveStructuredActivityBuilder INodeOptions<IOverridenReactiveStructuredActivityBuilder>.SetOptions(NodeOptions nodeOptions)
            => SetOptions(nodeOptions) as IOverridenReactiveStructuredActivityBuilder;

        IStructuredActivityBuilder INodeOptions<IStructuredActivityBuilder>.SetOptions(NodeOptions nodeOptions)
            => SetOptions(nodeOptions) as IStructuredActivityBuilder;

        IOverridenStructuredActivityBuilder INodeOptions<IOverridenStructuredActivityBuilder>.SetOptions(NodeOptions nodeOptions)
            => SetOptions(nodeOptions) as IOverridenStructuredActivityBuilder;
    }
    
    internal class StructuredActivityBuilder<TAction>(Node parentNode, BaseActivityBuilder parentActivityBuilder) :
        StructuredActivityBuilder(parentNode, parentActivityBuilder),
        ITypedActionBuilder<TAction>
        where TAction : class, IActionNode
    {
        public ITypedActionBuilder<TAction> Configure(System.Action<TAction> action)
        {
            return this;
        }
        
        #region ITypedActionBuilder
        ITypedActionBuilder<TAction> IObjectFlowBase<ITypedActionBuilder<TAction>>.AddFlow<TToken>(string targetNodeName, ObjectFlowBuildAction<TToken> buildAction)
            => AddFlow(targetNodeName, buildAction) as ITypedActionBuilder<TAction>;

        ITypedActionBuilder<TAction> IControlFlowBase<ITypedActionBuilder<TAction>>.AddControlFlow(string targetNodeName, ControlFlowBuildAction buildAction)
            => AddControlFlow(targetNodeName, buildAction) as ITypedActionBuilder<TAction>;

        ITypedActionBuilder<TAction> IExceptionHandlerBase<ITypedActionBuilder<TAction>>.AddExceptionHandler<TException>(ExceptionHandlerDelegateAsync<TException> exceptionHandler)
            => AddExceptionHandler(exceptionHandler) as ITypedActionBuilder<TAction>;
        #endregion

        ITypedActionBuilder<TAction> INodeOptions<ITypedActionBuilder<TAction>>.SetOptions(NodeOptions nodeOptions)
            => SetOptions(nodeOptions) as ITypedActionBuilder<TAction>;

        ITypedActionBuilder<TAction> INodeOptions<ITypedActionBuilder<TAction>>.UpdateOptions(Func<NodeOptions, NodeOptions> nodeOptionsUpdater)
            => UpdateOptions(nodeOptionsUpdater) as ITypedActionBuilder<TAction>;
    }
}