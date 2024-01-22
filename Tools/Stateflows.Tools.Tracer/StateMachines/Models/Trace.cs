using System;
using System.Linq;
using System.Collections.Generic;

namespace Stateflows.Tools.Tracer.StateMachines.Models
{
    internal class Trace : Tracer.Classes.Trace
    {
        public Step AddStep(string action, string element, TimeSpan duration)
        {
            var step = new Step()
            {
                Action = action,
                Element = element,
                ExecutedAt = DateTime.Now,
                Duration = duration
            };

            Steps.Add(step);
            return step;
        }

        public bool ShouldSerializeSteps()
            => Steps != null && Steps.Any();

        public List<Step> Steps { get; set; } = new List<Step>();
    }
}
