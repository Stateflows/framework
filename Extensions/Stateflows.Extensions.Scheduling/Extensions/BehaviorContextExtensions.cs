using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Stateflows.Common;
using Stateflows.Common.Utilities;
using Stateflows.StateMachines;
using Stateflows.Scheduler.StateMachine;
using Stateflows.Scheduler.StateMachine.Events;

namespace Stateflows
{
    public static class BehaviorContextExtensions
    {
        public static Task<(bool Success, Guid ScheduleId)> ScheduleTimeAsync<TEvent>(this IBehaviorContext context, DateTime time, TEvent @event, IEnumerable<EventHeader>? headers = null)
            => ScheduleAsync(context, new ScheduleTime()
            {
                Time = time,
                Events = new List<EventHolder>() { @event.ToEventHolder(headers) },
                Recipients = new List<BehaviorId>() { context.Id }
            });
        
        public static Task<(bool Success, Guid ScheduleId)> ScheduleDelayAsync<TEvent>(this IBehaviorContext context, TimeSpan delay, TEvent @event, IEnumerable<EventHeader>? headers = null)
            => ScheduleAsync(context, new ScheduleDelay()
            {
                Delay = delay,
                Events = new List<EventHolder>() { @event.ToEventHolder(headers) },
                Recipients = new List<BehaviorId>() { context.Id }
            });

        public static Task<(bool Success, Guid ScheduleId)> ScheduleIntervalAsync<TEvent>(this IBehaviorContext context, TimeSpan interval, TEvent @event, IEnumerable<EventHeader>? headers = null)
            => ScheduleAsync(context, new ScheduleInterval()
            {
                Interval = interval, 
                Events = new List<EventHolder>() { @event.ToEventHolder(headers) },
                Recipients = new List<BehaviorId>() { context.Id }
            });

        public static Task<(bool Success, Guid ScheduleId)> ScheduleCronAsync<TEvent>(this IBehaviorContext context, string cronExpression, TEvent @event, IEnumerable<EventHeader>? headers = null)
            => ScheduleAsync(context, new ScheduleCron()
            {
                CronExpressions = new List<string>() { cronExpression }, 
                Events = new List<EventHolder>() { @event.ToEventHolder(headers) },
                Recipients = new List<BehaviorId>() { context.Id }
            });

        public static async Task UnscheduleAsync(this IBehaviorContext context, Guid scheduleId)
        {
            var locator = context.ServiceProvider.GetRequiredService<IBehaviorLocator>();
            var behaviorId = new StateMachineId(StateMachine<StateflowsScheduler>.Name, string.Empty);
            if (locator.TryLocateBehavior(behaviorId, out var behavior))
            {
                await behavior.SendAsync(new Unschedule() { Ids = new List<Guid>() { scheduleId } });
            }
        } 
        
        private static async Task<(bool Success, Guid ScheduleId)> ScheduleAsync(IBehaviorContext context, ScheduleBase @event)
        {
            var locator = context.ServiceProvider.GetRequiredService<IBehaviorLocator>();
            var behaviorId = new StateMachineId(StateMachine<StateflowsScheduler>.Name, string.Empty);
            if (!locator.TryLocateBehavior(behaviorId, out var behavior))
            {
                return (false, Guid.Empty);
            }
            
            var result = await behavior.RequestAsync(@event);
            return (
                result is { Status: EventStatus.Consumed, Response: { } },
                result?.Response?.Id ?? Guid.Empty
            );
        }
    }
}