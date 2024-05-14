using System;
using System.Linq;
using System.Threading.Tasks;
using System.Runtime.Serialization;
using Stateflows.Common;
using Stateflows.Common.Extensions;
using Stateflows.Activities.Context.Classes;
using Stateflows.Activities.Context.Interfaces;
using Stateflows.Activities.Registration.Builders;
using Stateflows.Activities.Registration.Interfaces;
using Stateflows.Activities.Events;

namespace Stateflows.Activities.Extensions
{
    internal static class BuilderExtensions
    {
        public static void AddActivityEvents(this IActivityBuilder builder, Type activityType)
        {
            if (typeof(Activity).GetMethod(nameof(Activity.OnInitializeAsync)).IsOverridenIn(activityType))
            {
                builder.AddOnInitialize(c =>
                {
                    var context = (c as IRootContext).Context;
                    return context.Executor.GetActivity(activityType, context)?.OnInitializeAsync();
                });
            }

            if (typeof(Activity).GetMethod(nameof(Activity.OnFinalizeAsync)).IsOverridenIn(activityType))
            {
                builder.AddOnFinalize(c =>
                {
                    var context = (c as IRootContext).Context;
                    return context.Executor.GetActivity(activityType, context)?.OnFinalizeAsync();
                });
            }

            var baseInterfaceType = typeof(IInitializedBy<>);
            foreach (var interfaceType in activityType.GetInterfaces())
            {
                if (interfaceType.GetGenericTypeDefinition() == baseInterfaceType)
                {
                    var methodInfo = interfaceType.GetMethods().First(m => m.Name == "OnInitializeAsync");
                    var requestType = interfaceType.GenericTypeArguments[0];
                    var requestName = requestType.GetEventName();
                    (builder as ActivityBuilder).AddInitializer(requestType, requestName, c =>
                    {
                        var activity = c.Context.Executor.GetActivity(activityType, c.Context);
                        return methodInfo.Invoke(activity, new object[] { c.Context.Event is ExecutionRequest executionRequest ? executionRequest.InitializationRequest : c.Context.Event }) as Task<bool>;
                    });
                }
            }
        }

        public static void AddStructuredActivityEvents<TStructuredActivity>(this StructuredActivityBuilder builder)
            where TStructuredActivity : StructuredActivityNode
        {
            if (typeof(StructuredActivityNode).GetMethod(nameof(StructuredActivityNode.OnInitializeAsync)).IsOverridenIn<TStructuredActivity>())
            {
                builder.AddOnInitialize(c => (c as BaseContext).NodeScope.GetStructuredActivity<TStructuredActivity>(c as IActionContext)?.OnInitializeAsync());
            }

            if (typeof(StructuredActivityNode).GetMethod(nameof(StructuredActivityNode.OnFinalizeAsync)).IsOverridenIn<TStructuredActivity>())
            {
                builder.AddOnFinalize(c => (c as BaseContext).NodeScope.GetStructuredActivity<TStructuredActivity>(c as IActionContext)?.OnFinalizeAsync());
            }
        }

        public static void AddObjectTransformationFlowEvents<TObjectTransformationFlow, TToken, TTransformedToken>(this IObjectFlowBuilder<TToken> builder)
            where TObjectTransformationFlow : TransformationFlow<TToken, TTransformedToken>
            // where TToken : Token, new()
            ////where TTransformedToken : Token, new()
        {
            if (typeof(BaseTransformationFlow<TToken, TTransformedToken>).GetProperty(nameof(BaseTransformationFlow<TToken, TTransformedToken>.Weight)).IsOverridenIn<TObjectTransformationFlow>())
            {
                var objectFlow = FormatterServices.GetUninitializedObject(typeof(TObjectTransformationFlow)) as TObjectTransformationFlow;

                builder.SetWeight(objectFlow.Weight);
            }

            if (typeof(BaseControlFlow).GetMethod(nameof(BaseControlFlow.GuardAsync)).IsOverridenIn<TObjectTransformationFlow>())
            {
                builder.AddGuard(c => (c as BaseContext).NodeScope.GetObjectTransformationFlow<TObjectTransformationFlow, TToken, TTransformedToken>(c)?.GuardAsync());
            }

            if (typeof(BaseTransformationFlow<TToken, TTransformedToken>).GetMethod(nameof(BaseTransformationFlow<TToken, TTransformedToken>.TransformAsync)).IsOverridenIn<TObjectTransformationFlow>())
            {
                builder.AddTransformation(c => (c as BaseContext).NodeScope.GetObjectTransformationFlow<TObjectTransformationFlow, TToken, TTransformedToken>(c)?.TransformAsync());
            }
        }

        public static void AddObjectFlowEvents<TFlow, TToken>(this IObjectFlowBuilder<TToken> builder)
            where TFlow : Flow<TToken>
            // where TToken : Token, new()
        {
            if (typeof(BaseFlow<TToken>).GetProperty(nameof(BaseFlow<TToken>.Weight)).IsOverridenIn<TFlow>())
            {
                var objectFlow = FormatterServices.GetUninitializedObject(typeof(TFlow)) as TFlow;

                builder.SetWeight(objectFlow.Weight);
            }

            if (typeof(BaseControlFlow).GetMethod(nameof(BaseControlFlow.GuardAsync)).IsOverridenIn<TFlow>())
            {
                builder.AddGuard(c => (c as BaseContext).NodeScope.GetObjectFlow<TFlow, TToken>(c)?.GuardAsync());
            }
        }

        public static void AddControlFlowEvents<TControlFlow>(this IControlFlowBuilder builder)
            where TControlFlow : ControlFlow
        {
            if (typeof(BaseControlFlow).GetMethod(nameof(BaseControlFlow.GuardAsync)).IsOverridenIn<TControlFlow>())
            {
                builder.AddGuard(c => (c as BaseContext).NodeScope.GetControlFlow<TControlFlow>(c)?.GuardAsync());
            }
        }
    }
}
