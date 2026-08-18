using System.ComponentModel;
using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Stateflows.Actions;
using Stateflows.Common;
using Stateflows.Examples.Behaviors.Activities.Invoicing;
using Stateflows.Examples.Behaviors.StateMachines.Document.Effects;
using Stateflows.Examples.Behaviors.StateMachines.Document.Guards;
using Stateflows.Examples.Behaviors.StateMachines.Document.Interceptors;
using Stateflows.Examples.Behaviors.StateMachines.Document.States;
using Stateflows.Examples.Common.Events;
using Stateflows.StateMachines;
using Stateflows.Activities;
using Stateflows.Entities;
using Stateflows.Entities.Attributes;
using Stateflows.Entities.Enums;
using Stateflows.Extensions.MinimalAPIs;
using Stateflows.StateMachines.Attributes;
using IExecutionContext = Stateflows.Common.IExecutionContext;

namespace Stateflows.Examples.Behaviors.StateMachines.Document;

public class UniversalAction(IBehaviorContext bc, IExecutionContext ec) : IActionElement
{
    public Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        Trace.WriteLine("UniversalAction called");
        
        return Task.CompletedTask;
    }
}

// public class ReviewingAgent : IAgent
// {
//     public string GuardPrompt => "Check external API via MCP if I can proceed with my process";
//     public string SystemPrompt => "Generate review/summary for my document, i'm gonna provide feedback in loop until satisfied";
// }

public class BaseDocument : IStateMachine
{
    public static void Build(IStateMachineBuilder builder) => builder
        .AddInitialState<New>(b => b
            .AddProjectionSubscription<int>()
            .AddTransition<BaseReview, ApprovalPending>(b => b
                .AddEffect(async c => c.Event.Respond(new ReviewResponse()
                    { Summary = $"{c.Event.Content}: {c.Event.Rating}" }))
            )
            .AddTransition<AfterOneMinute, ReportAutorejection, Rejected>()
        );
}

[StateMachineBehavior]
public class Document : IStateMachine
{
    private interface Entity
    {
        [Field, DefaultValue(42)] int ProcessId { get; set; }
        [Field] string FirstName { get; set; }
        [Field] string LastName { get; set; }
        [Field] string FullName => $"{FirstName} {LastName}";

        [Projection(PublishScope.Owner)] string Description => FullName;
        [Mutation] void SetFirstName(string firstName) => FirstName = firstName;
    }
    
    public static void Build(IStateMachineBuilder builder) => builder
        .AddEntity<Entity>(b => b
            .AddDefaultInitializer(c =>
            {
                c.Entity.ProcessId = 42;
                c.Entity.FirstName = "John";
                c.Entity.LastName = "Doe";
            })
        )
        .AddInterceptor<HttpContextInterceptor>()
        .UseStateMachine<BaseDocument>(b => b
            .UseState<New>(b => b
                .UseTransition<BaseReview, ApprovalPending>(b => b
                    .ChangeTrigger<Review>()
                )
            )
        )
        // .AddInitialState<New>(b => b
        //     .AddProjectionSubscription<int>()
        //     .AddTransition<Review, ApprovalPending>(b => b
        //         .AddEffect(async c => c.Event.Respond(new ReviewResponse() { Summary = $"{c.Event.Content}: {c.Event.Rating}"}))
        //     )
        //     .AddTransition<AfterOneMinute, ReportAutorejection, Rejected>()
        // )
        .AddState<ApprovalPending>(b => b
            .AddTransition<Approve, Approved>()
            .AddTransition<Reject, ReportRejection, Rejected>()
        )
        .AddCompositeState<Approved>(b => b
            .AddInitialState<GeneratingInvoice>(b => b
                .AddDoActivity<Invoicing>(b => b
                    // .AddFinalizedNotificationPolicy()
                    .AddCompletionNotificationPolicy()
                )
                // .AddTransition<DoActivityFinalized, InvoiceGenerated>()
                
                .AddDefaultTransition<Paid>(b => b
                    .AddGuard(async c =>
                    {
                        var (success, fullName) = await c.Behavior.TryGetProjectionAsync<string>();
                        
                        return success && fullName == "Jane Doe";
                    })
                )
            )
            .AddState<InvoiceGenerated>(b => b
                .AddTransition<PaymentBooked, VerifyPayment, Paid>()
            )
        )
        .AddState<Paid>(b => b
            .AddInternalTransition<Reject>(b => b
                .AddEffectAction(
                    async c =>
                    {
                        await Task.Delay(5000);
                        await c.Behavior.Values.UpdateAsync("counter", c => c + 1, 0);
                    },
                    b => b
                        .AddCompletionNotificationPolicy()
                        .SetResourceName("heavy-work")
                )
            )
            .AddDefaultTransition<Rejected>(b => b
                .AddGuard(async c => await c.Behavior.Values.GetOrDefaultAsync<int>("counter") >= 5)
            )
        )
        .AddState<Rejected>()
    ;
}