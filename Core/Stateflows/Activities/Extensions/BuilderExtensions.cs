using Stateflows.Common.Classes;
using Stateflows.Activities.Models;
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
                    var node = await ((BaseContext)c).NodeScope.GetStructuredActivityAsync<TStructuredActivity>(c);

                    if (node != null)
                    {
                        ActivityNodeContextAccessor.Context.Value = c;
                        await ((IStructuredActivityNodeInitialization)node).OnInitializeAsync();
                        ActivityNodeContextAccessor.Context.Value = null;
                    }
                });
                builder.Node.OperationTypes[(ActivityOperationKind.Initialize, builder.Node.Initialize.Actions.Count - 1)] = typeof(TStructuredActivity);
            }

            if (typeof(IStructuredActivityNodeFinalization).IsAssignableFrom(typeof(TStructuredActivity)))
            {
                builder.AddOnFinalize(async c =>
                {
                    var node = await ((BaseContext)c).NodeScope.GetStructuredActivityAsync<TStructuredActivity>(c);

                    if (node != null)
                    {
                        ActivityNodeContextAccessor.Context.Value = c;
                        await ((IStructuredActivityNodeFinalization)node).OnFinalizeAsync();
                        ActivityNodeContextAccessor.Context.Value = null;
                    }
                });
                builder.Node.OperationTypes[(ActivityOperationKind.Finalize, builder.Node.Finalize.Actions.Count - 1)] = typeof(TStructuredActivity);
            }
        }

        public static void AddObjectFlowEvents<TFlow, TToken>(this IObjectFlowBuilder<TToken> builder)
            where TFlow : class, IFlow<TToken>
        {
            RegistrationMetadata.Flow(builder, typeof(TFlow));
            if (typeof(IEdge).GetProperty(nameof(IEdge.Weight)).IsImplementedIn<TFlow>())
            {
                builder.SetWeight(TFlow.Weight);
            }

            if (typeof(IFlowGuard<TToken>).IsAssignableFrom(typeof(TFlow)))
            {
                RegistrationMetadata.Guard(() => builder.AddGuard(async c =>
                {
                    var result = false;
                    var flow = await ((BaseContext)c).NodeScope.GetFlowAsync<TFlow>(c);
                    if (flow != null)
                    {
                        ActivityFlowContextAccessor.Context.Value = c;
                        result = await ((IFlowGuard<TToken>)flow).GuardAsync(c.Token);
                        ActivityFlowContextAccessor.Context.Value = null;
                    }

                    return result;
                }), ActivityGuardKind.Leaf, typeof(TFlow));
            }
        }

        public static void AddObjectTransformationFlowEvents<TTransformationFlow, TToken, TTransformedToken>(this IObjectFlowBuilder<TToken> builder)
            where TTransformationFlow : class, IFlowTransformation<TToken, TTransformedToken>
        {
            RegistrationMetadata.Flow(builder, typeof(TTransformationFlow));
            if (typeof(IEdge).GetProperty(nameof(IEdge.Weight)).IsImplementedIn<TTransformationFlow>())
            {
                builder.SetWeight(TTransformationFlow.Weight);
            }

            if (typeof(IFlowGuard<TToken>).IsAssignableFrom(typeof(TTransformationFlow)))
            {
                RegistrationMetadata.Guard(() => builder.AddGuard(async c =>
                {
                    var result = false;
                    var flow = await ((BaseContext)c).NodeScope.GetObjectTransformationFlowAsync<TTransformationFlow, TToken, TTransformedToken>(c);
                    if (flow != null)
                    {
                        ActivityFlowContextAccessor.Context.Value = c;
                        result = await ((IFlowGuard<TToken>)flow).GuardAsync(c.Token);
                        ActivityFlowContextAccessor.Context.Value = null;
                    }

                    return result;
                }), ActivityGuardKind.Leaf, typeof(TTransformationFlow));
            }

            if (typeof(IFlowTransformation<TToken, TTransformedToken>).IsAssignableFrom(typeof(TTransformationFlow)))
            {
                RegistrationMetadata.Transformation(() => builder.AddTransformation(async c =>
                {
                    TTransformedToken result = default;
                    var flow = await ((BaseContext)c).NodeScope.GetObjectTransformationFlowAsync<TTransformationFlow, TToken, TTransformedToken>(c);
                    if (flow != null)
                    {
                        ActivityFlowContextAccessor.Context.Value = c;
                        result = await flow.TransformAsync(c.Token);
                        ActivityFlowContextAccessor.Context.Value = null;
                    }

                    return result;
                }), typeof(TTransformationFlow));
            }
        }

        public static void AddElseObjectTransformationFlowEvents<TTransformationFlow, TToken, TTransformedToken>(this IObjectFlowBuilder<TToken> builder)
            where TTransformationFlow : class, IFlowTransformation<TToken, TTransformedToken>
        {
            RegistrationMetadata.Flow(builder, typeof(TTransformationFlow));
            if (typeof(IEdge).GetProperty(nameof(IEdge.Weight)).IsImplementedIn<TTransformationFlow>())
            {
                builder.SetWeight(TTransformationFlow.Weight);
            }

            if (typeof(IFlowTransformation<TToken, TTransformedToken>).IsAssignableFrom(typeof(TTransformationFlow)))
            {
                RegistrationMetadata.Transformation(() => builder.AddTransformation(async c =>
                {
                    TTransformedToken result = default;
                    var flow = await ((BaseContext)c).NodeScope.GetObjectTransformationFlowAsync<TTransformationFlow, TToken, TTransformedToken>(c);
                    if (flow != null)
                    {
                        ActivityFlowContextAccessor.Context.Value = c;
                        result = await flow.TransformAsync(c.Token);
                        ActivityFlowContextAccessor.Context.Value = null;
                    }

                    return result;
                }), typeof(TTransformationFlow));
            }
        }

        public static void AddControlFlowEvents<TControlFlow>(this IControlFlowBuilder builder)
            where TControlFlow : class, IControlFlow
        {
            RegistrationMetadata.Flow(builder, typeof(TControlFlow));
            if (typeof(IEdge).GetProperty(nameof(IEdge.Weight)).IsImplementedIn<TControlFlow>())
            {
                builder.SetWeight(TControlFlow.Weight);
            }

            if (typeof(IControlFlowGuard).IsAssignableFrom(typeof(TControlFlow)))
            {
                RegistrationMetadata.Guard(() => builder.AddGuard(async c =>
                {
                    var result = false;
                    var flow = await ((BaseContext)c).NodeScope.GetFlowAsync<TControlFlow>(c);
                    if (flow != null)
                    {
                        ActivityFlowContextAccessor.Context.Value = c;
                        result = await ((IControlFlowGuard)flow).GuardAsync();
                        ActivityFlowContextAccessor.Context.Value = null;
                    }

                    return result;
                }), ActivityGuardKind.Leaf, typeof(TControlFlow));
            }
        }
    }
}
