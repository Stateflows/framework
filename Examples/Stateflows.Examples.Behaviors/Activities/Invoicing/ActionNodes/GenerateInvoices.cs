using Stateflows.Activities;
using Stateflows.Examples.Behaviors.Activities.Invoicing.Tokens;

namespace Stateflows.Examples.Behaviors.Activities.Invoicing.ActionNodes;

public class GenerateInvoices(IOutputTokens<Invoice> invoicesOutput) : IActionNode
{
    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        // pretend that heavy work is going on
        for (var i = 0; i < 5; i++)
        {
            await Task.Delay(1000, cancellationToken);
            
            if (cancellationToken.IsCancellationRequested) return;
        }
        
        invoicesOutput.Add(new Invoice());
    }
}