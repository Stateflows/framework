using System;
using System.Linq;
using System.Threading.Tasks;
using Stateflows.Common.Classes;
using Stateflows.StateMachines.Context.Interfaces;

namespace Stateflows.StateMachines;

internal static class DelegateExtensions
{
    public static object InvokeDelegate(this Delegate @delegate, Func<Task<object>>[] valueFactories)
    {
        object invocationResult;
        try
        {
            var args = valueFactories
                .Select(f => f())
                .Select(x => x.Result)
                .ToArray();
            invocationResult = @delegate.DynamicInvoke(args: args);
        }
        catch (Exception e)
        {
            throw e.InnerException!;
        }

        return invocationResult;
    }
    
    public static async Task<bool> InvokeDelegatePredicateAsync(this Delegate @delegate, Func<Task<object>>[] valueFactories)
    {
        var invocationResult = InvokeDelegate(@delegate, valueFactories);

        return invocationResult switch
        {
            bool response => response,
            Task<bool> response => await response,
            _ => throw new InvalidOperationException("No bool returned from guard delegate")
        };
    }

    public static async Task InvokeDelegateActionAsync(this Delegate @delegate, Func<Task<object>>[] valueFactories)
    {
        var invocationResult = InvokeDelegate(@delegate, valueFactories);

        if (invocationResult is Task task)
        {
            await task;
        }
    }
}