using System;
using System.Linq;
using System.Collections.Generic;

namespace Stateflows.Common.Trace.Models
{
    public class BehaviorTraceStep
    {
        public BehaviorTraceStep AddStep(string actionName, string elementName, TimeSpan duration, object customData = null)
        {
            var step = new BehaviorTraceStep()
            {
                ActionName = actionName,
                ElementName = elementName,
                ExecutedAt = DateTime.Now,
                Duration = duration,
                CustomData = customData
            };

            Steps.Add(step);
            return step;
        }

        public bool ShouldSerializeSteps()
            => Steps != null && Steps.Any();

        public List<BehaviorTraceStep> Steps { get; set; } = new List<BehaviorTraceStep>();

        public DateTime ExecutedAt { get; set; }

        public TimeSpan Duration { get; set; }

        public string ActionName { get; set; }

        public string ElementName { get; set; }

        public bool ShouldSerializeExceptions()
            => Exceptions != null && Exceptions.Any();

        public List<Exception> Exceptions { get; set; } = new List<Exception>();

        public bool ShouldSerializeCustomData()
            => CustomData != null;

        public object CustomData { get; set; }


    }
}
