using Stateflows.Activities;
using Stateflows.Examples.Behaviors.Activities.Invoicing.ActionNodes;
using Stateflows.Examples.Behaviors.Activities.Invoicing.Tokens;
using Stateflows.Examples.Behaviors.StateMachines.Document;
using Stateflows.MAF.AIAgents.Extensions;

namespace Stateflows.Examples.Behaviors.Activities.Invoicing;

public class Invoicing : IActivity
{
    public static void Build(IActivityBuilder builder)
        => builder
            // .UseActivity<Test.Test>(b => b
            //     .UseIterativeActivity<int>("x", b => b
            //         
            //     )
            // )
            .AddInitial(b => b
                .AddControlFlow<GenerateInvoices>()
                .AddControlFlow("js")
            )
            .AddAction_ClearScript("js", "Console.WriteLine(JSON.stringify(behaviorContext.Id))")
            // .AddAgenticActivity<ReviewerAgent>()
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