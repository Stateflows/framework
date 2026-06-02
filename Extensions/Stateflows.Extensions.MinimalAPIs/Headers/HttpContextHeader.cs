using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using Stateflows.Common;

namespace Stateflows.Extensions.MinimalAPIs.Headers;

public class HttpContextHeader : EventHeader
{
    public HostString Host { get; set; }
    public string Protocol { get; set; }
    public string Scheme { get; set; }
    public string Method { get; set; }
    public PathString Path { get; set; }
    public PathString PathBase { get; set; }
    public string? Body { get; set; }
    public long? ContentLength { get; set; }
    public string? ContentType { get; set; }
    public bool HasFormContentType { get; set; }
    public IDictionary<string, StringValues> Headers { get; set; }
    public IDictionary<string, string> Cookies { get; set; }
    public IDictionary<string, StringValues> Form { get; set; }
    public IDictionary<string, StringValues> Query { get; set; }
    public IDictionary<string, object?> RouteValues { get; set; }

    private static string GetBodyString(HttpRequest request)
    {
        var bodyStream = new StreamReader(request.Body);
        var task = bodyStream.ReadToEndAsync();
        task.Wait();
        var bodyText = task.Result;
        return bodyText;
    }
    
    [JsonIgnore]
    public HttpContext Context
    {
        set
        {
            Host = value.Request.Host;
            Protocol = value.Request.Protocol;
            Scheme = value.Request.Scheme;
            Method = value.Request.Method;
            Path = value.Request.Path;
            PathBase = value.Request.PathBase;
            Body = value.Request.ContentLength > 0
                ? GetBodyString(value.Request)
                : null;
            ContentLength = value.Request.ContentLength;
            ContentType = value.Request.ContentType;
            HasFormContentType = value.Request.HasFormContentType;
            Headers = value.Request.Headers.ToDictionary();
            Cookies = value.Request.Cookies.ToDictionary();
            Form = value.Request.HasFormContentType
                ? value.Request.Form.ToDictionary()
                : [];
            Query = value.Request.Query.ToDictionary();
            RouteValues = value.Request.RouteValues.ToDictionary();
        }
    }
}