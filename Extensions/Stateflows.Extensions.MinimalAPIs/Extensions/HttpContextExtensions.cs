using System.Text;
using Microsoft.AspNetCore.Http;
using Stateflows.Common;
using Stateflows.Common.Utilities;

namespace Stateflows.Extensions.MinimalAPIs;

public static class HttpContextExtensions
{
    public static async Task WriteEventAsync(this HttpContext httpContext, EventHolder eventHolder)
    {
        await httpContext.Response.WriteAsync($"event: {eventHolder.Name}\n");
        await httpContext.Response.WriteAsync($"data: ");
        await httpContext.Response.Body.WriteAsync(
            Encoding.UTF8.GetBytes(StateflowsJsonConverter.SerializeObject(eventHolder))
        );
        await httpContext.Response.WriteAsync($"\n\n");
        await httpContext.Response.Body.FlushAsync();
    }
}