﻿using System;
using System.Reflection;
using Stateflows.Common.Interfaces;
using Stateflows.StateMachines.Models;
using Stateflows.StateMachines.Context.Interfaces;

namespace Stateflows.StateMachines.Registration.Extensions
{
    internal static class LambdaExtensions
    {
        public static Func<TContext, TResult> AddStateMachineInvocationContext<TContext, TResult>(this Func<TContext, TResult> lambda, Graph graph)
            where TContext : IBehaviorLocator
        {
            var stateMachineType = graph.StateMachineType;
            var lambdaInfo = lambda.GetMethodInfo();
            return (stateMachineType != null && lambdaInfo.DeclaringType == stateMachineType)
                ? (TContext context) =>
                    {
                        var rootContext = (context as IRootContext).Context;
                        var stateMachineInstance = rootContext.Executor.GetStateMachine(graph.StateMachineType, rootContext);

                        return (TResult)lambdaInfo.Invoke(stateMachineInstance, new object[] { context });
                    }
                : lambda;
        }

        public static Action<TContext> AddStateMachineInvocationContext<TContext>(this Action<TContext> lambda, Graph graph)
            where TContext : IBehaviorLocator
        {
            var stateMachineType = graph.StateMachineType;
            var lambdaInfo = lambda.GetMethodInfo();
            return (stateMachineType != null && lambdaInfo.DeclaringType == stateMachineType)
                ? (TContext context) =>
                    {
                        var rootContext = (context as IRootContext).Context;
                        var stateMachineInstance = rootContext.Executor.GetStateMachine(graph.StateMachineType, rootContext);

                        lambdaInfo.Invoke(stateMachineInstance, new object[] { context });
                    }
                : lambda;
        }
    }
}
