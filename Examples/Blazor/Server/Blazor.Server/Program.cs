using Blazor.Server.Data;
using Examples.Common;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Stateflows;
using Stateflows.StateMachines;
using Stateflows.Activities;
using System.Diagnostics;
using Stateflows.Activities.Context.Interfaces;
using System.Text;
using Stateflows.StateMachines.Context.Interfaces;
using Stateflows.Common;
using System.ComponentModel.Design;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Blazor.Server.Behaviors.Activities.Activity1;
//using Stateflows.Extensions.Scripting.JS;
using JavaScriptEngineSwitcher.Jurassic;
using Blazor.Server.Behaviors.Activities.ExampleActivity;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddSingleton<WeatherForecastService>();

builder.Services.AddSignalR();

builder.Services.AddStateflows(b => b
    .AddPlantUml()

    .AddActivity<Activity1>()
    .AddActivity<ExampleActivity>()
);

var app = builder.Build();

app.MapStateflowsTransportHub();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();