using Stateflows.Actions;

namespace WarszawskieDniInformatyki.Actions.Work;

public class Work : IAction
{
    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        await Task.Delay(3000, cancellationToken);
    }
}