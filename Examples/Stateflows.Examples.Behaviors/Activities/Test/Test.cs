using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Stateflows.Activities;
using Stateflows.Activities.Registration.Interfaces;
using Stateflows.Common;
using Stateflows.Common.Utilities;
using Stateflows.Examples.Common.Events;
using Stateflows.Extensions.MinimalAPIs;
using Stateflows.Extensions.MinimalAPIs.Headers;
using Stateflows.StateMachines;

namespace Stateflows.Examples.Behaviors.Activities.Test;

internal static class ObjectFlowBuilderExtensions
{
    public static IObjectFlowBuilder<BaseGetData> AddDataScopeGuard(this IObjectFlowBuilder<BaseGetData> builder, string scopeName)
        => builder.AddGuard(async c =>
            !c.Token.Scope.Any() ||
            c.Token.Scope.Contains(scopeName, StringComparer.OrdinalIgnoreCase)
        );
}

public class Test : IActivity
{
    public static void Build(IActivityBuilder builder) => builder
        .AddAcceptEventAction<GetData>(async c => c.Output(c.Event), b => b
            .AddFlow<GetData>("getDataAction", b => b
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
                    await Task.Delay(1000);
                    
                    c.Behavior.Publish(new List<Dictionary<string, DataNotification>>() { new() { { "x", new() { Data = "Lorem ipsum" } } } });
                }
            }
        )
    ;
}