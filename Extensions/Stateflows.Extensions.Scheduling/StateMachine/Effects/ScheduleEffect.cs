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
    /// <summary>Stores a schedule requested by a scheduling event and responds with its generated identifier.</summary>
    public class ScheduleEffect : ITransitionEffect<OneOf<ScheduleCron, ScheduleDelay, ScheduleInterval, ScheduleTime>>
    {
        private readonly IValue<List<Entry>> Schedules;

        /// <summary>Creates an effect that records schedule entries in the scheduler's global value.</summary>
        /// <param name="schedules">The optional global value containing persisted schedule entries.</param>
        public ScheduleEffect([GlobalValue(required: false)] IValue<List<Entry>> schedules)
        {
            Schedules = schedules;
        }

        /// <summary>Adds the requested schedule and returns its new identifier through the event response.</summary>
        /// <param name="event">The cron, delay, interval, or absolute-time scheduling request.</param>
        /// <returns>A task that completes after the entry is stored and its identifier is assigned to the response.</returns>
        public async Task EffectAsync(OneOf<ScheduleCron, ScheduleDelay, ScheduleInterval, ScheduleTime> @event)
        {
            var scheduleEvent = (ScheduleBase)@event.Value;
            var scheduleId = Guid.NewGuid();
            await Schedules.UpdateAsync(
                schedules =>
                {
                    schedules.Add(new Entry()
                    {
                        Id = scheduleId,
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

            scheduleEvent.Respond(new ScheduleResponse() { Id = scheduleId });
        }
    }
}