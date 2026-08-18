using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Stateflows.Activities;
using Stateflows.Activities.Registration.Interfaces;
using Stateflows.Common;
using Stateflows.Common.Classes;
using Stateflows.Common.Utilities;
using Stateflows.Examples.Common.Events;
using Stateflows.Extensions.MinimalAPIs;
using Stateflows.Extensions.MinimalAPIs.Headers;
using Stateflows.StateMachines;

namespace Stateflows.Examples.Behaviors.Activities.Test;

public interface IServ
{
    Task<string> DoAsync(string value);
}

public class Serv : IServ
{
    public async Task<string> DoAsync(string value)
        => $"{value} is altered";
}

public class ServHeader : EventHeader
{
    public string Value { get; set; }
}

public class TestInterceptor(IServiceProvider serviceProvider) : BehaviorInterceptor
{
    public override async Task NotificationPublishedAsync<TNotification>(IBehaviorActionContext context, TNotification notification,
        IDictionary<string, EventHeader> headers)
    {
        var s = serviceProvider.GetRequiredService<IServ>();
        headers.Add("x", new ServHeader() { Value = await s.DoAsync("x") });
    }
}

internal static class ObjectFlowBuilderExtensions
{
    public static IObjectFlowBuilder<BaseGetData> AddDataScopeGuard(this IObjectFlowBuilder<BaseGetData> builder, string scopeName)
        => builder.AddGuard(async c =>
            !c.Token.Scope.Any() ||
            c.Token.Scope.Contains(scopeName, StringComparer.OrdinalIgnoreCase)
        );
}

public class X(IOutputTokens<BaseGetData> eventOutput, IOutputTokens<int> intOutput) : IAcceptEventActionNode<BaseGetData>
{
    public Task ExecuteAsync(BaseGetData @event, CancellationToken cancellationToken)
    {
        eventOutput.Add(@event);
        intOutput.Add(42);

        return Task.CompletedTask;
    }
}

public class BaseTest : IActivity
{
    public static void Build(IActivityBuilder builder) => builder
        .AddAcceptEventAction<BaseGetData, X>(b => b
            .AddFlow<BaseGetData>("getDataAction", b => b
                .AddDataScopeGuard("x")
            )
            // .AddControlFlow("getDataAction")
        )
        .AddAction(
            "getDataAction",
            async c =>
            {
                // var accessor = c.Behavior.ServiceProvider.GetRequiredService<IHttpContextAccessor>();
                var header = c.Headers.Values.OfType<HttpContextHeader>().FirstOrDefault();

                try
                {
                    var ctxData = StateflowsJsonConverter.SerializePolymorphicObject(header);
                    var ctx = StateflowsJsonConverter.DeserializeObject(ctxData) as HttpContextHeader;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
                
                Debug.WriteLine(string.Join(", ", header.Headers.Keys));
                for (var i = 0; i < 10; i++)
                {
                    await Task.Delay(200);
                    
                    c.Behavior.Publish(new List<Dictionary<string, DataNotification>>() { new() { { "x", new() { Data = $"Lorem ipsum #{i}" } } } });
                }
            },
            b => b.AddControlFlow("final")
        )
        .AddAction(
            "final",
            async c => c.Behavior.Publish("finis"),
            b => b.AddControlFlow<FinalNode>()
        )
        .AddFinal()
        .AddFinalizationResetPolicy()
    ;
}

public class Test : IActivity
{
    public static void Build(IActivityBuilder builder) => builder
        .UseActivity<BaseTest>(b => b
            .UseAcceptEventAction<BaseGetData, X>(b => b
                .ChangeAcceptedEvent<GetData>()
            
                .AddFlow<int>("x")
            )
            .AddAction("x", async c => c.Behavior.Publish(new List<Dictionary<string, DataNotification>>() { new() { { "x", new() { Data = $"Lorem ipsum #{DateTime.Now}" } } } }))
        );
}