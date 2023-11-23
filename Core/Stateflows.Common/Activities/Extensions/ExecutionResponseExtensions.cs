using System.Linq;
using System.Collections.Generic;
using Stateflows.Activities.Events;

namespace Stateflows.Activities
{
    public static class ExecutionResponseExtensions
    {
        public static IEnumerable<T> GetOutputValues<T>(this ExecutionResponse response)
            => response != null
                ? response.OutputTokens.OfType<ValueToken<T>>().Select(t => t.Value).ToArray()
                : new T[0];

        public static T GetOutputValueOrDefault<T>(this ExecutionResponse response, T defaultValue = default)
        {
            var valueTokens = response.GetOutputValues<T>();

            return valueTokens.Any()
                ? valueTokens.First()
                : defaultValue;
        }
    }
}
