using System.Linq;
using System.Collections.Generic;
using Stateflows.Activities.Events;

namespace Stateflows.Activities
{
    public static class ExecutionResponseExtensions
    {
        public static IEnumerable<T> GetOutputTokensOfType<T>(this ExecutionResponse response)
            => response != null
                ? response.OutputTokens.OfType<TokenHolder<T>>().Select(t => t.Payload).ToArray()
                : new T[0];

        public static bool TryGetOutputTokenOfType<T>(this ExecutionResponse response, out T token)
        {
            var valueTokens = response.GetOutputTokensOfType<T>();

            token = valueTokens.FirstOrDefault();

            return valueTokens.Any();
        }
    }
}
