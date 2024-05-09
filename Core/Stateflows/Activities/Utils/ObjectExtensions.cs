using Stateflows.Activities;

namespace Stateflows.Utils
{
    internal static class ObjectExtensions
    {
        internal static Token<T> ToToken<T>(this T payload)
           => new Token<T>() { Payload = payload };
    }
}
