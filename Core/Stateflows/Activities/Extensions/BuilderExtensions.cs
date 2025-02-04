using Stateflows.Common.Classes;
using Stateflows.Common.Extensions;
using Stateflows.Activities.Context.Classes;
using Stateflows.Activities.Registration.Builders;
using Stateflows.Activities.Registration.Interfaces;

namespace Stateflows.Activities.Extensions
{
    internal static class BuilderExtensions
    {
        public static void AddStructuredActivityEvents<TStructuredActivity>(this StructuredActivityBuilder builder)
            where TStructuredActivity : class, IStructuredActivityNode
        {
            if (typeof(IStructuredActivityNodeInitialization).IsAssignableFrom(typeof(TStructuredActivity)))
            {
                builder.AddOnInitialize(async c =>
                {
                    var node = ((BaseContext)c).NodeScope.GetStructuredActivity<TStructuredActivity>(c as Context.Interfaces.IActionContext);

                    if (node != null)
                    {
                        ActivityNodeContextAccessor.Context.Value = c;
                        await ((IStructuredActivityNodeInitialization)node).OnInitializeAsync();
                        ActivityNodeContextAccessor.Context.Value = null;
                    }
                });
            }

            if (typeof(IStructuredActivityNodeFinalization).IsAssignableFrom(typeof(TStructuredActivity)))
            {
                builder.AddOnFinalize(async c =>
                {
                    var node = ((BaseContext)c).NodeScope.GetStructuredActivity<TStructuredActivity>(c as Context.Interfaces.IActionContext);

                    if (node != null)
                    {
                        ActivityNodeContextAccessor.Context.Value = c;
                        await ((IStructuredActivityNodeFinalization)node).OnFinalizeAsync();
                        ActivityNodeContextAccessor.Context.Value = null;
                    }
                });
            }
        }

        public static void AddObjectFlowEvents<TFlow, TToken>(this IObjectFlowBuilder<TToken> builder)
            where TFlow : class, IFlow<TToken>
        {
            if (typeof(IEdge).GetProperty(nameof(IEdge.Weight)).IsImplementedIn<TFlow>())
            {
                var objectFlow = StateflowsActivator.CreateUninitializedInstance<TFlow>();

                builder.SetWeight(objectFlow.Weight);
            }

            if (typeof(IFlowGuard<TToken>).IsAssignableFrom(typeof(TFlow)))
            {
                builder.AddGuard(async c =>
                {
                    var result = false;
                    var flow = ((BaseContext)c).NodeScope.GetFlow<TFlow>(c);
                    if (flow != null)
                    {
                        ActivityFlowContextAccessor.Context.Value = c;
                        result = await ((IFlowGuard<TToken>)flow).GuardAsync(c.Token);
                        ActivityFlowContextAccessor.Context.Value = null;
                    }

                    return result;
                });
            }
        }

        public static void AddObjectTransformationFlowEvents<TTransformationFlow, TToken, TTransformedToken>(this IObjectFlowBuilder<TToken> builder)
            where TTransformationFlow : class, IFlowTransformation<TToken, TTransformedToken>
        {
            if (typeof(IEdge).GetProperty(nameof(IEdge.Weight)).IsImplementedIn<TTransformationFlow>())
            {
                var objectFlow = StateflowsActivator.CreateUninitializedInstance<TTransformationFlow>();

                builder.SetWeight(objectFlow.Weight);
            }

            if (typeof(IFlowGuard<TToken>).IsAssignableFrom(typeof(TTransformationFlow)))
            {
                builder.AddGuard(async c =>
                {
                    var result = false;
                    var flow = ((BaseContext)c).NodeScope.GetObjectTransformationFlow<TTransformationFlow, TToken, TTransformedToken>(c);
                    if (flow != null)
                    {
                        ActivityFlowContextAccessor.Context.Value = c;
                        result = await ((IFlowGuard<TToken>)flow).GuardAsync(c.Token);
                        ActivityFlowContextAccessor.Context.Value = null;
                    }

                    return result;
                });
            }

            if (typeof(IFlowTransformation<TToken, TTransformedToken>).IsAssignableFrom(typeof(TTransformationFlow)))
            {
                builder.AddTransformation(async c =>
                {
                    TTransformedToken result = default;
                    var flow = ((BaseContext)c).NodeScope.GetObjectTransformationFlow<TTransformationFlow, TToken, TTransformedToken>(c);
                    if (flow != null)
                    {
                        ActivityFlowContextAccessor.Context.Value = c;
                        result = await flow.TransformAsync(c.Token);
                        ActivityFlowContextAccessor.Context.Value = null;
                    }

                    return result;
                });
            }
        }

        public static void AddElseObjectTransformationFlowEvents<TTransformationFlow, TToken, TTransformedToken>(this IObjectFlowBuilder<TToken> builder)
            where TTransformationFlow : class, IFlowTransformation<TToken, TTransformedToken>
        {
            if (typeof(IEdge).GetProperty(nameof(IEdge.Weight)).IsImplementedIn<TTransformationFlow>())
            {
                var objectFlow = StateflowsActivator.CreateUninitializedInstance<TTransformationFlow>();

                builder.SetWeight(objectFlow.Weight);
            }

            if (typeof(IFlowTransformation<TToken, TTransformedToken>).IsAssignableFrom(typeof(TTransformationFlow)))
            {
                builder.AddTransformation(async c =>
                {
                    TTransformedToken result = default;
                    var flow = ((BaseContext)c).NodeScope.GetObjectTransformationFlow<TTransformationFlow, TToken, TTransformedToken>(c);
                    if (flow != null)
                    {
                        ActivityFlowContextAccessor.Context.Value = c;
                        result = await flow.TransformAsync(c.Token);
                        ActivityFlowContextAccessor.Context.Value = null;
                    }

                    return result;
                });
            }
        }

        public static void AddControlFlowEvents<TFlow>(this IControlFlowBuilder builder)
            where TFlow : class, IControlFlow
        {
            if (typeof(IEdge).GetProperty(nameof(IEdge.Weight)).IsImplementedIn<TFlow>())
            {
                var objectFlow = StateflowsActivator.CreateUninitializedInstance<TFlow>();

                builder.SetWeight(objectFlow.Weight);
            }

            if (typeof(IControlFlowGuard).IsAssignableFrom(typeof(TFlow)))
            {
                builder.AddGuard(async c =>
                {
                    var result = false;
                    var flow = ((BaseContext)c).NodeScope.GetFlow<TFlow>(c);
                    if (flow != null)
                    {
                        ActivityFlowContextAccessor.Context.Value = c;
                        result = await ((IControlFlowGuard)flow).GuardAsync();
                        ActivityFlowContextAccessor.Context.Value = null;
                    }

                    return result;
                });
            }
        }
    }
}
