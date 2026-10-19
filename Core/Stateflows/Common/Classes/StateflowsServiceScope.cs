using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace Stateflows.Common.Classes;

public class StateflowsServiceScope(AsyncServiceScope serviceScope) : IAsyncDisposable
{
    private readonly List<Task> PendingTasks = [];
    
    public async ValueTask DisposeAsync()
    {
        if (PendingTasks.Count > 0)
        {
            await Task.WhenAll(PendingTasks);
        }
        
        await serviceScope.DisposeAsync();
    }
    
    public void AddPendingTask(Task task)
    {
        PendingTasks.Add(task);
    }
    
    public IServiceProvider ServiceProvider => serviceScope.ServiceProvider;
}