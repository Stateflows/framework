using System.Collections.Generic;
using System.Threading.Tasks;
using Stateflows.Common;
using Stateflows.Common.Attributes;
using Stateflows.Scheduler.Classes;
using Stateflows.Scheduler.StateMachine.Events;
using Stateflows.StateMachines;

namespace Stateflows.Scheduler.StateMachine.Effects
{
    public class UnscheduleEffect : ITransitionEffect<Unschedule>
    {
        private readonly IValue<List<Entry>> Schedules;
        public UnscheduleEffect([GlobalValue(required: false)] IValue<List<Entry>> schedules)
        {
            Schedules = schedules;
        }
        
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