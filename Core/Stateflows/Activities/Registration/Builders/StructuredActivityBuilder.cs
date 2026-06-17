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
        IActionBuilderWithOptions,
        IReactiveStructuredActivityBuilder,
        IReactiveStructuredActivityBuilderWithOptions,
        IOverridenReactiveStructuredActivityBuilder,
        IOverridenReactiveStructuredActivityBuilderWithOptions,
        IStructuredActivityBuilder,
        IStructuredActivityBuilderWithOptions,
        IOverridenStructuredActivityBuilder,
        IOverridenStructuredActivityBuilderWithOptions,
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

        public IActionBuilderWithOptions SetOptions(NodeOptions nodeOptions)
        {
            NodeBuilder.SetOptions(nodeOptions);

            return this;
        }

        #region IActionBuilder
        IActionBuilderWithOptions IObjectFlowBase<IActionBuilderWithOptions>.AddFlow<TToken>(string targetNodeName, ObjectFlowBuildAction<TToken> buildAction)
            => AddFlow<TToken>(targetNodeName, buildAction) as IActionBuilderWithOptions;

        IActionBuilderWithOptions IControlFlowBase<IActionBuilderWithOptions>.AddControlFlow(string targetNodeName, ControlFlowBuildAction buildAction)
            => AddControlFlow(targetNodeName, buildAction) as IActionBuilderWithOptions;

        IActionBuilderWithOptions IExceptionHandlerBase<IActionBuilderWithOptions>.AddExceptionHandler<TException>(ExceptionHandlerDelegateAsync<TException> exceptionHandler)
            => AddExceptionHandler<TException>(exceptionHandler) as IActionBuilderWithOptions;
        #endregion

        #region IReactiveStructuredActivityBuilder
        IReactiveStructuredActivityBuilderWithOptions IActivityActionBase<IReactiveStructuredActivityBuilderWithOptions>.AddAction(string actionNodeName, Func<IActionContext, Task> actionAsync, ActionBuildAction buildAction)
            => AddAction(actionNodeName, actionAsync, b => buildAction(b)) as IReactiveStructuredActivityBuilderWithOptions;

        IReactiveStructuredActivityBuilderWithOptions IControlFlowBase<IReactiveStructuredActivityBuilderWithOptions>.AddControlFlow(string targetNodeName, ControlFlowBuildAction buildAction)
            => AddControlFlow(targetNodeName, buildAction) as IReactiveStructuredActivityBuilderWithOptions;

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

        IOverridenStructuredActivityBuilderWithOptions IStructuredActivityEvents<IOverridenStructuredActivityBuilderWithOptions>.AddOnFinalize(Func<IActivityNodeContext, Task> actionAsync)
            => AddOnFinalize(actionAsync) as IOverridenStructuredActivityBuilderWithOptions;

        IOverridenStructuredActivityBuilderWithOptions IStructuredActivityEvents<IOverridenStructuredActivityBuilderWithOptions>.AddOnInitialize(Func<IActivityNodeContext, Task> actionAsync)
            => AddOnInitialize(actionAsync) as IOverridenStructuredActivityBuilderWithOptions;

        IOverridenStructuredActivityBuilder IStructuredActivityEvents<IOverridenStructuredActivityBuilder>.AddOnFinalize(Func<IActivityNodeContext, Task> actionAsync)
            => AddOnFinalize(actionAsync) as IOverridenStructuredActivityBuilder;

        IOverridenStructuredActivityBuilder IStructuredActivityEvents<IOverridenStructuredActivityBuilder>.AddOnInitialize(Func<IActivityNodeContext, Task> actionAsync)
            => AddOnInitialize(actionAsync) as IOverridenStructuredActivityBuilder;

        IOverridenReactiveStructuredActivityBuilderWithOptions IStructuredActivityEvents<IOverridenReactiveStructuredActivityBuilderWithOptions>.AddOnFinalize(Func<IActivityNodeContext, Task> actionAsync)
            => AddOnFinalize(actionAsync) as IOverridenReactiveStructuredActivityBuilderWithOptions;

        IOverridenReactiveStructuredActivityBuilderWithOptions IStructuredActivityEvents<IOverridenReactiveStructuredActivityBuilderWithOptions>.AddOnInitialize(Func<IActivityNodeContext, Task> actionAsync)
            => AddOnInitialize(actionAsync) as IOverridenReactiveStructuredActivityBuilderWithOptions;

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

        IOverridenStructuredActivityBuilderWithOptions IActivityBase<IOverridenStructuredActivityBuilderWithOptions>.AddParallelActivity<TParallelizationToken>(string actionNodeName,
            ParallelActivityBuildAction buildAction, int chunkSize)
            => AddParallelActivity<TParallelizationToken>(actionNodeName, buildAction, chunkSize) as IOverridenStructuredActivityBuilderWithOptions;

        IOverridenStructuredActivityBuilderWithOptions IActivityBase<IOverridenStructuredActivityBuilderWithOptions>.AddIterativeActivity<TIterationToken>(string actionNodeName,
            IterativeActivityBuildAction buildAction, int chunkSize)
            => AddIterativeActivity<TIterationToken>(actionNodeName, buildAction, chunkSize) as IOverridenStructuredActivityBuilderWithOptions;

        IOverridenStructuredActivityBuilderWithOptions IActivityBase<IOverridenStructuredActivityBuilderWithOptions>.AddStructuredActivity(string actionNodeName,
            StructuredActivityBuildAction buildAction)
            => AddStructuredActivity(actionNodeName, b => buildAction?.Invoke(b as IStructuredActivityBuilder)) as IOverridenStructuredActivityBuilderWithOptions;

        IOverridenStructuredActivityBuilder IActivityBase<IOverridenStructuredActivityBuilder>.AddParallelActivity<TParallelizationToken>(string actionNodeName,
            ParallelActivityBuildAction buildAction, int chunkSize)
            => AddParallelActivity<TParallelizationToken>(actionNodeName, buildAction, chunkSize) as IOverridenStructuredActivityBuilder;

        IOverridenStructuredActivityBuilder IActivityBase<IOverridenStructuredActivityBuilder>.AddIterativeActivity<TIterationToken>(string actionNodeName,
            IterativeActivityBuildAction buildAction, int chunkSize)
            => AddIterativeActivity<TIterationToken>(actionNodeName, buildAction, chunkSize) as IOverridenStructuredActivityBuilder;

        IOverridenReactiveStructuredActivityBuilderWithOptions IReactiveActivityBase<IOverridenReactiveStructuredActivityBuilderWithOptions>.AddParallelActivity<TParallelizationToken>(string actionNodeName,
            ParallelActivityBuildAction buildAction, int chunkSize)
            => AddParallelActivity<TParallelizationToken>(actionNodeName, buildAction, chunkSize) as IOverridenReactiveStructuredActivityBuilderWithOptions;

        IOverridenReactiveStructuredActivityBuilderWithOptions IReactiveActivityBase<IOverridenReactiveStructuredActivityBuilderWithOptions>.AddIterativeActivity<TIterationToken>(string actionNodeName,
            IterativeActivityBuildAction buildAction, int chunkSize)
            => AddIterativeActivity<TIterationToken>(actionNodeName, buildAction, chunkSize) as IOverridenReactiveStructuredActivityBuilderWithOptions;

        IOverridenReactiveStructuredActivityBuilderWithOptions IReactiveActivityBase<IOverridenReactiveStructuredActivityBuilderWithOptions>.AddStructuredActivity(string actionNodeName,
            ReactiveStructuredActivityBuildAction buildAction)
            => AddStructuredActivity(actionNodeName, buildAction) as IOverridenReactiveStructuredActivityBuilderWithOptions;

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

        IReactiveStructuredActivityBuilderWithOptions INodeOptions<IReactiveStructuredActivityBuilderWithOptions>.SetOptions(NodeOptions nodeOptions)
        {
            Node.Options = nodeOptions;

            return this;
        }

        IReactiveStructuredActivityBuilderWithOptions IObjectFlowBase<IReactiveStructuredActivityBuilderWithOptions>.AddFlow<TToken>(string targetNodeName, ObjectFlowBuildAction<TToken> buildAction)
            => AddFlow<TToken>(targetNodeName, buildAction) as IReactiveStructuredActivityBuilderWithOptions;

        IReactiveStructuredActivityBuilderWithOptions IExceptionHandlerBase<IReactiveStructuredActivityBuilderWithOptions>.AddExceptionHandler<TException>(ExceptionHandlerDelegateAsync<TException> exceptionHandler)
            => AddExceptionHandler(exceptionHandler) as IReactiveStructuredActivityBuilderWithOptions;

        IReactiveStructuredActivityBuilderWithOptions IFinalBase<IReactiveStructuredActivityBuilderWithOptions>.AddFinal()
            => AddFinal() as IReactiveStructuredActivityBuilderWithOptions;

        IReactiveStructuredActivityBuilderWithOptions IInitialBase<IReactiveStructuredActivityBuilderWithOptions>.AddInitial(InitialBuildAction buildAction)
            => AddInitial(buildAction) as IReactiveStructuredActivityBuilderWithOptions;

        IReactiveStructuredActivityBuilderWithOptions IInputBase<IReactiveStructuredActivityBuilderWithOptions>.AddInput(InputBuildAction buildAction)
            => AddInput(buildAction) as IReactiveStructuredActivityBuilderWithOptions;

        IReactiveStructuredActivityBuilderWithOptions IStructuredActivityEvents<IReactiveStructuredActivityBuilderWithOptions>.AddOnFinalize(Func<IActivityNodeContext, Task> actionAsync)
            => AddOnFinalize(actionAsync) as IReactiveStructuredActivityBuilderWithOptions;

        IReactiveStructuredActivityBuilderWithOptions IStructuredActivityEvents<IReactiveStructuredActivityBuilderWithOptions>.AddOnInitialize(Func<IActivityNodeContext, Task> actionAsync)
            => AddOnInitialize(actionAsync) as IReactiveStructuredActivityBuilderWithOptions;

        IReactiveStructuredActivityBuilderWithOptions IOutputBase<IReactiveStructuredActivityBuilderWithOptions>.AddOutput()
            => AddOutput() as IReactiveStructuredActivityBuilderWithOptions;

        IReactiveStructuredActivityBuilderWithOptions IReactiveActivityBase<IReactiveStructuredActivityBuilderWithOptions>.AddStructuredActivity(string actionNodeName, ReactiveStructuredActivityBuildAction buildAction)
            => AddStructuredActivity(actionNodeName, buildAction) as IReactiveStructuredActivityBuilderWithOptions;

        IReactiveStructuredActivityBuilderWithOptions IReactiveActivityBase<IReactiveStructuredActivityBuilderWithOptions>.AddParallelActivity<TToken>(string actionNodeName, ParallelActivityBuildAction buildAction, int chunkSize)
            => AddParallelActivity<TToken>(actionNodeName, buildAction, chunkSize) as IReactiveStructuredActivityBuilderWithOptions;

        IReactiveStructuredActivityBuilderWithOptions IReactiveActivityBase<IReactiveStructuredActivityBuilderWithOptions>.AddIterativeActivity<TIterationToken>(string actionNodeName, IterativeActivityBuildAction buildAction, int chunkSize)
            => AddIterativeActivity<TIterationToken>(actionNodeName, buildAction, chunkSize) as IReactiveStructuredActivityBuilderWithOptions;

        IReactiveStructuredActivityBuilder IExceptionHandlerBase<IReactiveStructuredActivityBuilder>.AddExceptionHandler<TException>(ExceptionHandlerDelegateAsync<TException> exceptionHandler)
            => AddExceptionHandler<TException>(exceptionHandler) as IReactiveStructuredActivityBuilder;

        IReactiveStructuredActivityBuilder ISendEventBase<IReactiveStructuredActivityBuilder>.AddSendEventAction<TEvent>(string actionNodeName, SendEventActionDelegateAsync<TEvent> actionAsync, BehaviorIdSelectorAsync targetSelectorAsync, SendEventActionBuildAction buildAction)
            => AddSendEventAction<TEvent>(actionNodeName, actionAsync, targetSelectorAsync, buildAction) as IReactiveStructuredActivityBuilder;

        IReactiveStructuredActivityBuilder IAcceptEventBase<IReactiveStructuredActivityBuilder>.AddAcceptEventAction<TEvent>(string actionNodeName, AcceptEventActionDelegateAsync<TEvent> eventActionAsync, AcceptEventActionBuildAction<TEvent> buildAction)
            => AddAcceptEventAction(actionNodeName, eventActionAsync, buildAction) as IReactiveStructuredActivityBuilder;

        IOverridenReactiveStructuredActivityBuilderWithOptions IAcceptEventBase<IOverridenReactiveStructuredActivityBuilderWithOptions>.AddTimeEventAction<TTimeEvent>(string actionNodeName,
            TimeEventActionDelegateAsync eventActionAsync, TimeEventNodeBuildAction buildAction)
            => AddTimeEventAction<TTimeEvent>(actionNodeName, eventActionAsync, buildAction) as IOverridenReactiveStructuredActivityBuilderWithOptions;

        IOverridenReactiveStructuredActivityBuilderWithOptions IAcceptEventBase<IOverridenReactiveStructuredActivityBuilderWithOptions>.AddAcceptEventAction<TEvent>(string actionNodeName,
            AcceptEventActionDelegateAsync<TEvent> eventActionAsync, AcceptEventActionBuildAction<TEvent> buildAction)
            => AddAcceptEventAction<TEvent>(actionNodeName, eventActionAsync, buildAction) as IOverridenReactiveStructuredActivityBuilderWithOptions;

        IOverridenReactiveStructuredActivityBuilder IAcceptEventBase<IOverridenReactiveStructuredActivityBuilder>.AddTimeEventAction<TTimeEvent>(string actionNodeName,
            TimeEventActionDelegateAsync eventActionAsync, TimeEventNodeBuildAction buildAction)
            => AddTimeEventAction<TTimeEvent>(actionNodeName, eventActionAsync, buildAction) as IOverridenReactiveStructuredActivityBuilder;

        IOverridenReactiveStructuredActivityBuilder IAcceptEventBase<IOverridenReactiveStructuredActivityBuilder>.AddAcceptEventAction<TEvent>(string actionNodeName,
            AcceptEventActionDelegateAsync<TEvent> eventActionAsync, AcceptEventActionBuildAction<TEvent> buildAction)
            => AddAcceptEventAction<TEvent>(actionNodeName, eventActionAsync, buildAction) as IOverridenReactiveStructuredActivityBuilder;

        IReactiveStructuredActivityBuilder IAcceptEventBase<IReactiveStructuredActivityBuilder>.AddTimeEventAction<TTimeEvent>(string actionNodeName, TimeEventActionDelegateAsync eventActionAsync, TimeEventNodeBuildAction buildAction)
            => AddTimeEventAction<TTimeEvent>(actionNodeName, eventActionAsync, buildAction) as IReactiveStructuredActivityBuilder;

        IReactiveStructuredActivityBuilderWithOptions ISendEventBase<IReactiveStructuredActivityBuilderWithOptions>.AddSendEventAction<TEvent>(string actionNodeName, SendEventActionDelegateAsync<TEvent> actionAsync, BehaviorIdSelectorAsync targetSelectorAsync, SendEventActionBuildAction buildAction)
            => AddSendEventAction<TEvent>(actionNodeName, actionAsync, targetSelectorAsync, buildAction) as IReactiveStructuredActivityBuilderWithOptions;

        IReactiveStructuredActivityBuilderWithOptions IAcceptEventBase<IReactiveStructuredActivityBuilderWithOptions>.AddAcceptEventAction<TEvent>(string actionNodeName, AcceptEventActionDelegateAsync<TEvent> eventActionAsync, AcceptEventActionBuildAction<TEvent> buildAction)
            => AddAcceptEventAction(actionNodeName, eventActionAsync, buildAction) as IReactiveStructuredActivityBuilderWithOptions;

        IReactiveStructuredActivityBuilderWithOptions IAcceptEventBase<IReactiveStructuredActivityBuilderWithOptions>.AddTimeEventAction<TTimeEvent>(string actionNodeName, TimeEventActionDelegateAsync eventActionAsync, TimeEventNodeBuildAction buildAction)
            => AddTimeEventAction<TTimeEvent>(actionNodeName, eventActionAsync, buildAction) as IReactiveStructuredActivityBuilderWithOptions;
        #endregion

        #region IStructuredActivityBuilder
        IStructuredActivityBuilderWithOptions IActivityActionBase<IStructuredActivityBuilderWithOptions>.AddAction(string actionNodeName, Func<IActionContext, Task> actionAsync, ActionBuildAction buildAction)
            => AddAction(actionNodeName, actionAsync, b => buildAction(b)) as IStructuredActivityBuilderWithOptions;

        IStructuredActivityBuilderWithOptions IControlFlowBase<IStructuredActivityBuilderWithOptions>.AddControlFlow(string targetNodeName, ControlFlowBuildAction buildAction)
            => AddControlFlow(targetNodeName, buildAction) as IStructuredActivityBuilderWithOptions;

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

        IStructuredActivityBuilderWithOptions INodeOptions<IStructuredActivityBuilderWithOptions>.SetOptions(NodeOptions nodeOptions)
        {
            Node.Options = nodeOptions;

            return this;
        }

        IStructuredActivityBuilderWithOptions IObjectFlowBase<IStructuredActivityBuilderWithOptions>.AddFlow<TToken>(string targetNodeName, ObjectFlowBuildAction<TToken> buildAction)
            => AddFlow<TToken>(targetNodeName, buildAction) as IStructuredActivityBuilderWithOptions;

        IStructuredActivityBuilderWithOptions IExceptionHandlerBase<IStructuredActivityBuilderWithOptions>.AddExceptionHandler<TException>(ExceptionHandlerDelegateAsync<TException> exceptionHandler)
            => AddExceptionHandler(exceptionHandler) as IStructuredActivityBuilderWithOptions;

        IStructuredActivityBuilderWithOptions IFinalBase<IStructuredActivityBuilderWithOptions>.AddFinal()
            => AddFinal() as IStructuredActivityBuilderWithOptions;

        IStructuredActivityBuilderWithOptions IInitialBase<IStructuredActivityBuilderWithOptions>.AddInitial(InitialBuildAction buildAction)
            => AddInitial(buildAction) as IStructuredActivityBuilderWithOptions;

        IStructuredActivityBuilderWithOptions IInputBase<IStructuredActivityBuilderWithOptions>.AddInput(InputBuildAction buildAction)
            => AddInput(buildAction) as IStructuredActivityBuilderWithOptions;

        IStructuredActivityBuilderWithOptions IStructuredActivityEvents<IStructuredActivityBuilderWithOptions>.AddOnFinalize(Func<IActivityNodeContext, Task> actionAsync)
            => AddOnFinalize(actionAsync) as IStructuredActivityBuilderWithOptions;

        IStructuredActivityBuilderWithOptions IStructuredActivityEvents<IStructuredActivityBuilderWithOptions>.AddOnInitialize(Func<IActivityNodeContext, Task> actionAsync)
            => AddOnInitialize(actionAsync) as IStructuredActivityBuilderWithOptions;

        IStructuredActivityBuilderWithOptions IOutputBase<IStructuredActivityBuilderWithOptions>.AddOutput()
            => AddOutput() as IStructuredActivityBuilderWithOptions;

        IStructuredActivityBuilderWithOptions IActivityBase<IStructuredActivityBuilderWithOptions>.AddStructuredActivity(string actionNodeName, StructuredActivityBuildAction buildAction)
            => AddStructuredActivity(actionNodeName, b => buildAction?.Invoke(b as IStructuredActivityBuilder)) as IStructuredActivityBuilderWithOptions;

        IStructuredActivityBuilderWithOptions IActivityBase<IStructuredActivityBuilderWithOptions>.AddParallelActivity<TToken>(string actionNodeName, ParallelActivityBuildAction buildAction, int chunkSize)
            => AddParallelActivity<TToken>(actionNodeName, buildAction, chunkSize) as IStructuredActivityBuilderWithOptions;

        IStructuredActivityBuilderWithOptions IActivityBase<IStructuredActivityBuilderWithOptions>.AddIterativeActivity<TIterationToken>(string actionNodeName, IterativeActivityBuildAction buildAction, int chunkSize)
            => AddIterativeActivity<TIterationToken>(actionNodeName, buildAction, chunkSize) as IStructuredActivityBuilderWithOptions;

        IStructuredActivityBuilder IExceptionHandlerBase<IStructuredActivityBuilder>.AddExceptionHandler<TException>(ExceptionHandlerDelegateAsync<TException> exceptionHandler)
            => AddExceptionHandler<TException>(exceptionHandler) as IStructuredActivityBuilder;

        IStructuredActivityBuilder ISendEventBase<IStructuredActivityBuilder>.AddSendEventAction<TEvent>(string actionNodeName, SendEventActionDelegateAsync<TEvent> actionAsync, BehaviorIdSelectorAsync targetSelectorAsync, SendEventActionBuildAction buildAction)
            => AddSendEventAction<TEvent>(actionNodeName, actionAsync, targetSelectorAsync, buildAction) as IStructuredActivityBuilder;

        IStructuredActivityBuilderWithOptions ISendEventBase<IStructuredActivityBuilderWithOptions>.AddSendEventAction<TEvent>(string actionNodeName, SendEventActionDelegateAsync<TEvent> actionAsync, BehaviorIdSelectorAsync targetSelectorAsync, SendEventActionBuildAction buildAction)
            => AddSendEventAction<TEvent>(actionNodeName, actionAsync, targetSelectorAsync, buildAction) as IStructuredActivityBuilderWithOptions;
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

        IOverridenReactiveStructuredActivityBuilderWithOptions INodeOptions<IOverridenReactiveStructuredActivityBuilderWithOptions>.SetOptions(NodeOptions nodeOptions)
            => SetOptions(nodeOptions) as IOverridenReactiveStructuredActivityBuilderWithOptions;

        IOverridenReactiveStructuredActivityBuilder ISendEventBase<IOverridenReactiveStructuredActivityBuilder>.AddSendEventAction<TEvent>(string actionNodeName,
            SendEventActionDelegateAsync<TEvent> actionAsync, BehaviorIdSelectorAsync targetSelectorAsync,
            SendEventActionBuildAction buildAction)
            => AddSendEventAction<TEvent>(actionNodeName, actionAsync, targetSelectorAsync, buildAction) as IOverridenReactiveStructuredActivityBuilder;

        IOverridenReactiveStructuredActivityBuilder IActivitySpecialsOverrides<IOverridenReactiveStructuredActivityBuilder>.UseInitial(OverridenInitialBuildAction buildAction)
            => UseInitial(buildAction) as IOverridenReactiveStructuredActivityBuilder;

        IOverridenReactiveStructuredActivityBuilderWithOptions IActivitySpecialsOverrides<IOverridenReactiveStructuredActivityBuilderWithOptions>.UseInput(OverridenInputBuildAction buildAction)
            => UseInput(buildAction) as IOverridenReactiveStructuredActivityBuilderWithOptions;

        IOverridenReactiveStructuredActivityBuilderWithOptions IActivitySpecialsOverrides<IOverridenReactiveStructuredActivityBuilderWithOptions>.UseJoin(string joinNodeName,
            OverridenJoinBuildAction buildAction)
            => UseJoin(joinNodeName, buildAction) as IOverridenReactiveStructuredActivityBuilderWithOptions;

        IOverridenReactiveStructuredActivityBuilderWithOptions IActivitySpecialsOverrides<IOverridenReactiveStructuredActivityBuilderWithOptions>.UseFork(string forkNodeName,
            OverridenForkBuildAction buildAction)
            => UseFork(forkNodeName, buildAction) as IOverridenReactiveStructuredActivityBuilderWithOptions;

        IOverridenReactiveStructuredActivityBuilderWithOptions IActivitySpecialsOverrides<IOverridenReactiveStructuredActivityBuilderWithOptions>.UseMerge(string mergeNodeName,
            OverridenMergeBuildAction buildAction)
            => UseMerge(mergeNodeName, buildAction) as IOverridenReactiveStructuredActivityBuilderWithOptions;

        IOverridenReactiveStructuredActivityBuilderWithOptions IActivitySpecialsOverrides<IOverridenReactiveStructuredActivityBuilderWithOptions>.UseControlDecision(string decisionNodeName,
            OverridenDecisionBuildAction buildAction)
            => UseControlDecision(decisionNodeName, buildAction) as IOverridenReactiveStructuredActivityBuilderWithOptions;
        

        IOverridenReactiveStructuredActivityBuilderWithOptions IActivitySpecialsOverrides<IOverridenReactiveStructuredActivityBuilderWithOptions>.UseDecision<TToken>(string decisionNodeName,
            OverridenDecisionBuildAction<TToken> decisionBuildAction)
            => UseDecision(decisionNodeName, decisionBuildAction) as IOverridenReactiveStructuredActivityBuilderWithOptions;

        IOverridenReactiveStructuredActivityBuilderWithOptions IActivitySpecialsOverrides<IOverridenReactiveStructuredActivityBuilderWithOptions>.UseDataStore(string dataStoreNodeName,
            OverridenDataStoreBuildAction buildAction)
            => UseDataStore(dataStoreNodeName, buildAction) as IOverridenReactiveStructuredActivityBuilderWithOptions;

        IOverridenReactiveStructuredActivityBuilderWithOptions IActivitySpecialsOverrides<IOverridenReactiveStructuredActivityBuilderWithOptions>.UseInitial(OverridenInitialBuildAction buildAction)
            => UseInitial(buildAction) as IOverridenReactiveStructuredActivityBuilderWithOptions;

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

        IOverridenReactiveStructuredActivityBuilderWithOptions IAcceptEventOverrides<IOverridenReactiveStructuredActivityBuilderWithOptions>.UseTimeEventAction<TTimeEvent>(string actionNodeName,
            OverridenTimeEventNodeBuildAction buildAction)
            => UseTimeEventAction<TTimeEvent>(actionNodeName, buildAction) as IOverridenReactiveStructuredActivityBuilderWithOptions;

        IOverridenReactiveStructuredActivityBuilderWithOptions IAcceptEventOverrides<IOverridenReactiveStructuredActivityBuilderWithOptions>.UseAcceptEventAction<TEvent>(string actionNodeName,
            OverridenAcceptEventActionBuildAction<TEvent> buildAction)
            => UseAcceptEventAction(actionNodeName, buildAction) as IOverridenReactiveStructuredActivityBuilderWithOptions;

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

        IOverridenReactiveStructuredActivityBuilderWithOptions IObjectFlowBase<IOverridenReactiveStructuredActivityBuilderWithOptions>.AddFlow<TToken>(string targetNodeName,
            ObjectFlowBuildAction<TToken> buildAction)
            => AddFlow<TToken>(targetNodeName, buildAction) as IOverridenReactiveStructuredActivityBuilderWithOptions;

        IOverridenReactiveStructuredActivityBuilderWithOptions IControlFlowBase<IOverridenReactiveStructuredActivityBuilderWithOptions>.AddControlFlow(string targetNodeName,
            ControlFlowBuildAction buildAction)
            => AddControlFlow(targetNodeName, buildAction) as IOverridenReactiveStructuredActivityBuilderWithOptions;

        IOverridenReactiveStructuredActivityBuilderWithOptions IActivityActionBase<IOverridenReactiveStructuredActivityBuilderWithOptions>.AddAction(string actionNodeName, Func<IActionContext, Task> actionAsync,
            ActionBuildAction buildAction)
            => AddAction(actionNodeName, actionAsync, b => buildAction?.Invoke(b)) as IOverridenReactiveStructuredActivityBuilderWithOptions;

        IOverridenReactiveStructuredActivityBuilderWithOptions IInitialBase<IOverridenReactiveStructuredActivityBuilderWithOptions>.AddInitial(InitialBuildAction buildAction)
            => AddInitial(buildAction) as IOverridenReactiveStructuredActivityBuilderWithOptions;

        IOverridenReactiveStructuredActivityBuilderWithOptions IFinalBase<IOverridenReactiveStructuredActivityBuilderWithOptions>.AddFinal()
            => AddFinal() as IOverridenReactiveStructuredActivityBuilderWithOptions;

        IOverridenReactiveStructuredActivityBuilderWithOptions IInputBase<IOverridenReactiveStructuredActivityBuilderWithOptions>.AddInput(InputBuildAction buildAction)
            => AddInput(buildAction) as IOverridenReactiveStructuredActivityBuilderWithOptions;

        IOverridenReactiveStructuredActivityBuilderWithOptions IOutputBase<IOverridenReactiveStructuredActivityBuilderWithOptions>.AddOutput()
            => AddOutput() as IOverridenReactiveStructuredActivityBuilderWithOptions;

        IOverridenReactiveStructuredActivityBuilderWithOptions IExceptionHandlerBase<IOverridenReactiveStructuredActivityBuilderWithOptions>.AddExceptionHandler<TException>(
            ExceptionHandlerDelegateAsync<TException> exceptionHandler)
            => AddExceptionHandler<TException>(exceptionHandler) as IOverridenReactiveStructuredActivityBuilderWithOptions;

        IOverridenReactiveStructuredActivityBuilderWithOptions ISendEventBase<IOverridenReactiveStructuredActivityBuilderWithOptions>.AddSendEventAction<TEvent>(string actionNodeName,
            SendEventActionDelegateAsync<TEvent> actionAsync, BehaviorIdSelectorAsync targetSelectorAsync,
            SendEventActionBuildAction buildAction)
            => AddSendEventAction<TEvent>(actionNodeName, actionAsync, targetSelectorAsync, buildAction) as IOverridenReactiveStructuredActivityBuilderWithOptions;

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

        IOverridenStructuredActivityBuilderWithOptions INodeOptions<IOverridenStructuredActivityBuilderWithOptions>.SetOptions(NodeOptions nodeOptions)
             => SetOptions(nodeOptions) as IOverridenStructuredActivityBuilderWithOptions;

        IOverridenStructuredActivityBuilder ISendEventBase<IOverridenStructuredActivityBuilder>.AddSendEventAction<TEvent>(string actionNodeName,
            SendEventActionDelegateAsync<TEvent> actionAsync, BehaviorIdSelectorAsync targetSelectorAsync,
            SendEventActionBuildAction buildAction)
            => AddSendEventAction<TEvent>(actionNodeName, actionAsync, targetSelectorAsync, buildAction) as IOverridenStructuredActivityBuilder;

        IOverridenStructuredActivityBuilderWithOptions IObjectFlowBase<IOverridenStructuredActivityBuilderWithOptions>.AddFlow<TToken>(string targetNodeName,
            ObjectFlowBuildAction<TToken> buildAction)
            => AddFlow<TToken>(targetNodeName, buildAction) as IOverridenStructuredActivityBuilderWithOptions;

        IOverridenStructuredActivityBuilderWithOptions IControlFlowBase<IOverridenStructuredActivityBuilderWithOptions>.AddControlFlow(string targetNodeName,
            ControlFlowBuildAction buildAction)
            => AddControlFlow(targetNodeName, buildAction) as IOverridenStructuredActivityBuilderWithOptions;

        IOverridenStructuredActivityBuilderWithOptions IActivityActionBase<IOverridenStructuredActivityBuilderWithOptions>.AddAction(string actionNodeName, Func<IActionContext, Task> actionAsync,
            ActionBuildAction buildAction)
            => AddAction(actionNodeName, actionAsync, b => buildAction?.Invoke(b)) as IOverridenStructuredActivityBuilderWithOptions;

        IOverridenStructuredActivityBuilderWithOptions IInitialBase<IOverridenStructuredActivityBuilderWithOptions>.AddInitial(InitialBuildAction buildAction)
             => AddInitial(buildAction) as IOverridenStructuredActivityBuilderWithOptions;

        IOverridenStructuredActivityBuilderWithOptions IFinalBase<IOverridenStructuredActivityBuilderWithOptions>.AddFinal()
            => AddFinal() as IOverridenStructuredActivityBuilderWithOptions;

        IOverridenStructuredActivityBuilderWithOptions IInputBase<IOverridenStructuredActivityBuilderWithOptions>.AddInput(InputBuildAction buildAction)
            => AddInput(buildAction) as IOverridenStructuredActivityBuilderWithOptions;

        IOverridenStructuredActivityBuilderWithOptions IOutputBase<IOverridenStructuredActivityBuilderWithOptions>.AddOutput()
            => AddOutput() as IOverridenStructuredActivityBuilderWithOptions;

        IOverridenStructuredActivityBuilderWithOptions IExceptionHandlerBase<IOverridenStructuredActivityBuilderWithOptions>.AddExceptionHandler<TException>(
            ExceptionHandlerDelegateAsync<TException> exceptionHandler)
            => AddExceptionHandler<TException>(exceptionHandler) as IOverridenStructuredActivityBuilderWithOptions;

        IOverridenStructuredActivityBuilderWithOptions ISendEventBase<IOverridenStructuredActivityBuilderWithOptions>.AddSendEventAction<TEvent>(string actionNodeName,
            SendEventActionDelegateAsync<TEvent> actionAsync, BehaviorIdSelectorAsync targetSelectorAsync,
            SendEventActionBuildAction buildAction)
            => AddSendEventAction<TEvent>(actionNodeName, actionAsync, targetSelectorAsync, buildAction) as IOverridenStructuredActivityBuilderWithOptions;

        IOverridenReactiveStructuredActivityBuilder ISendEventOverrides<IOverridenReactiveStructuredActivityBuilder>.UseSendEventAction<TEvent>(string actionNodeName,
            OverridenSendEventActionBuildAction buildAction)
            => UseSendEventAction<TEvent>(actionNodeName, buildAction) as IOverridenReactiveStructuredActivityBuilder;

        IOverridenReactiveStructuredActivityBuilderWithOptions ISendEventOverrides<IOverridenReactiveStructuredActivityBuilderWithOptions>.UseSendEventAction<TEvent>(string actionNodeName,
            OverridenSendEventActionBuildAction buildAction)
            => UseSendEventAction<TEvent>(actionNodeName, buildAction) as IOverridenReactiveStructuredActivityBuilderWithOptions;

        IOverridenStructuredActivityBuilder ISendEventOverrides<IOverridenStructuredActivityBuilder>.UseSendEventAction<TEvent>(string actionNodeName,
            OverridenSendEventActionBuildAction buildAction)
            => UseSendEventAction<TEvent>(actionNodeName, buildAction) as IOverridenStructuredActivityBuilder;

        IReactiveStructuredActivityBuilder IPublishEventBase<IReactiveStructuredActivityBuilder>.AddPublishEventAction<TEvent>(string actionNodeName,
            PublishEventActionDelegateAsync<TEvent> actionAsync, PublishEventActionBuildAction buildAction)
            => AddPublishEventAction(actionNodeName, actionAsync, buildAction) as IReactiveStructuredActivityBuilder;

        IReactiveStructuredActivityBuilderWithOptions IPublishEventBase<IReactiveStructuredActivityBuilderWithOptions>.AddPublishEventAction<TEvent>(string actionNodeName,
            PublishEventActionDelegateAsync<TEvent> actionAsync, PublishEventActionBuildAction buildAction)
            => AddPublishEventAction(actionNodeName, actionAsync, buildAction) as IReactiveStructuredActivityBuilderWithOptions;

        IOverridenReactiveStructuredActivityBuilder IPublishEventBase<IOverridenReactiveStructuredActivityBuilder>.AddPublishEventAction<TEvent>(string actionNodeName,
            PublishEventActionDelegateAsync<TEvent> actionAsync, PublishEventActionBuildAction buildAction)
            => AddPublishEventAction(actionNodeName, actionAsync, buildAction) as IOverridenReactiveStructuredActivityBuilder;

        IOverridenReactiveStructuredActivityBuilderWithOptions IPublishEventBase<IOverridenReactiveStructuredActivityBuilderWithOptions>.AddPublishEventAction<TEvent>(string actionNodeName,
            PublishEventActionDelegateAsync<TEvent> actionAsync, PublishEventActionBuildAction buildAction)
            => AddPublishEventAction(actionNodeName, actionAsync, buildAction) as IOverridenReactiveStructuredActivityBuilderWithOptions;

        IOverridenReactiveStructuredActivityBuilder IPublishEventOverrides<IOverridenReactiveStructuredActivityBuilder>.
            UsePublishEventAction<TEvent>(string actionNodeName,
                OverridenPublishEventActionBuildAction buildAction)
            => UsePublishEventAction<TEvent>(actionNodeName, buildAction) as IOverridenReactiveStructuredActivityBuilder;

        IOverridenReactiveStructuredActivityBuilderWithOptions IPublishEventOverrides<IOverridenReactiveStructuredActivityBuilderWithOptions>.UsePublishEventAction<TEvent>(string actionNodeName,
            OverridenPublishEventActionBuildAction buildAction)
            => UsePublishEventAction<TEvent>(actionNodeName, buildAction) as IOverridenReactiveStructuredActivityBuilderWithOptions;

        IOverridenStructuredActivityBuilder IActivityOverrides<IOverridenStructuredActivityBuilder>.UseStructuredActivity(string actionNodeName,
            OverridenStructuredActivityBuildAction buildAction)
            => UseStructuredActivity(actionNodeName, buildAction) as IOverridenStructuredActivityBuilder;

        IOverridenStructuredActivityBuilderWithOptions IActivityOverrides<IOverridenStructuredActivityBuilderWithOptions>.UseParallelActivity<TParallelizationToken>(string actionNodeName,
            OverridenParallelActivityBuildAction buildAction)
            => UseParallelActivity<TParallelizationToken>(actionNodeName, buildAction) as IOverridenStructuredActivityBuilderWithOptions;

        IOverridenStructuredActivityBuilderWithOptions IActivityOverrides<IOverridenStructuredActivityBuilderWithOptions>.UseIterativeActivity<TIterationToken>(string actionNodeName,
            OverridenIterativeActivityBuildAction buildAction)
            => UseIterativeActivity<TIterationToken>(actionNodeName, buildAction) as IOverridenStructuredActivityBuilderWithOptions;

        IOverridenStructuredActivityBuilderWithOptions IActivityOverrides<IOverridenStructuredActivityBuilderWithOptions>.UseStructuredActivity(string actionNodeName,
            OverridenStructuredActivityBuildAction buildAction)
            => UseStructuredActivity(actionNodeName, buildAction) as IOverridenStructuredActivityBuilderWithOptions;

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

        IOverridenReactiveStructuredActivityBuilderWithOptions IReactiveActivityOverrides<IOverridenReactiveStructuredActivityBuilderWithOptions>.UseParallelActivity<TParallelizationToken>(string actionNodeName,
            OverridenParallelActivityBuildAction buildAction)
            => UseParallelActivity<TParallelizationToken>(actionNodeName, buildAction) as IOverridenReactiveStructuredActivityBuilderWithOptions;

        IOverridenReactiveStructuredActivityBuilderWithOptions IReactiveActivityOverrides<IOverridenReactiveStructuredActivityBuilderWithOptions>.UseIterativeActivity<TIterationToken>(string actionNodeName,
            OverridenIterativeActivityBuildAction buildAction)
            => UseIterativeActivity<TIterationToken>(actionNodeName, buildAction) as IOverridenReactiveStructuredActivityBuilderWithOptions;

        IOverridenReactiveStructuredActivityBuilderWithOptions IReactiveActivityOverrides<IOverridenReactiveStructuredActivityBuilderWithOptions>.UseStructuredActivity(string actionNodeName,
            OverridenReactiveStructuredActivityBuildAction buildAction)
            => UseStructuredActivity(actionNodeName, b => buildAction(b as IOverridenReactiveStructuredActivityBuilder)) as IOverridenReactiveStructuredActivityBuilderWithOptions;

        IOverridenReactiveStructuredActivityBuilder IReactiveActivityOverrides<IOverridenReactiveStructuredActivityBuilder>.UseParallelActivity<TParallelizationToken>(string actionNodeName,
            OverridenParallelActivityBuildAction buildAction)
            => UseParallelActivity<TParallelizationToken>(actionNodeName, buildAction) as IOverridenReactiveStructuredActivityBuilder;

        IOverridenReactiveStructuredActivityBuilder IReactiveActivityOverrides<IOverridenReactiveStructuredActivityBuilder>.UseIterativeActivity<TIterationToken>(string actionNodeName,
            OverridenIterativeActivityBuildAction buildAction)
            => UseIterativeActivity<TIterationToken>(actionNodeName, buildAction) as IOverridenReactiveStructuredActivityBuilder;

        IOverridenReactiveStructuredActivityBuilderWithOptions IActivityActionOverrides<IOverridenReactiveStructuredActivityBuilderWithOptions>.UseAction(string actionNodeName,
            OverridenActionBuildAction buildAction)
            => UseAction(actionNodeName, buildAction) as IOverridenReactiveStructuredActivityBuilderWithOptions;

        IOverridenStructuredActivityBuilderWithOptions IActivityActionOverrides<IOverridenStructuredActivityBuilderWithOptions>.UseAction(string actionNodeName,
            OverridenActionBuildAction buildAction)
            => UseAction(actionNodeName, buildAction) as IOverridenStructuredActivityBuilderWithOptions;

        IOverridenStructuredActivityBuilder IActivityActionOverrides<IOverridenStructuredActivityBuilder>.UseAction(string actionNodeName, OverridenActionBuildAction buildAction)
            => UseAction(actionNodeName, buildAction) as IOverridenStructuredActivityBuilder;
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
    }
}