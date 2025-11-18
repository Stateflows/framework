using Stateflows.Activities;
using Stateflows.Common;
using Stateflows.Common.Attributes;
using Stateflows.Examples.Behaviors.Activities.Invoicing.Tokens;

namespace Stateflows.Examples.Behaviors.Activities.Invoicing.ActionNodes;

public class SendMail(IInputOutputToken<Invoice> invoice, [GlobalValue] IValue<bool> mailSent) : IActionNode
{
    public Task ExecuteAsync(CancellationToken cancellationToken)
    {
        invoice.PassOn();
        
        // send invoice.Token via email
        
        return mailSent.SetAsync(true);
    }
}