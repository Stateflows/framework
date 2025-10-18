using Microsoft.AspNetCore.Http;
using Stateflows.Common;

namespace Stateflows.Extensions.MinimalAPIs.Headers;

public class HttpContextHeader : EventHeader
{
    public HttpContext Context { get; set; }
}