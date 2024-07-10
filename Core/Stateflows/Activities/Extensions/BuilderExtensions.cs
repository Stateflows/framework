using System;
using System.Linq;
using System.Threading.Tasks;
using System.Runtime.Serialization;
using Stateflows.Common;
using Stateflows.Common.Extensions;
using Stateflows.Activities.Events;
using Stateflows.Activities.Context.Classes;
using Stateflows.Activities.Context.Interfaces;
using Stateflows.Activities.Registration.Builders;
using Stateflows.Activities.Registration.Interfaces;

namespace Stateflows.Activities.Extensions
{
    internal static class BuilderExtensions
    {
        //public static void AddActivityEvents(this IActivityBuilder builder, Type activityType)
        //{
        //    if (typeof(Activity).GetMethod(nameof(Activity.OnInitializeAsync)).IsOverridenIn(activityType))
        //    {
        //        builder.AddDefaultInitializer(c =>
        //        {
        //            var context = (c as IRootContext).Context;
        //            return context.Executor.GetActivity(activityType, context)?.OnInitializeAsync();
        //        });
        //    }

        //    if (typeof(Activity).GetMethod(nameof(Activity.OnFinalizeAsync)).IsOverridenIn(activityType))
        //    {
        //        builder.AddFinalizer(c =>
        //        {
        //            var context = (c as IRootContext).Context;
        //            return context.Executor.GetActivity(activityType, context)?.OnFinalizeAsync();
        //        });
        //    }

        //    //var baseInterfaceType = typeof(IInitializedBy<>);
        //    //foreach (var interfaceType in activityType.GetInterfaces())
        //    //{
        //    //    if (interfaceType.GetGenericTypeDefinition() == baseInterfaceType)
        //    //    {
        //    //        var methodInfo = interfaceType.GetMethods().First(m => m.Name == "OnInitializeAsync");
        //    //        var requestType = interfaceType.GenericTypeArguments[0];
        //    //        var requestName = requestType.GetEventName();
        //    //        (builder as ActivityBuilder).AddInitializer(requestType, requestName, c =>
        //    //        {
        //    //            var activity = c.Context.Executor.GetActivity(activityType, c.Context);
        //    //            return methodInfo.Invoke(activity, new object[] { c.Context.Event is ExecutionRequest executionRequest ? executionRequest.InitializationRequest : c.Context.Event }) as Task<bool>;
        //    //        });
        //    //    }
        //    //}
        //}

        public static void AddStructuredActivityEvents<TStructuredActivity>(this StructuredActivityBuilder builder)
            where TStructuredActivity : StructuredActivityNode
        {
            if (typeof(StructuredActivityNode).GetMethod(nameof(StructuredActivityNode.OnInitializeAsync)).IsOverridenIn<TStructuredActivity>())
            {
                builder.AddOnInitialize(async c =>
                {
                    var node = (c as BaseContext).NodeScope.GetStructuredActivity<TStructuredActivity>(c as IActionContext);

                    if (node != null)
                    {
                        ActivityNodeContextAccessor.Context.Value = c;
                        await node?.OnInitializeAsync();
                        ActivityNodeContextAccessor.Context.Value = null;
                    }
                });
            }

            if (typeof(StructuredActivityNode).GetMethod(nameof(StructuredActivityNode.OnFinalizeAsync)).IsOverridenIn<TStructuredActivity>())
            {
                builder.AddOnFinalize(async c =>
                {
                    var node = (c as BaseContext).NodeScope.GetStructuredActivity<TStructuredActivity>(c as IActionContext);

                    if (node != null)
                    {
                        ActivityNodeContextAccessor.Context.Value = c;
                        await node?.OnFinalizeAsync();
                        ActivityNodeContextAccessor.Context.Value = null;
                    }
                });
            }
        }

        public static void AddTransformationFlowEvents<TTransformationFlow, TToken, TTransformedToken>(this IObjectFlowBuilder<TToken> builder)
            where TTransformationFlow : TransformationFlow<TToken, TTransformedToken>
        {
            if (typeof(BaseTransformationFlow<TToken, TTransformedToken>).GetProperty(nameof(BaseTransformationFlow<TToken, TTransformedToken>.Weight)).IsOverridenIn<TTransformationFlow>())
            {
                var objectFlow = FormatterServices.GetUninitializedObject(typeof(TTransformationFlow)) as TTransformationFlow;

                builder.SetWeight(objectFlow.Weight);
            }

            if (typeof(BaseControlFlow).GetMethod(nameof(BaseControlFlow.GuardAsync)).IsOverridenIn<TTransformationFlow>())
            {
                builder.AddGuard(async c =>
                {
                    var result = false;
                    var flow = (c as BaseContext).NodeScope.GetObjectTransformationFlow<TTransformationFlow, TToken, TTransformedToken>(c);
                    if (flow != null)
                    {
                        ActivityFlowContextAccessor.Context.Value = c;
                        result = await flow?.GuardAsync();
                        ActivityFlowContextAccessor.Context.Value = null;
                    }

                    return result;
                });
            }

            if (typeof(BaseTransformationFlow<TToken, TTransformedToken>).GetMethod(nameof(BaseTransformationFlow<TToken, TTransformedToken>.TransformAsync)).IsOverridenIn<TTransformationFlow>())
            {
                builder.AddTransformation(async c =>
                {
                    TTransformedToken result = default;
                    var flow = (c as BaseContext).NodeScope.GetObjectTransformationFlow<TTransformationFlow, TToken, TTransformedToken>(c);
                    if (flow != null)
                    {
                        ActivityFlowContextAccessor.Context.Value = c;
                        result = await flow?.TransformAsync();
                        ActivityFlowContextAccessor.Context.Value = null;
                    }

                    return result;
                });
            }
        }

        public static void AddElseTransformationFlowEvents<TElseTransformationFlow, TToken, TTransformedToken>(this IObjectFlowBuilder<TToken> builder)
            where TElseTransformationFlow : ElseTransformationFlow<TToken, TTransformedToken>
        {
            if (typeof(BaseTransformationFlow<TToken, TTransformedToken>).GetProperty(nameof(BaseTransformationFlow<TToken, TTransformedToken>.Weight)).IsOverridenIn<TElseTransformationFlow>())
            {
                var objectFlow = FormatterServices.GetUninitializedObject(typeof(TElseTransformationFlow)) as TElseTransformationFlow;

                builder.SetWeight(objectFlow.Weight);
            }

            if (typeof(BaseControlFlow).GetMethod(nameof(BaseControlFlow.GuardAsync)).IsOverridenIn<TElseTransformationFlow>())
            {
                builder.AddGuard(async c =>
                {
                    var result = false;
                    var flow = (c as BaseContext).NodeScope.GetElseObjectTransformationFlow<TElseTransformationFlow, TToken, TTransformedToken>(c);
                    if (flow != null)
                    {
                        ActivityFlowContextAccessor.Context.Value = c;
                        result = await flow?.GuardAsync();
                        ActivityFlowContextAccessor.Context.Value = null;
                    }

                    return result;
                });
            }

            if (typeof(BaseTransformationFlow<TToken, TTransformedToken>).GetMethod(nameof(BaseTransformationFlow<TToken, TTransformedToken>.TransformAsync)).IsOverridenIn<TElseTransformationFlow>())
            {
                builder.AddTransformation(async c =>
                {
                    TTransformedToken result = default;
                    var flow = (c as BaseContext).NodeScope.GetElseObjectTransformationFlow<TElseTransformationFlow, TToken, TTransformedToken>(c);
                    if (flow != null)
                    {
                        ActivityFlowContextAccessor.Context.Value = c;
                        result = await flow?.TransformAsync();
                        ActivityFlowContextAccessor.Context.Value = null;
                    }

                    return result;
                });
            }
        }

        public static void AddObjectFlowEvents<TFlow, TToken>(this IObjectFlowBuilder<TToken> builder)
            where TFlow : Flow<TToken>
        {
            if (typeof(BaseFlow<TToken>).GetProperty(nameof(BaseFlow<TToken>.Weight)).IsOverridenIn<TFlow>())
            {
                var objectFlow = FormatterServices.GetUninitializedObject(typeof(TFlow)) as TFlow;

                builder.SetWeight(objectFlow.Weight);
            }

            if (typeof(BaseControlFlow).GetMethod(nameof(BaseControlFlow.GuardAsync)).IsOverridenIn<TFlow>())
            {
                builder.AddGuard(async c =>
                {
                    var result = false;
                    var flow = (c as BaseContext).NodeScope.GetObjectFlow<TFlow, TToken>(c);
                    if (flow != null)
                    {
                        ActivityFlowContextAccessor.Context.Value = c;
                        result = await flow?.GuardAsync();
                        ActivityFlowContextAccessor.Context.Value = null;
                    }

                    return result;
                });
            }
        }

        public static void AddControlFlowEvents<TControlFlow>(this IControlFlowBuilder builder)
            where TControlFlow : ControlFlow
        {
            if (typeof(BaseControlFlow).GetMethod(nameof(BaseControlFlow.GuardAsync)).IsOverridenIn<TControlFlow>())
            {
                builder.AddGuard(async c =>
                {
                    var result = false;
                    var flow = (c as BaseContext).NodeScope.GetControlFlow<TControlFlow>(c);
                    if (flow != null)
                    {
                        ActivityFlowContextAccessor.Context.Value = c;
                        result = await flow?.GuardAsync();
                        ActivityFlowContextAccessor.Context.Value = null;
                    }

                    return result;
                });
            }
        }
    }
}
