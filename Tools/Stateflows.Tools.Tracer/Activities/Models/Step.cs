using System;
using System.Linq;
using System.Collections.Generic;
using Newtonsoft.Json;
using Stateflows.Common;

namespace Stateflows.Tools.Tracer.Activities.Models
{
    internal class Step
    {
        public Step AddStep(string action, string node, TimeSpan duration, IEnumerable<Token> inputTokens)
        {
            var step = new Step()
            {
                Action = action,
                Node = node,
                ExecutedAt = DateTime.Now,
                Duration = duration,
                Input = inputTokens
            };

            Steps.Add(step);
            return step;
        }

        public bool ShouldSerializeSteps()
            => Steps != null && Steps.Any();

        public List<Step> Steps { get; set; } = new List<Step>();

        public DateTime ExecutedAt { get; set; }

        public TimeSpan Duration { get; set; }

        public bool ShouldSerializeInput()
            => Input != null && Input.Any();

        public IEnumerable<Token>? Input { get; set; }

        public string? Action { get; set; }

        public string? Node { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public Exception? Exception { get; set; }
    }
}
