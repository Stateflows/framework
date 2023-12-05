using System;

namespace Stateflows.Common
{
    public abstract class SpecificMomentEvent : TimeEvent
    {
        protected abstract DateTime Moment { get; }

        public override sealed bool ShouldTrigger(DateTime startedAt)
        {
            var now = DateTime.Now;
            var moment = Moment;
            var afterMoment = moment.AddMinutes(1).AddSeconds(30);
            return (now > moment) && (now < afterMoment);
        }

        protected override DateTime GetTriggerTime(DateTime startedAt)
            => Moment;
    }
}
