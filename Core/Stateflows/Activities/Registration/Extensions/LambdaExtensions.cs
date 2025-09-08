using System;
using System.Reflection;
using Stateflows.Common;
using Stateflows.Activities.Models;
using Stateflows.Activities.Context.Interfaces;

namespace Stateflows.Activities.Registration.Extensions
{
    internal static class LambdaExtensions
    {
        public static Func<TContext, TResult> AddActivityInvocationContext<TContext, TResult>(this Func<TContext, TResult> lambda, Graph graph)
            where TContext : IBehaviorLocator
        {
            return lambda;
            var activityType = graph.ActivityType;
            var lambdaInfo = lambda.GetMethodInfo();
            return (activityType != null && lambdaInfo.DeclaringType == activityType)
                ? (TContext context) =>
                    {
                        var rootContext = (context as IRootContext).Context;
                        var activityInstance = rootContext.Executor.GetActivityAsync(graph.ActivityType).GetAwaiter().GetResult();

                        return (TResult)lambdaInfo.Invoke(activityInstance, [ context ]);
                    }
                : lambda;
        }

        public static Action<TContext> AddActivityInvocationContext<TContext>(this Action<TContext> lambda, Graph graph)
            where TContext : IBehaviorLocator
        {
            return lambda;
            var activityType = graph.ActivityType;
            var lambdaInfo = lambda.GetMethodInfo();
            return (activityType != null && lambdaInfo.DeclaringType == activityType)
                ? (TContext context) =>
                    {
                        var rootContext = (context as IRootContext).Context;
                        var activityInstance = rootContext.Executor.GetActivityAsync(graph.ActivityType).GetAwaiter().GetResult();

                        lambdaInfo.Invoke(activityInstance, [ context ]);
                    }
                : lambda;
        }
    }
}
