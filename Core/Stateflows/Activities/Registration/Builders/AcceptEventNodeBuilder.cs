using System;
using Stateflows.Activities.Models;
using Stateflows.Activities.Registration.Interfaces;
using Stateflows.Activities.Registration.Interfaces.Base;
using Stateflows.Common.Interfaces;

namespace Stateflows.Activities.Registration.Builders
{
    internal class AcceptEventNodeBuilder<TEvent>(Node node, BaseActivityBuilder activityBuilder) :
        NodeBuilder(node, activityBuilder),
        IAcceptEventActionBuilder<TEvent>,
        IOverridenAcceptEventActionBuilder<TEvent>
    {
        IAcceptEventActionBuilder<TEvent> IObjectFlowBase<IAcceptEventActionBuilder<TEvent>>.AddFlow<TToken>(string targetNodeName, ObjectFlowBuildAction<TToken> buildAction)
            => AddFlow<TToken>(targetNodeName, buildAction) as IAcceptEventActionBuilder<TEvent>;

        IAcceptEventActionBuilder<TEvent> IControlFlowBase<IAcceptEventActionBuilder<TEvent>>.AddControlFlow(string targetNodeName, ControlFlowBuildAction buildAction)
            => AddControlFlow(targetNodeName, buildAction) as IAcceptEventActionBuilder<TEvent>;

        IAcceptEventActionBuilder<TEvent> IExceptionHandlerBase<IAcceptEventActionBuilder<TEvent>>.AddExceptionHandler<TException>(ExceptionHandlerDelegateAsync<TException> exceptionHandler)
            => AddExceptionHandler(exceptionHandler) as IAcceptEventActionBuilder<TEvent>;

        IOverridenAcceptEventActionBuilder<TEvent> IObjectFlowBase<IOverridenAcceptEventActionBuilder<TEvent>>.AddFlow<TToken>(string targetNodeName, ObjectFlowBuildAction<TToken> buildAction)
            => AddFlow<TToken>(targetNodeName, buildAction) as IOverridenAcceptEventActionBuilder<TEvent>;

        IOverridenAcceptEventActionBuilder<TEvent> IControlFlowBase<IOverridenAcceptEventActionBuilder<TEvent>>.AddControlFlow(string targetNodeName, ControlFlowBuildAction buildAction)
            => AddControlFlow(targetNodeName, buildAction) as IOverridenAcceptEventActionBuilder<TEvent>;

        IOverridenAcceptEventActionBuilder<TEvent> IExceptionHandlerBase<IOverridenAcceptEventActionBuilder<TEvent>>.AddExceptionHandler<TException>(ExceptionHandlerDelegateAsync<TException> exceptionHandler)
            => AddExceptionHandler(exceptionHandler) as IOverridenAcceptEventActionBuilder<TEvent>;

        IOverridenAcceptEventActionBuilder<TEvent> IOverridenObjectFlowBase<IOverridenAcceptEventActionBuilder<TEvent>>.UseFlow<TToken>(string targetNodeName, ObjectFlowBuildAction<TToken> buildAction)
            => UseFlow<TToken>(targetNodeName, buildAction) as IOverridenAcceptEventActionBuilder<TEvent>;

        IOverridenAcceptEventActionBuilder<TEvent> IOverridenControlFlowBase<IOverridenAcceptEventActionBuilder<TEvent>>.UseControlFlow(string targetNodeName, ControlFlowBuildAction buildAction)
            => UseControlFlow(targetNodeName, buildAction) as IOverridenAcceptEventActionBuilder<TEvent>;
    }
    
    internal class AcceptEventNodeBuilder<TEvent, TAcceptEventAction>(Node node, BaseActivityBuilder activityBuilder) :
        AcceptEventNodeBuilder<TEvent>(node, activityBuilder),
        IAcceptEventActionBuilder<TEvent, TAcceptEventAction>,
        IOverridenAcceptEventActionBuilder<TEvent, TAcceptEventAction>
        where TAcceptEventAction : class, IAcceptEventActionNode<TEvent>
    {
        public IAcceptEventActionBuilder<TEvent, TAcceptEventAction> Configure(Action<TEvent, TAcceptEventAction> action)
        {
            return this;
        }

        public IAcceptEventActionBuilder<TEvent, TAcceptEventAction> Configure(Action<TAcceptEventAction> action)
        {
            throw new NotImplementedException();
        }
        
        IAcceptEventActionBuilder<TEvent, TAcceptEventAction> IObjectFlowBase<IAcceptEventActionBuilder<TEvent, TAcceptEventAction>>.AddFlow<TToken>(string targetNodeName,
            ObjectFlowBuildAction<TToken> buildAction)
            => AddFlow<TToken>(targetNodeName, buildAction) as IAcceptEventActionBuilder<TEvent, TAcceptEventAction>;

        IAcceptEventActionBuilder<TEvent, TAcceptEventAction> IControlFlowBase<IAcceptEventActionBuilder<TEvent, TAcceptEventAction>>.AddControlFlow(string targetNodeName,
            ControlFlowBuildAction buildAction)
            => AddControlFlow(targetNodeName, buildAction) as IAcceptEventActionBuilder<TEvent, TAcceptEventAction>;

        IAcceptEventActionBuilder<TEvent, TAcceptEventAction> IExceptionHandlerBase<IAcceptEventActionBuilder<TEvent, TAcceptEventAction>>.AddExceptionHandler<TException>(
            ExceptionHandlerDelegateAsync<TException> exceptionHandler)
            => AddExceptionHandler<TException>(exceptionHandler) as IAcceptEventActionBuilder<TEvent, TAcceptEventAction>;

        IOverridenAcceptEventActionBuilder<TEvent, TAcceptEventAction> IObjectFlowBase<IOverridenAcceptEventActionBuilder<TEvent, TAcceptEventAction>>.AddFlow<TToken>(string targetNodeName, ObjectFlowBuildAction<TToken> buildAction)
            => AddFlow<TToken>(targetNodeName, buildAction) as IOverridenAcceptEventActionBuilder<TEvent, TAcceptEventAction>;

        IOverridenAcceptEventActionBuilder<TEvent, TAcceptEventAction> IControlFlowBase<IOverridenAcceptEventActionBuilder<TEvent, TAcceptEventAction>>.AddControlFlow(string targetNodeName, ControlFlowBuildAction buildAction)
            => AddControlFlow(targetNodeName, buildAction) as IOverridenAcceptEventActionBuilder<TEvent, TAcceptEventAction>;

        IOverridenAcceptEventActionBuilder<TEvent, TAcceptEventAction> IExceptionHandlerBase<IOverridenAcceptEventActionBuilder<TEvent, TAcceptEventAction>>.AddExceptionHandler<TException>(ExceptionHandlerDelegateAsync<TException> exceptionHandler)
            => AddExceptionHandler<TException>(exceptionHandler) as IOverridenAcceptEventActionBuilder<TEvent, TAcceptEventAction>;

        IOverridenAcceptEventActionBuilder<TEvent, TAcceptEventAction> IElementBuilderBase<TAcceptEventAction, IOverridenAcceptEventActionBuilder<TEvent, TAcceptEventAction>>.Configure(Action<TAcceptEventAction> action)
            => Configure(action) as IOverridenAcceptEventActionBuilder<TEvent, TAcceptEventAction>;

        IOverridenAcceptEventActionBuilder<TEvent, TAcceptEventAction> IOverridenObjectFlowBase<IOverridenAcceptEventActionBuilder<TEvent, TAcceptEventAction>>.UseFlow<TToken>(string targetNodeName, ObjectFlowBuildAction<TToken> buildAction)
            => UseFlow(targetNodeName, buildAction) as IOverridenAcceptEventActionBuilder<TEvent, TAcceptEventAction>;

        IOverridenAcceptEventActionBuilder<TEvent, TAcceptEventAction> IOverridenControlFlowBase<IOverridenAcceptEventActionBuilder<TEvent, TAcceptEventAction>>.UseControlFlow(string targetNodeName, ControlFlowBuildAction buildAction)
            => UseControlFlow(targetNodeName, buildAction) as IOverridenAcceptEventActionBuilder<TEvent, TAcceptEventAction>;
    }
}
