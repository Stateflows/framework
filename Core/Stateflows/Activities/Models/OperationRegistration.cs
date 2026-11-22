using System;
using System.Collections.Generic;
using Stateflows.Activities.Registration.Builders;
using Stateflows.Activities.Registration.Interfaces.Internal;
using Stateflows.Common;
using Stateflows.Activities.Context.Classes;
using System.Threading.Tasks;

namespace Stateflows.Activities.Models
{
    internal sealed class ActivityOperationRegistration
    {
        internal string Identifier { get; init; }
        internal string OwnerId { get; init; }
        internal int Order { get; init; }
        internal ActivityOperationKind Operation { get; init; }
        internal ActivityGuardKind GuardKind { get; init; }
        internal string? ParentGuardId { get; init; }
        internal bool ObservedInBody { get; init; }
    }

    internal sealed class FlowOperationRegistration
    {
        internal bool IsTransformation { get; init; }
        internal Type InputType { get; init; }
        internal Type OutputType { get; init; }
        internal Type[] ImplementationTypes { get; set; } = [];
        internal ActivityGuardKind GuardKind { get; set; } = ActivityGuardKind.Leaf;
        internal ActivityOperationRegistration Registration { get; set; }
        internal ActivityOperationRegistration[] GuardChildren { get; set; } = [];
    }

    internal interface IFlowMetadataBuilder
    {
        Edge Edge { get; }
    }

    internal static class RegistrationMetadata
    {
        internal static Task<bool> ObserveGuardLeafAsync(object context, int index, Func<Task<bool>> next)
        {
            var flowContext = (FlowContext)context;
            return flowContext.Context.Executor.Inspector.OnPredicateAsync(
                new PredicateObservationContext(flowContext, flowContext.Operation.GuardChildren[index]), next);
        }

        internal static TResult Guard<TResult>(Func<TResult> register, ActivityGuardKind kind, params Type[] implementationTypes)
        {
            var result = register();
            ((IFlowMetadataBuilder)result).Edge.Operations[^1].ImplementationTypes = implementationTypes;
            ((IFlowMetadataBuilder)result).Edge.Operations[^1].GuardKind = kind;
            return result;
        }

        internal static TResult Transformation<TResult>(Func<TResult> register, Type implementationType)
        {
            var result = register();
            ((IFlowMetadataBuilder)result).Edge.Operations[^1].ImplementationTypes = [implementationType];
            return result;
        }

        internal static void Flow(object builder, Type implementationType)
            => ((IFlowMetadataBuilder)builder).Edge.ImplementationType = implementationType;

        internal static TResult Initializer<TResult>(Func<TResult> register, Type implementationType, Type eventType = null)
        {
            var result = register();
            var graph = ((IGraphBuilder)result).Graph;
            if (eventType == null)
                graph.DefaultInitializerImplementation = implementationType;
            else
            {
                var name = eventType.GetEventName();
                graph.InitializerImplementations[(name, graph.Initializers[name].Actions.Count - 1)] = implementationType;
            }
            return result;
        }

        internal static TResult Finalizer<TResult>(Func<TResult> register, Type implementationType)
        {
            var result = register();
            var graph = ((IGraphBuilder)result).Graph;
            graph.OperationTypes[(ActivityOperationKind.Finalize, graph.Finalize.Actions.Count - 1)] = implementationType;
            return result;
        }

        internal static TResult ExceptionHandler<TResult>(Func<TResult> register, Type exceptionType, Type implementationType)
        {
            var result = register();
            var builder = result is StructuredActivityBuilder structured ? structured.NodeBuilder : (Stateflows.Activities.Registration.NodeBuilder)(object)result;
            var node = builder.Graph.AllNamedNodes[$"{builder.Node.Name}.{exceptionType.FullName}.ExceptionHandler"];
            node.ImplementationType = implementationType;
            node.OperationTypes[(ActivityOperationKind.Execute, node.Action.Actions.Count - 1)] = implementationType;
            return result;
        }
    }
}
