using Stateflows.Activities;
using Stateflows.Extensions.MinimalAPIs;
using Stateflows.Extensions.MinimalAPIs.Interfaces;
using WarszawskieDniInformatyki.Activities.Process.Events;

namespace WarszawskieDniInformatyki.Activities.Process;

public class Process : IActivity, IActivityEndpoints
{
    public void Build(IActivityBuilder builder)
        => builder
            .AddInitial(b => b
                .AddControlFlow("firstAction")
            )
            .AddAction(
                "firstAction", 
                async c => c.OutputRange<int>([1, 2, 3]),
                b => b
                    // .AddFlow<int, AcceptEventActionNode<Input>>(b => b.SetWeight(0))
                    .AddFlow<int>("thirdAction")
            )
            .AddAcceptEventAction<Input>(
                async c => c.OutputRange(["1", "2", "3"]),
                b => b
                    .AddFlow<string>("secondAction", b => b.SetWeight(8))
                    .AddFlow<string>("thirdAction")
            )
            .AddAction(
                "secondAction",
                async c => { }
            )
            .AddAction(
                "thirdAction",
                async c => { }
            )
            ;

    public void RegisterEndpoints(IEndpointsBuilder endpointsBuilder)
    {
        endpointsBuilder
            .AddGet("/global", async (IActivityEndpointContext context) =>
            {
                await context.Behavior.Values.UpdateAsync("counter", c => c + 1, 0);
                return Results.Ok(
                    context.Response(
                        await context.Behavior.Values.GetOrDefaultAsync<int>("counter")
                    )
                );
            });
    }
}