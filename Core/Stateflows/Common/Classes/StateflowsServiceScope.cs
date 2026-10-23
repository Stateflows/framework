using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Stateflows.Common.Engine;

namespace Stateflows.Common.Classes;

public class StateflowsServiceScope(AsyncServiceScope serviceScope) : IAsyncDisposable
{
    private readonly bool executionOwned = PendingTasksScope.Current != null;
    private readonly PendingTasks pendingTasks = PendingTasksScope.Current ?? new PendingTasks();
    
    public async ValueTask DisposeAsync()
    {
        try
        {
            await pendingTasks.DrainAsync();
        }
        finally
        {
            await serviceScope.DisposeAsync();
        }

        if (!executionOwned)
            pendingTasks.ThrowIfFailed();
    }
    
    public void AddPendingTask(Task task)
    {
        pendingTasks.Add(task);
    }
    
    public IServiceProvider ServiceProvider => serviceScope.ServiceProvider;
}