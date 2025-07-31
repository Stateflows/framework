using Stateflows.Actions;

namespace WarszawskieDniInformatyki.Actions.Work;

public class Work : IAction
{
    public Task ExecuteAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}