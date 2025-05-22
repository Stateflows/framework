using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Stateflows.Common;
using Stateflows.Common.Attributes;
using Stateflows.Scheduler.Classes;
using Stateflows.StateMachines;

namespace Stateflows.Scheduler.StateMachine.Effects
{
    public class PublishEffect : ITransitionEffect
    {
        private readonly IValue<List<Entry>> Schedules;
        private readonly IValue<DateTime> LastCheck;
        private readonly IBehaviorLocator BehaviorLocator;
        
        public PublishEffect(
            [GlobalValue(required: false)] IValue<List<Entry>> schedules,
            [GlobalValue(required: false)] IValue<DateTime> lastCheck,
            IBehaviorLocator behaviorLocator
        )
        {
            Schedules = schedules;
            LastCheck = lastCheck;
            BehaviorLocator = behaviorLocator;
        }
        
        public async Task EffectAsync()
        {
            var now = DateTime.Now;
            var lastCheck = await LastCheck.GetOrDefaultAsync(DateTime.MinValue);
            await LastCheck.SetAsync(now);

            await Schedules.UpdateAsync(s =>
            {
                s.RemoveAll(s => s.GetTriggerTime(lastCheck) < lastCheck);
                return s;
            });

            var allSchedules = await Schedules.GetOrDefaultAsync(new List<Entry>());
            var schedules = allSchedules
                .Select(s => (Schedule: s, TriggerTime: s.GetTriggerTime(lastCheck)))
                .Where(g => now >= g.TriggerTime)
                .SelectMany(s => s.Schedule.Recipients
                    .Select(r => (Recipient: r, Schedule: s.Schedule))
                )
                .ToList();

            var schedulesByRecipient = schedules
                .ToDictionary(
                    t => t.Recipient,
                    t => schedules
                        .Where(s => s.Recipient == t.Recipient)
                        .Select(s => s.Schedule)
                        .ToArray()
                );

            foreach (var (behaviorId, recipientSchedules) in schedulesByRecipient)
            {
                if (!BehaviorLocator.TryLocateBehavior(behaviorId, out var behavior)) continue;

                var events = recipientSchedules
                    .SelectMany(s => s.Events)
                    .ToArray();
                
                foreach (var eventHolder in events)
                {
                    _ = behavior.SendAsync(eventHolder.BoxedPayload, eventHolder.Headers);
                }
            }
        }
    }
}