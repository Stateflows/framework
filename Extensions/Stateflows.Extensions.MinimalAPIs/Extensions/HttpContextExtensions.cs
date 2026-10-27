using System.Diagnostics;
using System.Text;
using Microsoft.AspNetCore.Http;
using Stateflows.Common;
using Stateflows.Common.Utilities;

namespace Stateflows.Extensions.MinimalAPIs;

/// <summary>Provides HTTP response helpers for behavior notification streams.</summary>
public static class HttpContextExtensions
{
    /// <summary>Writes and flushes a notification as a UTF-8 server-sent event.</summary>
    /// <param name="httpContext">HTTP context whose response receives the event.</param>
    /// <param name="eventHolder">Notification envelope, including its event name and payload.</param>
    /// <returns>A task completing after the write attempt.</returns>
    /// <remarks>Serialization and write failures are reported to diagnostic tracing rather than rethrown.</remarks>
    public static async Task WriteEventAsync(this HttpContext httpContext, EventHolder eventHolder)
    {
        var behaviorId = eventHolder.SenderId;
        try
        {
            var jsonString = StructuralJson.SerializeObject(eventHolder);
        
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