using Microsoft.AspNetCore.Http;

namespace Stateflows.Common
{
    public class HttpRequest<TResponsePayload> : Request<Response<TResponsePayload>>
    {
        public override string Name => $"{base.Name}[{Method} {Url}]";

        private string url = string.Empty;
        public string Url
        {
            get => HttpEvent.UrlOverride ?? url;
            set => url = value;
        }

        private string method = string.Empty;
        public string Method
        {
            get => HttpEvent.MethodOverride ?? method;
            set => method = value;
        }

        public HttpRequest Request { get; set; }
    }

    public class HttpRequest<TRequestPayload, TResponsePayload> : Request<TRequestPayload, TResponsePayload>
    {
        public override string Name => $"{base.Name}[{Method} {Url}]";

        private string url = string.Empty;
        public string Url
        {
            get => HttpEvent.UrlOverride ?? url;
            set => url = value;
        }

        private string method = string.Empty;
        public string Method
        {
            get => HttpEvent.MethodOverride ?? method;
            set => method = value;
        }

        public HttpRequest Request { get; set; }
    }
}
