using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Stateflows.Activities.Models;
using Stateflows.Activities.Context.Interfaces;
using Stateflows.Activities.Registration.Interfaces;
using Stateflows.Activities.Registration.Interfaces.Base;

namespace Stateflows.Activities.Registration.Builders
{
    internal class StructuredActivityBuilder :
        BaseActivityBuilder,
        IActionBuilder,
        IActionBuilderWithOptions,
        IStructuredActivityBuilder,
        IStructuredActivityBuilderWithOptions
    {
        public NodeBuilder NodeBuilder { get; set; }

        public StructuredActivityBuilder(Node parentNode, BaseActivityBuilder parentActivityBuilder, IServiceCollection services)
            : base(parentNode, services)
        {
            NodeBuilder = new NodeBuilder(Node, parentActivityBuilder, Services);
        }

        public IActionBuilder AddObjectFlow<TToken>(string targetNodeName, FlowBuilderAction<TToken> buildAction = null)
            where TToken : Token, new()
        {
            NodeBuilder.AddObjectFlow<TToken>(targetNodeName, buildAction);

            return this;
        }

        public IActionBuilder AddControlFlow(string targetNodeName, ControlFlowBuilderAction buildAction = null)
        {
            NodeBuilder.AddControlFlow(targetNodeName, buildAction);

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

        IStructuredActivityBuilder IObjectFlow<IStructuredActivityBuilder>.AddObjectFlow<TToken>(string targetNodeName, FlowBuilderAction<TToken> buildAction)
            => AddObjectFlow<TToken>(targetNodeName, buildAction) as IStructuredActivityBuilder;

        IStructuredActivityBuilder IStructuredActivityEvents<IStructuredActivityBuilder>.AddOnFinalize(Func<IActivityActionContext, Task> actionAsync)
            => AddOnFinalize(actionAsync) as IStructuredActivityBuilder;

        IStructuredActivityBuilder IStructuredActivityEvents<IStructuredActivityBuilder>.AddOnInitialize(Func<IActivityActionContext, Task> actionAsync)
            => AddOnInitialize(actionAsync) as IStructuredActivityBuilder;

        IStructuredActivityBuilder IOutput<IStructuredActivityBuilder>.AddOutput()
            => AddOutput() as IStructuredActivityBuilder;

        IStructuredActivityBuilder IActivity<IStructuredActivityBuilder>.AddStructuredActivity(string actionNodeName, StructuredActivityBuilderAction builderAction)
            => AddStructuredActivity(actionNodeName, builderAction) as IStructuredActivityBuilder;

        IStructuredActivityBuilder IActivity<IStructuredActivityBuilder>.AddParallelActivity<TToken>(string actionNodeName, StructuredActivityBuilderAction builderAction)
            => AddParallelActivity<TToken>(actionNodeName, builderAction) as IStructuredActivityBuilder;

        IStructuredActivityBuilder IActivity<IStructuredActivityBuilder>.AddIterativeActivity<TToken>(string actionNodeName, StructuredActivityBuilderAction builderAction)
            => AddIterativeActivity<TToken>(actionNodeName, builderAction) as IStructuredActivityBuilder;

        IStructuredActivityBuilderWithOptions INodeOptions<IStructuredActivityBuilderWithOptions>.SetOptions(NodeOptions nodeOptions)
        {
            Node.Options = nodeOptions;

            return this;
        }

        IStructuredActivityBuilderWithOptions IObjectFlow<IStructuredActivityBuilderWithOptions>.AddObjectFlow<TToken>(string targetNodeName, FlowBuilderAction<TToken> buildAction)
            => AddObjectFlow<TToken>(targetNodeName, buildAction) as IStructuredActivityBuilderWithOptions;

        IStructuredActivityBuilderWithOptions IExceptionHandler<IStructuredActivityBuilderWithOptions>.AddExceptionHandler<TException>(ExceptionHandlerDelegateAsync<TException> exceptionHandler)
            => AddExceptionHandler(exceptionHandler) as IStructuredActivityBuilderWithOptions;

        IStructuredActivityBuilderWithOptions IFinal<IStructuredActivityBuilderWithOptions>.AddFinal()
            => AddFinal() as IStructuredActivityBuilderWithOptions;

        IStructuredActivityBuilderWithOptions IInitial<IStructuredActivityBuilderWithOptions>.AddInitial(InitialBuilderAction buildAction)
            => AddInitial(buildAction) as IStructuredActivityBuilderWithOptions;

        IStructuredActivityBuilderWithOptions IInput<IStructuredActivityBuilderWithOptions>.AddInput(InputBuilderAction buildAction)
            => AddInput(buildAction) as IStructuredActivityBuilderWithOptions;

        IStructuredActivityBuilderWithOptions IStructuredActivityEvents<IStructuredActivityBuilderWithOptions>.AddOnFinalize(Func<IActivityActionContext, Task> actionAsync)
            => AddOnFinalize(actionAsync) as IStructuredActivityBuilderWithOptions;

        IStructuredActivityBuilderWithOptions IStructuredActivityEvents<IStructuredActivityBuilderWithOptions>.AddOnInitialize(Func<IActivityActionContext, Task> actionAsync)
            => AddOnInitialize(actionAsync) as IStructuredActivityBuilderWithOptions;

        IStructuredActivityBuilderWithOptions IOutput<IStructuredActivityBuilderWithOptions>.AddOutput()
            => AddOutput() as IStructuredActivityBuilderWithOptions;

        IStructuredActivityBuilderWithOptions IActivity<IStructuredActivityBuilderWithOptions>.AddStructuredActivity(string actionNodeName, StructuredActivityBuilderAction builderAction)
            => AddStructuredActivity(actionNodeName, builderAction) as IStructuredActivityBuilderWithOptions;

        IStructuredActivityBuilderWithOptions IActivity<IStructuredActivityBuilderWithOptions>.AddParallelActivity<TToken>(string actionNodeName, StructuredActivityBuilderAction builderAction)
            => AddParallelActivity<TToken>(actionNodeName, builderAction) as IStructuredActivityBuilderWithOptions;

        IStructuredActivityBuilderWithOptions IActivity<IStructuredActivityBuilderWithOptions>.AddIterativeActivity<TToken>(string actionNodeName, StructuredActivityBuilderAction builderAction)
            => AddIterativeActivity<TToken>(actionNodeName, builderAction) as IStructuredActivityBuilderWithOptions;

        IStructuredActivityBuilder IExceptionHandler<IStructuredActivityBuilder>.AddExceptionHandler<TException>(ExceptionHandlerDelegateAsync<TException> exceptionHandler)
            => AddExceptionHandler<TException>(exceptionHandler) as IStructuredActivityBuilder;

        IActionBuilderWithOptions IObjectFlow<IActionBuilderWithOptions>.AddObjectFlow<TToken>(string targetNodeName, FlowBuilderAction<TToken> buildAction)
            => AddObjectFlow<TToken>(targetNodeName, buildAction) as IActionBuilderWithOptions;

        IActionBuilderWithOptions IControlFlow<IActionBuilderWithOptions>.AddControlFlow(string targetNodeName, ControlFlowBuilderAction buildAction)
            => AddControlFlow(targetNodeName, buildAction) as IActionBuilderWithOptions;

        IActionBuilderWithOptions IExceptionHandler<IActionBuilderWithOptions>.AddExceptionHandler<TException>(ExceptionHandlerDelegateAsync<TException> exceptionHandler)
            => AddExceptionHandler<TException>(exceptionHandler) as IActionBuilderWithOptions;

        IStructuredActivityBuilder ISendEvent<IStructuredActivityBuilder>.AddSendEventAction<TEvent>(string actionNodeName, SendEventActionDelegateAsync<TEvent> actionAsync, BehaviorIdSelectorAsync targetSelectorAsync, SendEventActionBuilderAction buildAction)
            => AddSendEventAction<TEvent>(actionNodeName, actionAsync, targetSelectorAsync, buildAction) as IStructuredActivityBuilder;

        IStructuredActivityBuilder IAcceptEvent<IStructuredActivityBuilder>.AddAcceptEventAction<TEvent>(string actionNodeName, AcceptEventActionDelegateAsync<TEvent> eventActionAsync, AcceptEventActionBuilderAction buildAction)
            => AddAcceptEventAction(actionNodeName, eventActionAsync, buildAction) as IStructuredActivityBuilder;

        IStructuredActivityBuilderWithOptions ISendEvent<IStructuredActivityBuilderWithOptions>.AddSendEventAction<TEvent>(string actionNodeName, SendEventActionDelegateAsync<TEvent> actionAsync, BehaviorIdSelectorAsync targetSelectorAsync, SendEventActionBuilderAction buildAction)
            => AddSendEventAction<TEvent>(actionNodeName, actionAsync, targetSelectorAsync, buildAction) as IStructuredActivityBuilderWithOptions;

        IStructuredActivityBuilderWithOptions IAcceptEvent<IStructuredActivityBuilderWithOptions>.AddAcceptEventAction<TEvent>(string actionNodeName, AcceptEventActionDelegateAsync<TEvent> eventActionAsync, AcceptEventActionBuilderAction buildAction)
            => AddAcceptEventAction(actionNodeName, eventActionAsync, buildAction) as IStructuredActivityBuilderWithOptions;
    }
}
