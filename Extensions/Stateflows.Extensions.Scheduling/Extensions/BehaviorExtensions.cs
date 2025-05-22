using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Stateflows.Common;
using Stateflows.Common.Interfaces;
using Stateflows.Scheduler.StateMachine;
using Stateflows.Scheduler.StateMachine.Events;
using Stateflows.StateMachines;

namespace Stateflows
{
    public static class BehaviorExtensions
    {
        public static Task<(bool Success, Guid ScheduleId)> ScheduleTimeAsync<TEvent>(this IBehavior behavior, DateTime time, TEvent @event, IEnumerable<EventHeader>? headers = null)
            => ScheduleAsync(behavior, new ScheduleTime()
            {
                Time = time,
                Events = new List<EventHolder>() { @event.ToEventHolder(headers) },
                Recipients = new List<BehaviorId>() { behavior.Id }
            });
        
        public static Task<(bool Success, Guid ScheduleId)> ScheduleDelayAsync<TEvent>(this IBehavior behavior, TimeSpan delay, TEvent @event, IEnumerable<EventHeader>? headers = null)
            => ScheduleAsync(behavior, new ScheduleDelay()
            {
                Delay = delay,
                Events = new List<EventHolder>() { @event.ToEventHolder(headers) },
                Recipients = new List<BehaviorId>() { behavior.Id }
            });

        public static Task<(bool Success, Guid ScheduleId)> ScheduleIntervalAsync<TEvent>(this IBehavior behavior, TimeSpan interval, TEvent @event, IEnumerable<EventHeader>? headers = null)
            => ScheduleAsync(behavior, new ScheduleInterval()
            {
                Interval = interval, 
                Events = new List<EventHolder>() { @event.ToEventHolder(headers) },
                Recipients = new List<BehaviorId>() { behavior.Id }
            });

        public static Task<(bool Success, Guid ScheduleId)> ScheduleCronAsync<TEvent>(this IBehavior behavior, string cronExpression, TEvent @event, IEnumerable<EventHeader>? headers = null)
            => ScheduleAsync(behavior, new ScheduleCron()
            {
                CronExpressions = new List<string>() { cronExpression }, 
                Events = new List<EventHolder>() { @event.ToEventHolder(headers) },
                Recipients = new List<BehaviorId>() { behavior.Id }
            });

        public static async Task UnscheduleAsync(this IBehavior behavior, Guid scheduleId)
        {
            if (!(behavior is IInjectionScope injectionScope)) throw new NotSupportedException($"{behavior.GetType()} implementation of IBehavior does not support events scheduling");
            
            var behaviorId = new StateMachineId(StateMachine<StateflowsScheduler>.Name, string.Empty);
            var locator = injectionScope.ServiceProvider.GetRequiredService<IBehaviorLocator>();
            if (locator.TryLocateBehavior(behaviorId, out var schedulerBehavior))
            {
                await schedulerBehavior.SendAsync(new Unschedule() { Ids = new List<Guid>() { scheduleId } });
            }
        } 
        
        private static async Task<(bool Success, Guid ScheduleId)> ScheduleAsync(this IBehavior behavior, ScheduleBase @event)
        {
            if (!(behavior is IInjectionScope injectionScope)) throw new NotSupportedException($"{behavior.GetType()} implementation of IBehavior does not support events scheduling");
            
            var behaviorId = new StateMachineId(StateMachine<StateflowsScheduler>.Name, string.Empty);
            var locator = injectionScope.ServiceProvider.GetRequiredService<IBehaviorLocator>();
            if (locator.TryLocateBehavior(behaviorId, out var schedulerBehavior))
            {
                var result = await schedulerBehavior.RequestAsync(@event);
                return (
                    result is { Status: EventStatus.Consumed, Response: { } },
                    result?.Response?.Id ?? Guid.Empty
                );
            }

            return (false, Guid.Empty);
        }
    }
}