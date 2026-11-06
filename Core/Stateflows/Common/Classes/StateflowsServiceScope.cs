using System;
using System.Collections.Generic;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Stateflows.Common.Engine;

namespace Stateflows.Common.Classes;

public class StateflowsServiceScope(AsyncServiceScope serviceScope) : IAsyncDisposable
{
    private readonly bool executionOwned = PendingTasksScope.Current != null;
    private readonly PendingTasks pendingTasks = PendingTasksScope.Current ?? new PendingTasks();
    private List<StateflowsServiceScope>? childScopes;

    internal StateflowsServiceScope CreateChildScope()
    {
        var child = new StateflowsServiceScope(serviceScope.ServiceProvider.CreateAsyncScope());
        (childScopes ??= []).Add(child);
        return child;
    }
    
    public async ValueTask DisposeAsync()
    {
        try
        {
            await pendingTasks.DrainAsync();
        }
        finally
        {
            await DisposeServiceScopesAsync();
        }

        if (!executionOwned)
            pendingTasks.ThrowIfFailed();
    }

    private async ValueTask DisposeServiceScopesAsync()
    {
        if (childScopes == null)
        {
            await serviceScope.DisposeAsync();
            return;
        }

        List<Exception> exceptions = [];
        for (var i = childScopes.Count - 1; i >= 0; i--)
        {
            try
            {
                await childScopes[i].DisposeAsync();
            }
            catch (Exception exception)
            {
                exceptions.Add(exception);
            }
        }
        childScopes.Clear();

        try
        {
            await serviceScope.DisposeAsync();
        }
        catch (Exception exception)
        {
            exceptions.Add(exception);
        }

        if (exceptions.Count == 1)
            ExceptionDispatchInfo.Capture(exceptions[0]).Throw();
        if (exceptions.Count > 1)
            throw new AggregateException(exceptions);
    }
    
    public void AddPendingTask(Task task)
    {
        pendingTasks.Add(task);
    }
    
    public IServiceProvider ServiceProvider => serviceScope.ServiceProvider;
}