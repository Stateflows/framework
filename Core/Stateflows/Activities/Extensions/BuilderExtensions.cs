using System.Runtime.Serialization;
using Stateflows.Common.Extensions;
using Stateflows.Activities.Context.Classes;
using Stateflows.Activities.Context.Interfaces;
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
                    var node = (c as BaseContext).NodeScope.GetStructuredActivity<TStructuredActivity>(c as IActionContext);

                    if (node != null)
                    {
                        ActivityNodeContextAccessor.Context.Value = c;
                        await (node as IStructuredActivityNodeInitialization)?.OnInitializeAsync();
                        ActivityNodeContextAccessor.Context.Value = null;
                    }
                });
            }

            if (typeof(IStructuredActivityNodeFinalization).IsAssignableFrom(typeof(TStructuredActivity)))
            {
                builder.AddOnFinalize(async c =>
                {
                    var node = (c as BaseContext).NodeScope.GetStructuredActivity<TStructuredActivity>(c as IActionContext);

                    if (node != null)
                    {
                        ActivityNodeContextAccessor.Context.Value = c;
                        await (node as IStructuredActivityNodeFinalization)?.OnFinalizeAsync();
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
                var objectFlow = FormatterServices.GetUninitializedObject(typeof(TFlow)) as TFlow;

                builder.SetWeight(objectFlow.Weight);
            }

            if (typeof(IFlowGuard<TToken>).IsAssignableFrom(typeof(TFlow)))
            {
                builder.AddGuard(async c =>
                {
                    var result = false;
                    var flow = (c as BaseContext).NodeScope.GetFlow<TFlow>(c);
                    if (flow != null)
                    {
                        ActivityFlowContextAccessor.Context.Value = c;
                        result = await (flow as IFlowGuard<TToken>)?.GuardAsync(c.Token);
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
                var objectFlow = FormatterServices.GetUninitializedObject(typeof(TTransformationFlow)) as TTransformationFlow;

                builder.SetWeight(objectFlow.Weight);
            }

            if (typeof(IFlowGuard<TToken>).IsAssignableFrom(typeof(TTransformationFlow)))
            {
                builder.AddGuard(async c =>
                {
                    var result = false;
                    var flow = (c as BaseContext).NodeScope.GetObjectTransformationFlow<TTransformationFlow, TToken, TTransformedToken>(c);
                    if (flow != null)
                    {
                        ActivityFlowContextAccessor.Context.Value = c;
                        result = await (flow as IFlowGuard<TToken>)?.GuardAsync(c.Token);
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
                    var flow = (c as BaseContext).NodeScope.GetObjectTransformationFlow<TTransformationFlow, TToken, TTransformedToken>(c);
                    if (flow != null)
                    {
                        ActivityFlowContextAccessor.Context.Value = c;
                        result = await (flow as IFlowTransformation<TToken, TTransformedToken>)?.TransformAsync(c.Token);
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
                var objectFlow = FormatterServices.GetUninitializedObject(typeof(TTransformationFlow)) as TTransformationFlow;

                builder.SetWeight(objectFlow.Weight);
            }

            if (typeof(IFlowTransformation<TToken, TTransformedToken>).IsAssignableFrom(typeof(TTransformationFlow)))
            {
                builder.AddTransformation(async c =>
                {
                    TTransformedToken result = default;
                    var flow = (c as BaseContext).NodeScope.GetObjectTransformationFlow<TTransformationFlow, TToken, TTransformedToken>(c);
                    if (flow != null)
                    {
                        ActivityFlowContextAccessor.Context.Value = c;
                        result = await (flow as IFlowTransformation<TToken, TTransformedToken>)?.TransformAsync(c.Token);
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
                var objectFlow = FormatterServices.GetUninitializedObject(typeof(TFlow)) as TFlow;

                builder.SetWeight(objectFlow.Weight);
            }

            if (typeof(IControlFlowGuard).IsAssignableFrom(typeof(TFlow)))
            {
                builder.AddGuard(async c =>
                {
                    var result = false;
                    var flow = (c as BaseContext).NodeScope.GetFlow<TFlow>(c);
                    if (flow != null)
                    {
                        ActivityFlowContextAccessor.Context.Value = c;
                        result = await (flow as IControlFlowGuard)?.GuardAsync();
                        ActivityFlowContextAccessor.Context.Value = null;
                    }

                    return result;
                });
            }
        }
    }
}
