using Examples.Common;
using Stateflows;
using Stateflows.StateMachines;

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

app.MapStateflowsTransportHub();

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
