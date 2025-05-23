using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using OneOf;
using Stateflows.Common;
using Stateflows.Common.Attributes;
using Stateflows.Scheduler.Classes;
using Stateflows.Scheduler.StateMachine.Events;
using Stateflows.StateMachines;

namespace Stateflows.Scheduler.StateMachine.Effects
{
    public class ScheduleEffect : ITransitionEffect<OneOf<ScheduleCron, ScheduleDelay, ScheduleInterval, ScheduleTime>>
    {
        private readonly IValue<List<Entry>> Schedules;
        public ScheduleEffect([GlobalValue(required: false)] IValue<List<Entry>> schedules)
        {
            Schedules = schedules;
        }

        public Task EffectAsync(OneOf<ScheduleCron, ScheduleDelay, ScheduleInterval, ScheduleTime> @event)
        {
            var scheduleEvent = (ScheduleBase)@event.Value;
            return Schedules.UpdateAsync(
                schedules =>
                {
                    schedules.Add(new Entry()
                    {
                        Id = Guid.NewGuid(),
                        CreatedAt = DateTime.Now,
                        Events = scheduleEvent.Events,
                        Recipients = scheduleEvent.Recipients,
                        Rules = new List<Rule>()
                        {
                            scheduleEvent switch
                            {
                                ScheduleTime scheduleTime => new Rule()
                                {
                                    Kind = RuleKind.Time,
                                    Time = scheduleTime.Time
                                },

                                ScheduleDelay scheduleDelay => new Rule()
                                {
                                    Kind = RuleKind.Delay,
                                    Delay = scheduleDelay.Delay
                                },

                                ScheduleInterval scheduleInterval => new Rule()
                                {
                                    Kind = RuleKind.Interval,
                                    Interval = scheduleInterval.Interval
                                },

                                ScheduleCron scheduleCron => new Rule()
                                {
                                    Kind = RuleKind.Cron,
                                    CronExpressions = scheduleCron.CronExpressions
                                },

                                _ => throw new NotSupportedException(
                                    $"{Event.GetName(@event.GetType())} is not supported.")
                            }
                        }
                    });
                    return schedules;
                },
                new List<Entry>()
            );
        }
    }
}