using System;

namespace Stateflows.Common
{
    public abstract class RecurringMomentEvent : RecurringEvent
    {
        protected abstract DateTime Moment { get; }

        public override sealed bool ShouldTrigger(DateTime startedAt)
        {
            var now = DateTime.Now;
            var moment = Moment;
            var afterMoment = moment.AddMinutes(1).AddSeconds(30);
            return (now > moment) && (now < afterMoment);
        }
    }
}
