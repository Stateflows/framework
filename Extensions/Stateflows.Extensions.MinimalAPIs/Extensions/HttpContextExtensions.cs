using System.Diagnostics;
using System.Text;
using Microsoft.AspNetCore.Http;
using Stateflows.Common;
using Stateflows.Common.Utilities;

namespace Stateflows.Extensions.MinimalAPIs;

public static class HttpContextExtensions
{
    public static async Task WriteEventAsync(this HttpContext httpContext, EventHolder eventHolder)
    {
        var behaviorId = eventHolder.SenderId;
        try
        {
            var jsonString = StateflowsJsonConverter.SerializeObject(eventHolder, true);
        
            await httpContext.Response.WriteAsync($"event: {eventHolder.Name}\n");
            await httpContext.Response.WriteAsync($"data: ");
            await httpContext.Response.Body.WriteAsync(Encoding.UTF8.GetBytes(jsonString));
            await httpContext.Response.WriteAsync($"\n\n");
            await httpContext.Response.Body.FlushAsync();
        }
        catch (Exception e)
        {
            Trace.WriteLine(behaviorId is not null
                ? $"⦗→s⦘ Notification {eventHolder.Name} published by {behaviorId.Value.Type} '{behaviorId.Value.Name}:{behaviorId.Value.Instance}' failed to serialize: {e}"
                : $"⦗→s⦘ Notification {eventHolder.Name} failed to serialize: {e}"
            );
        }
    }
}