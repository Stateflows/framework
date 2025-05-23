using Stateflows.Activities;
using Stateflows.Extensions.MinimalAPIs;
using Stateflows.Extensions.MinimalAPIs.Interfaces;
using WarszawskieDniInformatyki.Activities.Process.Events;

namespace WarszawskieDniInformatyki.Activities.Process;

public class Process : IActivity, IActivityEndpoints
{
    public void Build(IActivityBuilder builder)
        => builder
            .AddDefaultInitializer(async c =>
            {
                await c.Behavior.Values.SetAsync("counter", 42);

                return true;
            })

            .AddEndpoints(b => b
                .AddGet("/global", async (IActivityEndpointContext context) =>
                {
                    await context.Behavior.Values.UpdateAsync("counter", c => c + 1, 0);
                    return Results.Ok(
                        context.Response(
                            await context.Behavior.Values.GetOrDefaultAsync<int>("counter")
                        )
                    );
                })
            )
            
            .AddInitial(b => b
                .AddControlFlow("first")
                .AddControlFlow("low")
            )
            .AddAcceptEventAction<Input>("low", async c => c.ToString())
            .AddStructuredActivity("first", b => b
                .AddInitial(b => b
                    .AddControlFlow("second")
                    .AddControlFlow("high")
                )
                .AddAcceptEventAction<Input>("high", async c => c.ToString())
                .AddStructuredActivity("second", b => b
                    .AddInitial(b => b
                        .AddControlFlow("higher")
                    )
                    .AddAcceptEventAction<Input>("higher", async c => c.ToString())
                )
                .AddEndpoints(b => b
                    .AddGet("/data",
                        async (IActivityNodeEndpointContext context) =>
                            Results.Ok(
                                context.Response(
                                    await context.Behavior.Values.GetOrDefaultAsync<int>("counter")
                                )
                            )
                    )
                )
            );

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