using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Stateflows.Activities.Models;
using Stateflows.Activities.Context.Interfaces;
using Stateflows.Activities.Registration.Interfaces;
using Stateflows.Activities.Registration.Interfaces.Base;
using Stateflows.Common;

namespace Stateflows.Activities.Registration.Builders
{
    internal class StructuredActivityBuilder :
        BaseActivityBuilder,
        IActionBuilder,
        IActionBuilderWithOptions,
        IReactiveStructuredActivityBuilder,
        IReactiveStructuredActivityBuilderWithOptions,
        IStructuredActivityBuilder,
        IStructuredActivityBuilderWithOptions
    {
        public NodeBuilder NodeBuilder { get; set; }

        public StructuredActivityBuilder(Node parentNode, BaseActivityBuilder parentActivityBuilder, IServiceCollection services)
            : base(parentNode, services)
        {
            NodeBuilder = new NodeBuilder(Node, parentActivityBuilder, Services);
        }

        public IActionBuilder AddTokenFlow<TToken>(string targetNodeName, ObjectFlowBuilderAction<TToken> buildAction = null)
            where TToken : Token, new()
        {
            NodeBuilder.AddTokenFlow<TToken>(targetNodeName, buildAction);

            return this;
        }

        public IActionBuilder AddElseTokenFlow<TToken>(string targetNodeName, ElseObjectFlowBuilderAction<TToken> buildAction = null)
            where TToken : Token, new()
        {
            NodeBuilder.AddElseTokenFlow<TToken>(targetNodeName, buildAction);

            return this;
        }

        public IActionBuilder AddControlFlow(string targetNodeName, ControlFlowBuilderAction buildAction = null)
        {
            NodeBuilder.AddControlFlow(targetNodeName, buildAction);

            return this;
        }

        public IActionBuilder AddElseControlFlow(string targetNodeName, ElseControlFlowBuilderAction buildAction = null)
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
        IActionBuilderWithOptions IObjectFlow<IActionBuilderWithOptions>.AddTokenFlow<TToken>(string targetNodeName, ObjectFlowBuilderAction<TToken> buildAction)
            => AddTokenFlow<TToken>(targetNodeName, buildAction) as IActionBuilderWithOptions;

        IActionBuilderWithOptions IControlFlow<IActionBuilderWithOptions>.AddControlFlow(string targetNodeName, ControlFlowBuilderAction buildAction)
            => AddControlFlow(targetNodeName, buildAction) as IActionBuilderWithOptions;

        IActionBuilderWithOptions IExceptionHandler<IActionBuilderWithOptions>.AddExceptionHandler<TException>(ExceptionHandlerDelegateAsync<TException> exceptionHandler)
            => AddExceptionHandler<TException>(exceptionHandler) as IActionBuilderWithOptions;
        #endregion

        #region IReactiveStructuredActivityBuilder
        IReactiveStructuredActivityBuilderWithOptions IReactiveActivity<IReactiveStructuredActivityBuilderWithOptions>.AddAction(string actionNodeName, ActionDelegateAsync actionAsync, ActionBuilderAction buildAction)
            => AddAction(actionNodeName, actionAsync, b => buildAction(b)) as IReactiveStructuredActivityBuilderWithOptions;

        IReactiveStructuredActivityBuilderWithOptions IControlFlow<IReactiveStructuredActivityBuilderWithOptions>.AddControlFlow(string targetNodeName, ControlFlowBuilderAction buildAction)
            => AddControlFlow(targetNodeName, buildAction) as IReactiveStructuredActivityBuilderWithOptions;

        IReactiveStructuredActivityBuilder IReactiveActivity<IReactiveStructuredActivityBuilder>.AddAction(string actionNodeName, ActionDelegateAsync actionAsync, ActionBuilderAction buildAction)
            => AddAction(actionNodeName, actionAsync, b => buildAction?.Invoke(b)) as IReactiveStructuredActivityBuilder;

        IReactiveStructuredActivityBuilder IControlFlow<IReactiveStructuredActivityBuilder>.AddControlFlow(string targetNodeName, ControlFlowBuilderAction buildAction)
            => AddControlFlow(targetNodeName, buildAction) as IReactiveStructuredActivityBuilder;

        IReactiveStructuredActivityBuilder IFinal<IReactiveStructuredActivityBuilder>.AddFinal()
            => AddFinal() as IReactiveStructuredActivityBuilder;

        IReactiveStructuredActivityBuilder IInitial<IReactiveStructuredActivityBuilder>.AddInitial(InitialBuilderAction buildAction)
            => AddInitial(buildAction) as IReactiveStructuredActivityBuilder;

        IReactiveStructuredActivityBuilder IInput<IReactiveStructuredActivityBuilder>.AddInput(InputBuilderAction buildAction)
            => AddInput(buildAction) as IReactiveStructuredActivityBuilder;

        IReactiveStructuredActivityBuilder IObjectFlow<IReactiveStructuredActivityBuilder>.AddTokenFlow<TToken>(string targetNodeName, ObjectFlowBuilderAction<TToken> buildAction)
            => AddTokenFlow<TToken>(targetNodeName, buildAction) as IReactiveStructuredActivityBuilder;

        IReactiveStructuredActivityBuilder IStructuredActivityEvents<IReactiveStructuredActivityBuilder>.AddOnFinalize(Func<IActivityNodeContext, Task> actionAsync)
            => AddOnFinalize(actionAsync) as IReactiveStructuredActivityBuilder;

        IReactiveStructuredActivityBuilder IStructuredActivityEvents<IReactiveStructuredActivityBuilder>.AddOnInitialize(Func<IActivityNodeContext, Task> actionAsync)
            => AddOnInitialize(actionAsync) as IReactiveStructuredActivityBuilder;

        IReactiveStructuredActivityBuilder IOutput<IReactiveStructuredActivityBuilder>.AddOutput()
            => AddOutput() as IReactiveStructuredActivityBuilder;

        IReactiveStructuredActivityBuilder IReactiveActivity<IReactiveStructuredActivityBuilder>.AddStructuredActivity(string actionNodeName, ReactiveStructuredActivityBuilderAction builderAction)
            => AddStructuredActivity(actionNodeName, builderAction) as IReactiveStructuredActivityBuilder;

        IReactiveStructuredActivityBuilder IReactiveActivity<IReactiveStructuredActivityBuilder>.AddParallelActivity<TToken>(string actionNodeName, ParallelActivityBuilderAction builderAction)
            => AddParallelActivity<TToken>(actionNodeName, builderAction) as IReactiveStructuredActivityBuilder;

        IReactiveStructuredActivityBuilder IReactiveActivity<IReactiveStructuredActivityBuilder>.AddIterativeActivity<TToken>(string actionNodeName, IterativeActivityBuilderAction builderAction)
            => AddIterativeActivity<TToken>(actionNodeName, builderAction) as IReactiveStructuredActivityBuilder;

        IReactiveStructuredActivityBuilderWithOptions INodeOptions<IReactiveStructuredActivityBuilderWithOptions>.SetOptions(NodeOptions nodeOptions)
        {
            Node.Options = nodeOptions;

            return this;
        }

        IReactiveStructuredActivityBuilderWithOptions IObjectFlow<IReactiveStructuredActivityBuilderWithOptions>.AddTokenFlow<TToken>(string targetNodeName, ObjectFlowBuilderAction<TToken> buildAction)
            => AddTokenFlow<TToken>(targetNodeName, buildAction) as IReactiveStructuredActivityBuilderWithOptions;

        IReactiveStructuredActivityBuilderWithOptions IExceptionHandler<IReactiveStructuredActivityBuilderWithOptions>.AddExceptionHandler<TException>(ExceptionHandlerDelegateAsync<TException> exceptionHandler)
            => AddExceptionHandler(exceptionHandler) as IReactiveStructuredActivityBuilderWithOptions;

        IReactiveStructuredActivityBuilderWithOptions IFinal<IReactiveStructuredActivityBuilderWithOptions>.AddFinal()
            => AddFinal() as IReactiveStructuredActivityBuilderWithOptions;

        IReactiveStructuredActivityBuilderWithOptions IInitial<IReactiveStructuredActivityBuilderWithOptions>.AddInitial(InitialBuilderAction buildAction)
            => AddInitial(buildAction) as IReactiveStructuredActivityBuilderWithOptions;

        IReactiveStructuredActivityBuilderWithOptions IInput<IReactiveStructuredActivityBuilderWithOptions>.AddInput(InputBuilderAction buildAction)
            => AddInput(buildAction) as IReactiveStructuredActivityBuilderWithOptions;

        IReactiveStructuredActivityBuilderWithOptions IStructuredActivityEvents<IReactiveStructuredActivityBuilderWithOptions>.AddOnFinalize(Func<IActivityNodeContext, Task> actionAsync)
            => AddOnFinalize(actionAsync) as IReactiveStructuredActivityBuilderWithOptions;

        IReactiveStructuredActivityBuilderWithOptions IStructuredActivityEvents<IReactiveStructuredActivityBuilderWithOptions>.AddOnInitialize(Func<IActivityNodeContext, Task> actionAsync)
            => AddOnInitialize(actionAsync) as IReactiveStructuredActivityBuilderWithOptions;

        IReactiveStructuredActivityBuilderWithOptions IOutput<IReactiveStructuredActivityBuilderWithOptions>.AddOutput()
            => AddOutput() as IReactiveStructuredActivityBuilderWithOptions;

        IReactiveStructuredActivityBuilderWithOptions IReactiveActivity<IReactiveStructuredActivityBuilderWithOptions>.AddStructuredActivity(string actionNodeName, ReactiveStructuredActivityBuilderAction builderAction)
            => AddStructuredActivity(actionNodeName, builderAction) as IReactiveStructuredActivityBuilderWithOptions;

        IReactiveStructuredActivityBuilderWithOptions IReactiveActivity<IReactiveStructuredActivityBuilderWithOptions>.AddParallelActivity<TToken>(string actionNodeName, ParallelActivityBuilderAction builderAction)
            => AddParallelActivity<TToken>(actionNodeName, builderAction) as IReactiveStructuredActivityBuilderWithOptions;

        IReactiveStructuredActivityBuilderWithOptions IReactiveActivity<IReactiveStructuredActivityBuilderWithOptions>.AddIterativeActivity<TToken>(string actionNodeName, IterativeActivityBuilderAction builderAction)
            => AddIterativeActivity<TToken>(actionNodeName, builderAction) as IReactiveStructuredActivityBuilderWithOptions;

        IReactiveStructuredActivityBuilder IExceptionHandler<IReactiveStructuredActivityBuilder>.AddExceptionHandler<TException>(ExceptionHandlerDelegateAsync<TException> exceptionHandler)
            => AddExceptionHandler<TException>(exceptionHandler) as IReactiveStructuredActivityBuilder;

        IReactiveStructuredActivityBuilder ISendEvent<IReactiveStructuredActivityBuilder>.AddSendEventAction<TEvent>(string actionNodeName, SendEventActionDelegateAsync<TEvent> actionAsync, BehaviorIdSelectorAsync targetSelectorAsync, SendEventActionBuilderAction buildAction)
            => AddSendEventAction<TEvent>(actionNodeName, actionAsync, targetSelectorAsync, buildAction) as IReactiveStructuredActivityBuilder;

        IReactiveStructuredActivityBuilder IAcceptEvent<IReactiveStructuredActivityBuilder>.AddAcceptEventAction<TEvent>(string actionNodeName, AcceptEventActionDelegateAsync<TEvent> eventActionAsync, AcceptEventActionBuilderAction buildAction)
            => AddAcceptEventAction(actionNodeName, eventActionAsync, buildAction) as IReactiveStructuredActivityBuilder;

        IReactiveStructuredActivityBuilderWithOptions ISendEvent<IReactiveStructuredActivityBuilderWithOptions>.AddSendEventAction<TEvent>(string actionNodeName, SendEventActionDelegateAsync<TEvent> actionAsync, BehaviorIdSelectorAsync targetSelectorAsync, SendEventActionBuilderAction buildAction)
            => AddSendEventAction<TEvent>(actionNodeName, actionAsync, targetSelectorAsync, buildAction) as IReactiveStructuredActivityBuilderWithOptions;

        IReactiveStructuredActivityBuilderWithOptions IAcceptEvent<IReactiveStructuredActivityBuilderWithOptions>.AddAcceptEventAction<TEvent>(string actionNodeName, AcceptEventActionDelegateAsync<TEvent> eventActionAsync, AcceptEventActionBuilderAction buildAction)
            => AddAcceptEventAction(actionNodeName, eventActionAsync, buildAction) as IReactiveStructuredActivityBuilderWithOptions;
        #endregion

        #region IStructuredActivityBuilder
        IStructuredActivityBuilderWithOptions IActivity<IStructuredActivityBuilderWithOptions>.AddAction(string actionNodeName, ActionDelegateAsync actionAsync, ActionBuilderAction buildAction)
            => AddAction(actionNodeName, actionAsync, b => buildAction(b)) as IStructuredActivityBuilderWithOptions;

        IStructuredActivityBuilderWithOptions IControlFlow<IStructuredActivityBuilderWithOptions>.AddControlFlow(string targetNodeName, ControlFlowBuilderAction buildAction)
            => AddControlFlow(targetNodeName, buildAction) as IStructuredActivityBuilderWithOptions;

        IStructuredActivityBuilder IActivity<IStructuredActivityBuilder>.AddAction(string actionNodeName, ActionDelegateAsync actionAsync, ActionBuilderAction buildAction)
            => AddAction(actionNodeName, actionAsync, b => buildAction?.Invoke(b)) as IStructuredActivityBuilder;

        IStructuredActivityBuilder IControlFlow<IStructuredActivityBuilder>.AddControlFlow(string targetNodeName, ControlFlowBuilderAction buildAction)
            => AddControlFlow(targetNodeName, buildAction) as IStructuredActivityBuilder;

        IStructuredActivityBuilder IFinal<IStructuredActivityBuilder>.AddFinal()
            => AddFinal() as IStructuredActivityBuilder;

        IStructuredActivityBuilder IInitial<IStructuredActivityBuilder>.AddInitial(InitialBuilderAction buildAction)
            => AddInitial(buildAction) as IStructuredActivityBuilder;

        IStructuredActivityBuilder IInput<IStructuredActivityBuilder>.AddInput(InputBuilderAction buildAction)
            => AddInput(buildAction) as IStructuredActivityBuilder;

        IStructuredActivityBuilder IObjectFlow<IStructuredActivityBuilder>.AddTokenFlow<TToken>(string targetNodeName, ObjectFlowBuilderAction<TToken> buildAction)
            => AddTokenFlow<TToken>(targetNodeName, buildAction) as IStructuredActivityBuilder;

        IStructuredActivityBuilder IStructuredActivityEvents<IStructuredActivityBuilder>.AddOnFinalize(Func<IActivityNodeContext, Task> actionAsync)
            => AddOnFinalize(actionAsync) as IStructuredActivityBuilder;

        IStructuredActivityBuilder IStructuredActivityEvents<IStructuredActivityBuilder>.AddOnInitialize(Func<IActivityNodeContext, Task> actionAsync)
            => AddOnInitialize(actionAsync) as IStructuredActivityBuilder;

        IStructuredActivityBuilder IOutput<IStructuredActivityBuilder>.AddOutput()
            => AddOutput() as IStructuredActivityBuilder;

        IStructuredActivityBuilder IActivity<IStructuredActivityBuilder>.AddStructuredActivity(string actionNodeName, StructuredActivityBuilderAction builderAction)
            => AddStructuredActivity(actionNodeName, b => builderAction?.Invoke(b as IStructuredActivityBuilder)) as IStructuredActivityBuilder;

        IStructuredActivityBuilder IActivity<IStructuredActivityBuilder>.AddParallelActivity<TToken>(string actionNodeName, ParallelActivityBuilderAction builderAction)
            => AddParallelActivity<TToken>(actionNodeName, builderAction) as IStructuredActivityBuilder;

        IStructuredActivityBuilder IActivity<IStructuredActivityBuilder>.AddIterativeActivity<TToken>(string actionNodeName, IterativeActivityBuilderAction builderAction)
            => AddIterativeActivity<TToken>(actionNodeName, builderAction) as IStructuredActivityBuilder;

        IStructuredActivityBuilderWithOptions INodeOptions<IStructuredActivityBuilderWithOptions>.SetOptions(NodeOptions nodeOptions)
        {
            Node.Options = nodeOptions;

            return this;
        }

        IStructuredActivityBuilderWithOptions IObjectFlow<IStructuredActivityBuilderWithOptions>.AddTokenFlow<TToken>(string targetNodeName, ObjectFlowBuilderAction<TToken> buildAction)
            => AddTokenFlow<TToken>(targetNodeName, buildAction) as IStructuredActivityBuilderWithOptions;

        IStructuredActivityBuilderWithOptions IExceptionHandler<IStructuredActivityBuilderWithOptions>.AddExceptionHandler<TException>(ExceptionHandlerDelegateAsync<TException> exceptionHandler)
            => AddExceptionHandler(exceptionHandler) as IStructuredActivityBuilderWithOptions;

        IStructuredActivityBuilderWithOptions IFinal<IStructuredActivityBuilderWithOptions>.AddFinal()
            => AddFinal() as IStructuredActivityBuilderWithOptions;

        IStructuredActivityBuilderWithOptions IInitial<IStructuredActivityBuilderWithOptions>.AddInitial(InitialBuilderAction buildAction)
            => AddInitial(buildAction) as IStructuredActivityBuilderWithOptions;

        IStructuredActivityBuilderWithOptions IInput<IStructuredActivityBuilderWithOptions>.AddInput(InputBuilderAction buildAction)
            => AddInput(buildAction) as IStructuredActivityBuilderWithOptions;

        IStructuredActivityBuilderWithOptions IStructuredActivityEvents<IStructuredActivityBuilderWithOptions>.AddOnFinalize(Func<IActivityNodeContext, Task> actionAsync)
            => AddOnFinalize(actionAsync) as IStructuredActivityBuilderWithOptions;

        IStructuredActivityBuilderWithOptions IStructuredActivityEvents<IStructuredActivityBuilderWithOptions>.AddOnInitialize(Func<IActivityNodeContext, Task> actionAsync)
            => AddOnInitialize(actionAsync) as IStructuredActivityBuilderWithOptions;

        IStructuredActivityBuilderWithOptions IOutput<IStructuredActivityBuilderWithOptions>.AddOutput()
            => AddOutput() as IStructuredActivityBuilderWithOptions;

        IStructuredActivityBuilderWithOptions IActivity<IStructuredActivityBuilderWithOptions>.AddStructuredActivity(string actionNodeName, StructuredActivityBuilderAction builderAction)
            => AddStructuredActivity(actionNodeName, b => builderAction?.Invoke(b as IStructuredActivityBuilder)) as IStructuredActivityBuilderWithOptions;

        IStructuredActivityBuilderWithOptions IActivity<IStructuredActivityBuilderWithOptions>.AddParallelActivity<TToken>(string actionNodeName, ParallelActivityBuilderAction builderAction)
            => AddParallelActivity<TToken>(actionNodeName, builderAction) as IStructuredActivityBuilderWithOptions;

        IStructuredActivityBuilderWithOptions IActivity<IStructuredActivityBuilderWithOptions>.AddIterativeActivity<TToken>(string actionNodeName, IterativeActivityBuilderAction builderAction)
            => AddIterativeActivity<TToken>(actionNodeName, builderAction) as IStructuredActivityBuilderWithOptions;

        IStructuredActivityBuilder IExceptionHandler<IStructuredActivityBuilder>.AddExceptionHandler<TException>(ExceptionHandlerDelegateAsync<TException> exceptionHandler)
            => AddExceptionHandler<TException>(exceptionHandler) as IStructuredActivityBuilder;

        IStructuredActivityBuilder ISendEvent<IStructuredActivityBuilder>.AddSendEventAction<TEvent>(string actionNodeName, SendEventActionDelegateAsync<TEvent> actionAsync, BehaviorIdSelectorAsync targetSelectorAsync, SendEventActionBuilderAction buildAction)
            => AddSendEventAction<TEvent>(actionNodeName, actionAsync, targetSelectorAsync, buildAction) as IStructuredActivityBuilder;

        IStructuredActivityBuilderWithOptions ISendEvent<IStructuredActivityBuilderWithOptions>.AddSendEventAction<TEvent>(string actionNodeName, SendEventActionDelegateAsync<TEvent> actionAsync, BehaviorIdSelectorAsync targetSelectorAsync, SendEventActionBuilderAction buildAction)
            => AddSendEventAction<TEvent>(actionNodeName, actionAsync, targetSelectorAsync, buildAction) as IStructuredActivityBuilderWithOptions;
        #endregion
    }
}