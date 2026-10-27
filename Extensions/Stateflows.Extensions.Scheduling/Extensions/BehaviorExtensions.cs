using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Stateflows.Common;
using Stateflows.Common.Interfaces;
using Stateflows.Common.Utilities;
using Stateflows.Scheduler.StateMachine;
using Stateflows.Scheduler.StateMachine.Events;
using Stateflows.StateMachines;

namespace Stateflows
{
    /// <summary>Schedules events for delivery to a behavior instance.</summary>
    public static class BehaviorExtensions
    {
        /// <summary>Schedules an event for delivery at a specified date and time.</summary>
        /// <typeparam name="TEvent">The event type to publish.</typeparam>
        /// <param name="behavior">The behavior that receives the event.</param>
        /// <param name="time">The scheduled delivery time.</param>
        /// <param name="event">The event instance to deliver.</param>
        /// <param name="headers">Optional headers to attach to the scheduled event.</param>
        /// <returns>A task containing whether the schedule was accepted and its identifier; a rejected request returns an empty identifier.</returns>
        public static Task<(bool Success, Guid ScheduleId)> ScheduleTimeAsync<TEvent>(this IBehavior behavior, DateTime time, TEvent @event, IDictionary<string, EventHeader>? headers = null)
            => ScheduleAsync(behavior, new ScheduleTime()
            {
                Time = time,
                Events = new List<EventHolder>() { @event.ToEventHolder(headers) },
                Recipients = new List<BehaviorId>() { behavior.Id }
            });
        
        /// <summary>Schedules an event for delivery after a delay.</summary>
        /// <typeparam name="TEvent">The event type to publish.</typeparam>
        /// <param name="behavior">The behavior that receives the event.</param>
        /// <param name="delay">The delay before delivery.</param>
        /// <param name="event">The event instance to deliver.</param>
        /// <param name="headers">Optional headers to attach to the scheduled event.</param>
        /// <returns>A task containing whether the schedule was accepted and its identifier; a rejected request returns an empty identifier.</returns>
        public static Task<(bool Success, Guid ScheduleId)> ScheduleDelayAsync<TEvent>(this IBehavior behavior, TimeSpan delay, TEvent @event, IDictionary<string, EventHeader>? headers = null)
            => ScheduleAsync(behavior, new ScheduleDelay()
            {
                Delay = delay,
                Events = new List<EventHolder>() { @event.ToEventHolder(headers) },
                Recipients = new List<BehaviorId>() { behavior.Id }
            });

        /// <summary>Schedules repeated delivery of an event at a fixed interval.</summary>
        /// <typeparam name="TEvent">The event type to publish.</typeparam>
        /// <param name="behavior">The behavior that receives the event.</param>
        /// <param name="interval">The interval between deliveries.</param>
        /// <param name="event">The event instance to deliver on each occurrence.</param>
        /// <param name="headers">Optional headers to attach to each scheduled event.</param>
        /// <returns>A task containing whether the schedule was accepted and its identifier; a rejected request returns an empty identifier.</returns>
        public static Task<(bool Success, Guid ScheduleId)> ScheduleIntervalAsync<TEvent>(this IBehavior behavior, TimeSpan interval, TEvent @event, IDictionary<string, EventHeader>? headers = null)
            => ScheduleAsync(behavior, new ScheduleInterval()
            {
                Interval = interval, 
                Events = new List<EventHolder>() { @event.ToEventHolder(headers) },
                Recipients = new List<BehaviorId>() { behavior.Id }
            });

        /// <summary>Schedules repeated delivery of an event using a cron expression.</summary>
        /// <typeparam name="TEvent">The event type to publish.</typeparam>
        /// <param name="behavior">The behavior that receives the event.</param>
        /// <param name="cronExpression">The cron schedule expression.</param>
        /// <param name="event">The event instance to deliver on each occurrence.</param>
        /// <param name="headers">Optional headers to attach to each scheduled event.</param>
        /// <returns>A task containing whether the schedule was accepted and its identifier; a rejected request returns an empty identifier.</returns>
        public static Task<(bool Success, Guid ScheduleId)> ScheduleCronAsync<TEvent>(this IBehavior behavior, string cronExpression, TEvent @event, IDictionary<string, EventHeader>? headers = null)
            => ScheduleAsync(behavior, new ScheduleCron()
            {
                CronExpressions = new List<string>() { cronExpression }, 
                Events = new List<EventHolder>() { @event.ToEventHolder(headers) },
                Recipients = new List<BehaviorId>() { behavior.Id }
            });

        /// <summary>Removes a previously accepted schedule.</summary>
        /// <param name="behavior">The behavior used to access the scheduler.</param>
        /// <param name="scheduleId">The identifier returned by a scheduling operation.</param>
        /// <returns>A task that completes when the unschedule request has been sent.</returns>
        /// <exception cref="NotSupportedException">The behavior implementation does not expose the injection scope required to locate the scheduler.</exception>
        /// <remarks>If the scheduler behavior cannot be located, this method completes without removing a schedule.</remarks>
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