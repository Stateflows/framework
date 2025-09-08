using System.Diagnostics;
using Stateflows.Common;
using Stateflows.Common.Attributes;
using Stateflows.Extensions.MinimalAPIs;
using Stateflows.Extensions.MinimalAPIs.Interfaces;
using Stateflows.StateMachines;
using WarszawskieDniInformatyki.Actions.Work;
using WarszawskieDniInformatyki.Activities.Process.Events;
using WarszawskieDniInformatyki.StateMachines.Document.Effects;
using WarszawskieDniInformatyki.StateMachines.Document.Events;
using WarszawskieDniInformatyki.StateMachines.Document.Guards;
using WarszawskieDniInformatyki.StateMachines.Document.States;

namespace WarszawskieDniInformatyki.StateMachines.Document;

public struct Test {}
public enum EnumTest {}

public class DocumentExceptionHandler([GlobalValue] IValue<int> counter) : StateMachineExceptionHandler
{
    public override bool OnStateEntryException(IStateActionContext context, Exception exception)
    {
        Debug.WriteLine(exception.Message);
        return true;
    }
}

public class Document : IStateMachine
{
    public static void Build(IStateMachineBuilder builder)
        => builder
            .AddDefaultInitializer(async c =>
            {
                await c.Behavior.Values.SetAsync("counter", 42);

                return true;
            })
            
            .AddEndpoints(b =>
            {
                b.AddGet("/global", async (IBehaviorEndpointContext context) =>
                    {
                        await context.Behavior.Values.UpdateAsync("counter", c => c + 1, 0);
                        return Results.Ok(
                            context.Response(
                                await context.Behavior.Values.GetOrDefaultAsync<int>("counter")
                            )
                        );
                    });

                b.AddPost("/operation", async () => Results.Ok());
            })
            
            .AddInitialState<New>(b => b
                .AddOnEntry(async c =>
                {
                    await c.State.Values.UpdateAsync("x", c => c + 1, 0);
                })
                .AddTransition<Review, Reviewed>(b => b
                    .AddEffect<ReviewEffect>()
                )
                .AddTransition<AfterFiveMinutes, Rejected>()
            
                .AddEndpoints(b => b
                    .AddGet("/data", async (IStateEndpointContext context) => Results.Ok(await context.State.Values.GetOrDefaultAsync<int>("x")))
                )
            )
            .AddState<Reviewed>(b => b
                .AddOnEntryAction<Work>()
                .AddTransition<Accept, Accepted>(b => b
                    .AddEffect(c => c.Event.Respond(new AcceptResponse()))
                )
            )
            .AddState<Accepted>(b => b
                .AddDoActivity("Proc")
                .AddTransition<Pay, PayGuard, Paid>()
                .AddTransition<Reject, Rejected>()
            )
            .AddState<Paid>()
            .AddState<Rejected>()
        ;
}