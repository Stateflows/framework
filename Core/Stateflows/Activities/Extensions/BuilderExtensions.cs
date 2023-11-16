﻿using System;
using System.Linq;
using System.Threading.Tasks;
using System.Runtime.Serialization;
using Stateflows.Common;
using Stateflows.Common.Extensions;
using Stateflows.Activities.Context.Classes;
using Stateflows.Activities.Context.Interfaces;
using Stateflows.Activities.Registration.Builders;
using Stateflows.Activities.Registration.Interfaces;

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
                    var methodInfo = interfaceType.GetMethods().First(m => m.Name == "InitializeAsync");
                    var requestType = interfaceType.GenericTypeArguments[0];
                    var requestName = Stateflows.Common.EventInfo.GetName(requestType);
                    (builder as ActivityBuilder).AddInitializer(requestName, c =>
                    {
                        var activity = c.Context.Executor.GetActivity(activityType, c.Context);
                        return methodInfo.Invoke(activity, new object[] { c.Context.Event }) as Task;
                    });
                }
            }
        }

        public static void AddStructuredActivityEvents<TStructuredActivity>(this IStructuredActivityBuilder builder)
            where TStructuredActivity : StructuredActivity
        {
            if (typeof(StructuredActivity).GetMethod(nameof(StructuredActivity.OnInitializeAsync)).IsOverridenIn<TStructuredActivity>())
            {
                builder.AddOnInitialize(c => (c as BaseContext).NodeScope.GetStructuredActivity<TStructuredActivity>(c as IActionContext)?.OnInitializeAsync());
            }

            if (typeof(StructuredActivity).GetMethod(nameof(StructuredActivity.OnFinalizeAsync)).IsOverridenIn<TStructuredActivity>())
            {
                builder.AddOnFinalize(c => (c as BaseContext).NodeScope.GetStructuredActivity<TStructuredActivity>(c as IActionContext)?.OnFinalizeAsync());
            }
        }

        public static void AddObjectTransformationFlowEvents<TObjectTransformationFlow, TToken, TTransformedToken>(this IFlowBuilder<TToken> builder)
            where TObjectTransformationFlow : ObjectTransformationFlow<TToken, TTransformedToken>
            where TToken : Token, new()
            where TTransformedToken : Token, new()
        {
            if (typeof(ObjectTransformationFlow<TToken, TTransformedToken>).GetProperty(nameof(ObjectTransformationFlow<TToken, TTransformedToken>.Weight)).IsOverridenIn<TObjectTransformationFlow>())
            {
                var objectFlow = FormatterServices.GetUninitializedObject(typeof(TObjectTransformationFlow)) as TObjectTransformationFlow;

                builder.SetWeight(objectFlow.Weight);
            }

            if (typeof(ControlFlow).GetMethod(nameof(ControlFlow.GuardAsync)).IsOverridenIn<TObjectTransformationFlow>())
            {
                builder.AddGuard(c => (c as BaseContext).NodeScope.GetObjectTransformationFlow<TObjectTransformationFlow, TToken, TTransformedToken>(c)?.GuardAsync());
            }

            if (typeof(ObjectTransformationFlow<TToken, TTransformedToken>).GetMethod(nameof(ObjectTransformationFlow<TToken, TTransformedToken>.TransformAsync)).IsOverridenIn<TObjectTransformationFlow>())
            {
                builder.AddTransformation(c => (c as BaseContext).NodeScope.GetObjectTransformationFlow<TObjectTransformationFlow, TToken, TTransformedToken>(c)?.TransformAsync());
            }
        }

        public static void AddObjectFlowEvents<TObjectFlow, TToken>(this IFlowBuilder<TToken> builder)
            where TObjectFlow : ObjectFlow<TToken>
            where TToken : Token, new()
        {
            if (typeof(ObjectFlow<TToken>).GetProperty(nameof(ObjectFlow<TToken>.Weight)).IsOverridenIn<TObjectFlow>())
            {
                var objectFlow = FormatterServices.GetUninitializedObject(typeof(TObjectFlow)) as TObjectFlow;

                builder.SetWeight(objectFlow.Weight);
            }

            if (typeof(ControlFlow).GetMethod(nameof(ControlFlow.GuardAsync)).IsOverridenIn<TObjectFlow>())
            {
                builder.AddGuard(c => (c as BaseContext).NodeScope.GetObjectFlow<TObjectFlow, TToken>(c)?.GuardAsync());
            }
        }

        public static void AddControlFlowEvents<TControlFlow>(this IFlowBuilder builder)
            where TControlFlow : ControlFlow
        {
            if (typeof(ControlFlow).GetMethod(nameof(ControlFlow.GuardAsync)).IsOverridenIn<TControlFlow>())
            {
                builder.AddGuard(c => (c as BaseContext).NodeScope.GetControlFlow<TControlFlow>(c)?.GuardAsync());
            }
        }
    }
}