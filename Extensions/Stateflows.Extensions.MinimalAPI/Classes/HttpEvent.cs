using Microsoft.AspNetCore.Http;

namespace Stateflows.Common
{
    public class HttpEvent<TPayload> : Event<TPayload>
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

    public class HttpEvent : Event
    {
        internal static string? UrlOverride = null;
        internal static string? MethodOverride = null;
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
