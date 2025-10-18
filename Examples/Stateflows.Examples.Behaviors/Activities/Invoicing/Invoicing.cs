using Stateflows.Activities;
using Stateflows.Examples.Behaviors.Activities.Invoicing.ActionNodes;
using Stateflows.Examples.Behaviors.Activities.Invoicing.Tokens;

namespace Stateflows.Examples.Behaviors.Activities.Invoicing;

public class Invoicing : IActivity
{
    public static void Build(IActivityBuilder builder)
        => builder
            .AddInitial(b => b
                .AddControlFlow<GenerateInvoices>()
            )
            .AddAction<GenerateInvoices>(b => b
                .AddFlow<Invoice, SendMail>()
                .AddFlow<Invoice, SendSMS>()
            )
            .AddAction<SendMail>(b => b
                .AddFlow<Invoice, SendNotification>()
            )
            .AddAction<SendSMS>(b => b
                .AddFlow<Invoice, SendNotification>()
            )
            .AddAction<SendNotification>();
}