using System.Collections.Generic;
using System.Threading.Tasks;
using Stateflows.Common;
using Stateflows.Common.Attributes;
using Stateflows.Scheduler.Classes;
using Stateflows.Scheduler.StateMachine.Events;
using Stateflows.StateMachines;

namespace Stateflows.Scheduler.StateMachine.Effects
{
    /// <summary>Removes stored schedule entries whose identifiers are requested for cancellation.</summary>
    public class UnscheduleEffect : ITransitionEffect<Unschedule>
    {
        private readonly IValue<List<Entry>> Schedules;

        /// <summary>Creates an effect that updates the scheduler's global schedule value.</summary>
        /// <param name="schedules">The optional global value containing persisted schedule entries.</param>
        public UnscheduleEffect([GlobalValue(required: false)] IValue<List<Entry>> schedules)
        {
            Schedules = schedules;
        }
        
        /// <summary>Removes every stored entry whose identifier appears in the cancellation event.</summary>
        /// <param name="event">The event containing schedule identifiers to remove.</param>
        /// <returns>A task that completes when the updated schedule list has been stored.</returns>
        public Task EffectAsync(Unschedule @event)
        {
            return Schedules.UpdateAsync(
                schedules =>
                {
                    schedules.RemoveAll(schedule => @event.Ids.Contains(schedule.Id));

                    return schedules;
                },
                new List<Entry>()
            );
        }
    }
}