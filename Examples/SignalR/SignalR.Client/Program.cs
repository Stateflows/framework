using Examples.Common;
using SignalR.Client;
using Stateflows;
using Stateflows.Common;
using Stateflows.StateMachines;
using Stateflows.StateMachines.Sync;
using Stateflows.StateMachines.Typed;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSignalR(options =>
{
    options.EnableDetailedErrors = true;
});

builder.Services.AddStateflows(b => b
    .AddStateMachines(b => b
        .AddStateMachine("stateMachine1", b => b
            .AddInitialState("state1", b => b
                .AddTransition<OtherEvent>("state2")
                .AddHttpGetInternalTransition<Payload>("/my-url")
            )
            .AddState("state2", b => b
                .AddTransition<OtherEvent>("state1")
            )
        )
    )
    .AddPlantUml()
    .SetEnvironment(
        builder.Environment.IsDevelopment()
            ? $"{StateflowsEnvironments.Development}.{Environment.MachineName}"
            : StateflowsEnvironments.Production
    )
);

// Add services to the container.

builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.MapStateflowsSignalRTransport();

app.UseCors(builder => builder
  .AllowAnyHeader()
  .AllowAnyMethod()
  .SetIsOriginAllowed((host) => true)
  .AllowCredentials()
);

app.MapControllerRoute(
    name: "default",
    pattern: "{controller}/{action=Index}/{id?}");

app.MapFallbackToFile("index.html");

app.Run();

public class Tran1 : Transition<HttpRequest<Payload, Payload>> { }
public class Tran2 : Transition<HttpEvent<Payload>> { }