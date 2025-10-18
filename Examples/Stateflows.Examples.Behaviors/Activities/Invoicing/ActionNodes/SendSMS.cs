using Stateflows.Activities;
using Stateflows.Common;
using Stateflows.Common.Attributes;
using Stateflows.Examples.Behaviors.Activities.Invoicing.Tokens;

namespace Stateflows.Examples.Behaviors.Activities.Invoicing.ActionNodes;

public class SendSMS(IInputOutputToken<Invoice> invoice, [GlobalValue] IValue<bool> smsSent) : IActionNode
{
    public Task ExecuteAsync(CancellationToken cancellationToken)
    {
        invoice.PassOn();
        
        // send invoice.Token via SMS

        return smsSent.SetAsync(true);
    }
}