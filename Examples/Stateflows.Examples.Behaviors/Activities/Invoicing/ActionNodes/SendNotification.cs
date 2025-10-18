using Stateflows.Activities;
using Stateflows.Common;
using Stateflows.Examples.Behaviors.Activities.Invoicing.Tokens;
using Stateflows.Examples.Common.Events;

namespace Stateflows.Examples.Behaviors.Activities.Invoicing.ActionNodes;

public class SendNotification(IBehaviorContext behaviorContext, IInputToken<Invoice> invoice) : IActionNode
{
    public Task ExecuteAsync(CancellationToken cancellationToken)
    {
        behaviorContext.Publish(new InvoiceNotification { InvoiceNumber = invoice.Token.InvoiceNumber });

        return Task.CompletedTask;
    }
}