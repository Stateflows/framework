using Stateflows.Activities;
using Stateflows.Common;
using Stateflows.Examples.Behaviors.Activities.Invoicing.Tokens;
using Stateflows.Examples.Common.Events;

namespace Stateflows.Examples.Behaviors.Activities.Invoicing.ActionNodes;

public class SendNotification(
    IOwnerBehaviorContext behaviorContext,
    IInputToken<Invoice> invoice
) : IActionNode
{
    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        await behaviorContext.TryMutateAsync("Jane");
        
        behaviorContext.Publish(new InvoiceNotification { InvoiceNumber = invoice.Token.InvoiceNumber });
    }
}