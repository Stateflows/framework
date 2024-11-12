using System.Threading;
using System.Collections.Generic;

namespace Stateflows.Common
{
    public static class ResponseHolder
    {
        private readonly static AsyncLocal<Dictionary<object, EventHolder>> Responses =
            new AsyncLocal<Dictionary<object, EventHolder>>();

        public static bool ResponsesAreSet()
            => Responses.Value != null;

        public static void SetResponses(Dictionary<object, EventHolder> responses)
            => Responses.Value = responses;

        public static void CopyResponses(Dictionary<object, EventHolder> responses)
        {
            foreach (var key in responses.Keys)
            {
                Responses.Value[key] = responses[key];
            }
        }

        public static void ClearResponses()
            => Responses.Value = null;

        public static void Respond(object request, EventHolder response)
            => Responses.Value[request] = response;

        public static bool IsResponded(object request)
            => Responses.Value.ContainsKey(request);

        public static EventHolder GetResponseOrDefault(object request)
            => Responses.Value.GetValueOrDefault(request);
    }

    public interface IRequest<in TResponse>
    { }
}
