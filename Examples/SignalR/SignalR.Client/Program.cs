using Examples.Common;
using SignalR.Client;
using Stateflows;
using Stateflows.Activities;
using Stateflows.Common;
using Stateflows.StateMachines;
using Stateflows.StateMachines.Sync;
using Stateflows.StateMachines;
using Stateflows.Transport.Http;

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
                .AddTransition<AfterOneMinute>("state2")
            )
            .AddState("state2", b => b
                .AddOnEntry(c => c.StateMachine.Publish(new SomeNotification()))
                .AddTransition<OtherEvent>("state1")
                .AddTransition<AfterOneMinute>("state1")
            )
        )
    )


    //.AddActivities(b => b
    //    .AddActivity("handleVerification", b => b
    //        .AddAcceptEventAction<Event<VerificationRequest>>(async c =>
    //            {
    //                var allDossierIds = getDossierIds(c.SelectionCriteria);
    //                c.OutputRange(allDossierIds.ToTokens());
    //            },
    //            b => b.AddFlow<Token<int>>("processing")
    //        )

    //        .AddIterativeActivity<Token<int>>(
    //            "processing",
    //            b => b
    //                .AddInput(b => b
    //                    .AddFlow<Token<int>>("process")
    //                )
    //                .AddTimeEventAction<EveryFiveMinutes>(b => b
    //                    .AddControlFlow("process")
    //                )
    //                .AddAction("process",
    //                    async c => { /* processing */ }
    //                ),
    //            10
    //        )  
    //    )
    //)


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
app.MapStateflowsHttpTransport();

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
