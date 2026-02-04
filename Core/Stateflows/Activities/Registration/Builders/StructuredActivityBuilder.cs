using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Stateflows.Actions;
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
        IStructuredActivityBuilder,
        IStructuredActivityBuilderWithOptions,
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

        IReactiveStructuredActivityBuilder IStructuredActivityEvents<IReactiveStructuredActivityBuilder>.AddOnFinalize(Func<IActivityNodeContext, Task> actionAsync)
            => AddOnFinalize(actionAsync) as IReactiveStructuredActivityBuilder;

        IReactiveStructuredActivityBuilder IStructuredActivityEvents<IReactiveStructuredActivityBuilder>.AddOnInitialize(Func<IActivityNodeContext, Task> actionAsync)
            => AddOnInitialize(actionAsync) as IReactiveStructuredActivityBuilder;

        IReactiveStructuredActivityBuilder IOutputBase<IReactiveStructuredActivityBuilder>.AddOutput()
            => AddOutput() as IReactiveStructuredActivityBuilder;

        IReactiveStructuredActivityBuilder IReactiveActivityBase<IReactiveStructuredActivityBuilder>.AddStructuredActivity(string actionNodeName, ReactiveStructuredActivityBuildAction buildAction)
            => AddStructuredActivity(actionNodeName, buildAction) as IReactiveStructuredActivityBuilder;

        IReactiveStructuredActivityBuilder IReactiveActivityBase<IReactiveStructuredActivityBuilder>.AddParallelActivity<TToken>(string actionNodeName, ParallelActivityBuildAction buildAction, int chunkSize)
            => AddParallelActivity<TToken>(actionNodeName, buildAction, chunkSize) as IReactiveStructuredActivityBuilder;

        IReactiveStructuredActivityBuilder IReactiveActivityBase<IReactiveStructuredActivityBuilder>.AddIterativeActivity<TToken>(string actionNodeName, IterativeActivityBuildAction buildAction, int chunkSize)
            => AddIterativeActivity<TToken>(actionNodeName, buildAction, chunkSize) as IReactiveStructuredActivityBuilder;

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

        IReactiveStructuredActivityBuilderWithOptions IReactiveActivityBase<IReactiveStructuredActivityBuilderWithOptions>.AddIterativeActivity<TToken>(string actionNodeName, IterativeActivityBuildAction buildAction, int chunkSize)
            => AddIterativeActivity<TToken>(actionNodeName, buildAction, chunkSize) as IReactiveStructuredActivityBuilderWithOptions;

        IReactiveStructuredActivityBuilder IExceptionHandlerBase<IReactiveStructuredActivityBuilder>.AddExceptionHandler<TException>(ExceptionHandlerDelegateAsync<TException> exceptionHandler)
            => AddExceptionHandler<TException>(exceptionHandler) as IReactiveStructuredActivityBuilder;

        IReactiveStructuredActivityBuilder ISendEventBase<IReactiveStructuredActivityBuilder>.AddSendEventAction<TEvent>(string actionNodeName, SendEventActionDelegateAsync<TEvent> actionAsync, BehaviorIdSelectorAsync targetSelectorAsync, SendEventActionBuildAction buildAction)
            => AddSendEventAction<TEvent>(actionNodeName, actionAsync, targetSelectorAsync, buildAction) as IReactiveStructuredActivityBuilder;

        IReactiveStructuredActivityBuilder IAcceptEvent<IReactiveStructuredActivityBuilder>.AddAcceptEventAction<TEvent>(string actionNodeName, AcceptEventActionDelegateAsync<TEvent> eventActionAsync, AcceptEventActionBuildAction buildAction)
            => AddAcceptEventAction(actionNodeName, eventActionAsync, buildAction) as IReactiveStructuredActivityBuilder;

        IReactiveStructuredActivityBuilder IAcceptEvent<IReactiveStructuredActivityBuilder>.AddTimeEventAction<TTimeEvent>(string actionNodeName, TimeEventActionDelegateAsync eventActionAsync, TimeEventNodeBuildAction buildAction)
            => AddTimeEventAction<TTimeEvent>(actionNodeName, eventActionAsync, buildAction) as IReactiveStructuredActivityBuilder;

        IReactiveStructuredActivityBuilderWithOptions ISendEventBase<IReactiveStructuredActivityBuilderWithOptions>.AddSendEventAction<TEvent>(string actionNodeName, SendEventActionDelegateAsync<TEvent> actionAsync, BehaviorIdSelectorAsync targetSelectorAsync, SendEventActionBuildAction buildAction)
            => AddSendEventAction<TEvent>(actionNodeName, actionAsync, targetSelectorAsync, buildAction) as IReactiveStructuredActivityBuilderWithOptions;

        IReactiveStructuredActivityBuilderWithOptions IAcceptEvent<IReactiveStructuredActivityBuilderWithOptions>.AddAcceptEventAction<TEvent>(string actionNodeName, AcceptEventActionDelegateAsync<TEvent> eventActionAsync, AcceptEventActionBuildAction buildAction)
            => AddAcceptEventAction(actionNodeName, eventActionAsync, buildAction) as IReactiveStructuredActivityBuilderWithOptions;

        IReactiveStructuredActivityBuilderWithOptions IAcceptEvent<IReactiveStructuredActivityBuilderWithOptions>.AddTimeEventAction<TTimeEvent>(string actionNodeName, TimeEventActionDelegateAsync eventActionAsync, TimeEventNodeBuildAction buildAction)
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

        IStructuredActivityBuilder IActivityBase<IStructuredActivityBuilder>.AddIterativeActivity<TToken>(string actionNodeName, IterativeActivityBuildAction buildAction, int chunkSize)
            => AddIterativeActivity<TToken>(actionNodeName, buildAction, chunkSize) as IStructuredActivityBuilder;

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

        IStructuredActivityBuilderWithOptions IActivityBase<IStructuredActivityBuilderWithOptions>.AddIterativeActivity<TToken>(string actionNodeName, IterativeActivityBuildAction buildAction, int chunkSize)
            => AddIterativeActivity<TToken>(actionNodeName, buildAction, chunkSize) as IStructuredActivityBuilderWithOptions;

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