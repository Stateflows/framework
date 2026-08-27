using System.ComponentModel;
using System.Diagnostics;
using Azure.AI.Projects;
using Azure.Identity;
using Microsoft.Agents.AI;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
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
using Stateflows.Entities.Attributes;
using Stateflows.Entities.Enums;
using Stateflows.MAF.AIAgents;
using Stateflows.MAF.AIAgents.Extensions;
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

public static class DocumentManager
{
    public static string Document() => "Personne n’a sans doute oublié le terrible coup de vent de nord-est qui se déchaîna au milieu de l’équinoxe de cette année, et pendant lequel le baromètre tomba à sept cent dix millimètres. Ce fut un ouragan, sans intermittence, qui dura du 18 au 26 mars. Les ravages qu’il produisit furent immenses en Amérique, en Europe, en Asie, sur une zone large de dix-huit cents milles, qui se dessinait obliquement à l’équateur, depuis le trente-cinquième parallèle nord jusqu’au quarantième parallèle sud ! Villes renversées, forêts déracinées, rivages dévastés par des montagnes d’eau qui se précipitaient comme des mascarets, navires jetés à la côte, que les relevés du Bureau-Veritas chiffrèrent par centaines, territoires entiers nivelés par des trombes qui broyaient tout sur leur passage, plusieurs milliers de personnes écrasées sur terre ou englouties en mer : tels furent les témoignages de sa fureur, qui furent laissés après lui par ce formidable ouragan. Il dépassait en désastres ceux qui ravagèrent si épouvantablement la Havane et la Guadeloupe, l’un le 25 octobre 1810, l’autre le 26 juillet 1825.";
}

public class ReviewerAgent(IConfiguration configuration) : IAIAgent
{
    public string? InitialPrompt => "Please analyse the document and provide your opinion about it.";
    public async Task<AIAgent> BuildAgentAsync(IAIAgentContext aiAgentContext, AITool[] frameworkTools)
    {
        var ProjectClient = new AIProjectClient(
            new Uri(configuration["Foundry:Endpoint"]!),
            new AzureCliCredential()
        );
                
        return ProjectClient.AsAIAgent(
            model: configuration["Foundry:Model"]!,
            instructions: "You are a helpful assistant in the area of literature.",
            tools: [
                ..frameworkTools,
                AIFunctionFactory.Create(
                    () => DocumentManager.Document(),
                    new AIFunctionFactoryOptions()
                    {
                        Name = "get_document_content",
                        Description = "Returns document content"
                    }
                )
            ]
        );
    }
}

public class TranslatorAgent(IConfiguration configuration) : IAIAgent
{
    public string? InitialPrompt => "Translate document to English language.";
    public async Task<AIAgent> BuildAgentAsync(IAIAgentContext aiAgentContext, AITool[] frameworkTools)
    {
        var ProjectClient = new AIProjectClient(
            new Uri(configuration["Foundry:Endpoint"]!),
            new AzureCliCredential()
        );
                
        return ProjectClient.AsAIAgent(
            model: configuration["Foundry:Model"]!,
            instructions: "You are a translation agent.",
            tools: [
                ..frameworkTools,
                AIFunctionFactory.Create(
                    () => DocumentManager.Document(),
                    new AIFunctionFactoryOptions()
                    {
                        Name = "get_document_content",
                        Description = "Returns document content"
                    }
                )
            ]
        );
    }
}

public class BaseDocument : IStateMachine
{
    public static void Build(IStateMachineBuilder builder) => builder
        .AddProjectionSubscription<string>()
        
        .AddInitialState<New>(b => b
            .AddTransition<BaseReview, ApprovalPending>(b => b
                .AddEffect(async c => c.Event.Respond(new ReviewResponse()
                    { Summary = $"{c.Event.Content}: {c.Event.Rating}" }))
            )
            .AddTransition<AfterOneMinute, ReportAutorejection, Rejected>()
            .AddDefaultTransition<AgenticState<ReviewerAgent>>()
        
            .AddDoAction<UniversalAction>()
        )
        
        .AddAgenticState<ReviewerAgent>(b => b
                .AddAgenticTransition<Rejected>("Move here if user likes document")
                .AddAgenticTransition<Approved>("Move here if user dislikes document")
            
                .AddAgenticHandoff<TranslatorAgent>("Move here if user wants to translate document.")
            
            // .AddAgenticReaction<string>(c => new ChatMessage(ChatRole.User, [ new TextContent($"Received event: {c.Event}") ]))
        )
        
        .AddAgenticState<TranslatorAgent>(b => b
                .AddAgenticHandoff<TranslatorAgent>("Move here if user expresses like or dislike towards the document.")
            
            // .AddAgenticReaction<string>(c => new ChatMessage(ChatRole.User, [ new TextContent($"Received event: {c.Event}") ]))
        )
    ;
}

[StateMachineBehavior]
public class Document : IStateMachine
{
    public class PatientIdsResponse
    {
        public string CorrelationId { get; set; }
        
        public string[] PatientIds { get; set; }

        public int PatientIdsTotal { get; set; }
    }

    public interface ICompletable
    {
        bool Completed { get; }
    }
    
    public class PatientIdsCompleted : ICompletable
    {
        public bool Completed { get; set; }
    }
    
    public interface PatientIdsEntity
    {
        [Field]
        public List<string> PatientIds { get; set; }
        
        [Field, DefaultValue(int.MaxValue)]
        public int PatientIdsTargetCount { get; set; }
        
        [Field]
        public bool PatientIdsComplete
            => PatientIds.Count >= PatientIdsTargetCount;

        [Mutation]
        public void AppendPatientIds(PatientIdsResponse response)
        {
            PatientIds = [..PatientIds, ..response.PatientIds];
            PatientIdsTargetCount = Math.Min(PatientIdsTargetCount, response.PatientIdsTotal);
        }

        [Projection]
        public string[] GetPatientIds
            => PatientIds.ToArray() ?? [];
        
        [Projection(PublishScope.Self)]
        public PatientIdsCompleted ArePatientIdsCompleted
            => new PatientIdsCompleted() { Completed = PatientIdsComplete };
    }
    
    private interface Entity : IAIAgentSessionEntity, PatientIdsEntity
    {
        [Field, DefaultValue(42)] int ProcessId { get; set; }
        [Field] string FirstName { get; set; }
        [Field] string LastName { get; set; }
        [Field] string FullName => $"{FirstName} {LastName}";

        [Projection(PublishScope.Self)] string Description => FullName;
        [Mutation] void SetFirstName(string firstName) => FirstName = firstName;

        [DefaultInitializer]
        void DefaultInitializer()
        {
            ProcessId = 42;
            FirstName = "John";
            LastName = "Doe";
        }
    }
    
    public static void Build(IStateMachineBuilder builder) => builder
        .AddEntity<Entity>()
        .AddProjectionSubscription<PatientIdsCompleted>()
        
        .AddInterceptor<HttpContextInterceptor>()
        .UseStateMachine<BaseDocument>(b => b
            .UseState<New>(b => b
                .UseTransition<BaseReview, ApprovalPending>(b => b
                    .ChangeTrigger<Review>()
                    .AddEffect(c => c.Behavior.TryMutateAsync(c.Event.Content))
                    .AddEffect(c => c.Behavior.TryMutateAsync(new PatientIdsResponse()
                    {
                        CorrelationId = "",
                        PatientIds = ["a", "b", "c"],
                        PatientIdsTotal = 3
                    }))
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