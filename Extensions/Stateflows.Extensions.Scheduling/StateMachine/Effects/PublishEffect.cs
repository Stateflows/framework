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
    /// <summary>Publishes the events of schedules whose trigger time has arrived.</summary>
    /// <remarks>Removes schedules that expired before the last check and sends each due event to its configured recipients.</remarks>
    public class PublishEffect : ITransitionEffect
    {
        private readonly IValue<List<Entry>> Schedules;
        private readonly IValue<DateTime> LastCheck;
        private readonly IBehaviorLocator BehaviorLocator;
        
        /// <summary>Creates the scheduler's due-event publication effect.</summary>
        /// <param name="schedules">The optional global value containing persisted schedule entries.</param>
        /// <param name="lastCheck">The optional global value recording the previous publication check.</param>
        /// <param name="behaviorLocator">The locator used to resolve each scheduled recipient.</param>
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
        
        /// <summary>Removes expired schedules and sends events for schedules that are due.</summary>
        /// <returns>A task that completes after due events have been submitted to their recipients.</returns>
        public async Task EffectAsync()
        {
            var now = DateTime.Now;
            var lastCheck = await LastCheck.GetOrDefaultAsync(DateTime.MinValue);
            await LastCheck.SetAsync(now);

            await Schedules.UpdateAsync(s =>
            {
                s.RemoveAll(s => s.GetTriggerTime(lastCheck) < lastCheck);
                return s;
            }, []);

            var allSchedules = await Schedules.GetOrDefaultAsync(new List<Entry>());
            var schedules = allSchedules
                .Select(s => (Schedule: s, TriggerTime: s.GetTriggerTime(lastCheck)))
                .Where(g => now >= g.TriggerTime)
                .SelectMany(s => s.Schedule.Recipients
                    .Select(r => (Recipient: r, Schedule: s.Schedule))
                )
                .ToList();

            var schedulesByRecipient = schedules
                .GroupBy(s => s.Recipient)
                .ToDictionary(
                    t => t.Key,
                    t => t.Select(s => s.Schedule).ToArray()
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