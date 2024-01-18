using System.Linq;
using System.Collections.Generic;
using Stateflows.Common;
using Stateflows.Common.Data;
using Stateflows.Activities.Context.Interfaces;

namespace Stateflows.Activities
{
    public static class IOutputExtensions
    {
        public static void Output<TTokenPayload>(this IOutput output, TTokenPayload payload)
            => output.Output(payload.ToToken());

        public static void OutputRange<TTokenPayload>(this IOutput output, IEnumerable<TTokenPayload> payloads)
            => output.Output(payloads.Select(payload => payload.ToToken()).ToArray());

        public static void OutputRangeAsGroup<TTokenPayload>(this IOutput output, IEnumerable<TTokenPayload> payloads)
            => output.OutputRangeAsGroup(payloads.Select(payload => payload.ToToken()).ToArray());

        public static void PassTokensOfTypeOn<TTokenPayload>(this IOutput output)
            => output.PassTokensOfTypeOn<Token<TTokenPayload>>();
    }
}
