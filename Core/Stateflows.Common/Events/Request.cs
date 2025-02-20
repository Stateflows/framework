using System.Threading;
using System.Collections.Generic;

namespace Stateflows.Common
{
    public static class ResponseHolder
    {
        private static readonly AsyncLocal<Dictionary<object, EventHolder>> Responses =
            new AsyncLocal<Dictionary<object, EventHolder>>();

        public static bool ResponsesAreSet()
        {
            lock (Responses)
            {
                return Responses.Value != null;
            }
        }

        public static void SetResponses(Dictionary<object, EventHolder> responses)
        {
            lock (Responses)
            {
                Responses.Value = responses;
            }
        }

        public static void CopyResponses(Dictionary<object, EventHolder> responses)
        {
            lock (Responses)
            {
                foreach (var key in responses.Keys)
                {
                    Responses.Value[key] = responses[key];
                }
            }
        }

        public static void ClearResponses()
        {
            lock (Responses)
            {
                Responses.Value = null;
            }
        }

        public static void Respond(object request, EventHolder response)
        {
            lock (Responses)
            {
                Responses.Value[request] = response;
            }
        }

        public static bool IsResponded(object request)
        {
            lock (Responses)
            {
                return Responses.Value.ContainsKey(request);
            }
        }

        public static EventHolder GetResponseOrDefault(object request)
        {
            lock (Responses)
            {
                return Responses.Value.GetValueOrDefault(request);
            }
        }
    }

    public interface IRequest<in TResponse>
    { }
}
