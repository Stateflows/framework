using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using Stateflows.Common;

namespace Stateflows.Extensions.MinimalAPIs.Headers;

/// <summary>Carries a snapshot of HTTP request information with an event delivered through the HTTP API.</summary>
/// <remarks>The live HTTP context is not serialized. Captured headers, cookies, body, and form data
/// may contain sensitive information; account for this when persisting or logging event headers.</remarks>
public class HttpContextHeader : EventHeader
{
    /// <summary>Gets or sets the request host and optional port.</summary>
    public HostString Host { get; set; }
    /// <summary>Gets or sets the HTTP protocol version string.</summary>
    public string Protocol { get; set; }
    /// <summary>Gets or sets the request scheme.</summary>
    public string Scheme { get; set; }
    /// <summary>Gets or sets the HTTP request method.</summary>
    public string Method { get; set; }
    /// <summary>Gets or sets the request path.</summary>
    public PathString Path { get; set; }
    /// <summary>Gets or sets the request's base path.</summary>
    public PathString PathBase { get; set; }
    /// <summary>Gets or sets the captured request body, when a positive content length was supplied.</summary>
    public string? Body { get; set; }
    /// <summary>Gets or sets the declared content length in bytes.</summary>
    public long? ContentLength { get; set; }
    /// <summary>Gets or sets the request content type.</summary>
    public string? ContentType { get; set; }
    /// <summary>Gets or sets whether the request has a recognized form content type.</summary>
    public bool HasFormContentType { get; set; }
    /// <summary>Gets or sets the captured HTTP headers.</summary>
    public IDictionary<string, StringValues> Headers { get; set; }
    /// <summary>Gets or sets the captured request cookies.</summary>
    public IDictionary<string, string> Cookies { get; set; }
    /// <summary>Gets or sets the captured form values; empty for non-form requests.</summary>
    public IDictionary<string, StringValues> Form { get; set; }
    /// <summary>Gets or sets the captured query-string values.</summary>
    public IDictionary<string, StringValues> Query { get; set; }
    /// <summary>Gets or sets the matched endpoint's route values.</summary>
    public IDictionary<string, object?> RouteValues { get; set; }

    private static string GetBodyString(HttpRequest request)
    {
        var bodyStream = new StreamReader(request.Body);
        var task = bodyStream.ReadToEndAsync();
        task.Wait();
        var bodyText = task.Result;
        return bodyText;
    }
    
    /// <summary>Captures request properties from the supplied live HTTP context.</summary>
    /// <remarks>Reading a nonempty body consumes the request stream from its current position.
    /// The context itself is not retained or serialized.</remarks>
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