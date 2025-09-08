using System;
using System.Reflection;
using Stateflows.Common;
using Stateflows.StateMachines.Models;
using Stateflows.StateMachines.Context.Interfaces;

namespace Stateflows.StateMachines.Registration.Extensions
{
    internal static class LambdaExtensions
    {
        public static Func<TContext, TResult> AddStateMachineInvocationContext<TContext, TResult>(this Func<TContext, TResult> lambda, Graph graph)
            where TContext : IBehaviorLocator
        {
            return lambda;
            var stateMachineType = graph.StateMachineType;
            var lambdaInfo = lambda.GetMethodInfo();
            return (stateMachineType != null && lambdaInfo.DeclaringType == stateMachineType)
                ? (TContext context) =>
                    {
                        var rootContext = (context as IRootContext).Context;
                        var stateMachineInstance = rootContext.Executor.GetStateMachineAsync(graph.StateMachineType).GetAwaiter().GetResult();
            
                        return (TResult)lambdaInfo.Invoke(stateMachineInstance, [ context ]);
                    }
            : lambda;
        }

        public static Action<TContext> AddStateMachineInvocationContext<TContext>(this Action<TContext> lambda, Graph graph)
            where TContext : IBehaviorLocator
        {
            return lambda;
            var stateMachineType = graph.StateMachineType;
            var lambdaInfo = lambda.GetMethodInfo();
            return (stateMachineType != null && lambdaInfo.DeclaringType == stateMachineType)
                ? (TContext context) =>
                    {
                        var rootContext = (context as IRootContext).Context;
                        var stateMachineInstance = rootContext.Executor.GetStateMachineAsync(graph.StateMachineType).GetAwaiter().GetResult();

                        lambdaInfo.Invoke(stateMachineInstance, [ context ]);
                    }
                : lambda;
        }
    }
}
