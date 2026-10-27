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
    /// <summary>Schedules events for delivery to the behavior associated with an execution context.</summary>
    public static class BehaviorContextExtensions
    {
        /// <summary>Schedules an event for delivery at a specified date and time to the current behavior.</summary>
        /// <typeparam name="TEvent">The event type to publish.</typeparam>
        /// <param name="context">The current behavior context.</param>
        /// <param name="time">The scheduled delivery time.</param>
        /// <param name="event">The event instance to deliver.</param>
        /// <param name="headers">Optional headers to attach to the scheduled event.</param>
        /// <returns>A task containing whether the schedule was accepted and its identifier; a rejected request returns an empty identifier.</returns>
        public static Task<(bool Success, Guid ScheduleId)> ScheduleTimeAsync<TEvent>(this IBehaviorContext context, DateTime time, TEvent @event, IDictionary<string, EventHeader>? headers = null)
            => ScheduleAsync(context, new ScheduleTime()
            {
                Time = time,
                Events = new List<EventHolder>() { @event.ToEventHolder(headers) },
                Recipients = new List<BehaviorId>() { context.Id }
            });
        
        /// <summary>Schedules an event for delivery after a delay to the current behavior.</summary>
        /// <typeparam name="TEvent">The event type to publish.</typeparam>
        /// <param name="context">The current behavior context.</param>
        /// <param name="delay">The delay before delivery.</param>
        /// <param name="event">The event instance to deliver.</param>
        /// <param name="headers">Optional headers to attach to the scheduled event.</param>
        /// <returns>A task containing whether the schedule was accepted and its identifier; a rejected request returns an empty identifier.</returns>
        public static Task<(bool Success, Guid ScheduleId)> ScheduleDelayAsync<TEvent>(this IBehaviorContext context, TimeSpan delay, TEvent @event, IDictionary<string, EventHeader>? headers = null)
            => ScheduleAsync(context, new ScheduleDelay()
            {
                Delay = delay,
                Events = new List<EventHolder>() { @event.ToEventHolder(headers) },
                Recipients = new List<BehaviorId>() { context.Id }
            });

        /// <summary>Schedules repeated delivery of an event at a fixed interval to the current behavior.</summary>
        /// <typeparam name="TEvent">The event type to publish.</typeparam>
        /// <param name="context">The current behavior context.</param>
        /// <param name="interval">The interval between deliveries.</param>
        /// <param name="event">The event instance to deliver on each occurrence.</param>
        /// <param name="headers">Optional headers to attach to each scheduled event.</param>
        /// <returns>A task containing whether the schedule was accepted and its identifier; a rejected request returns an empty identifier.</returns>
        public static Task<(bool Success, Guid ScheduleId)> ScheduleIntervalAsync<TEvent>(this IBehaviorContext context, TimeSpan interval, TEvent @event, IDictionary<string, EventHeader>? headers = null)
            => ScheduleAsync(context, new ScheduleInterval()
            {
                Interval = interval, 
                Events = new List<EventHolder>() { @event.ToEventHolder(headers) },
                Recipients = new List<BehaviorId>() { context.Id }
            });

        /// <summary>Schedules repeated delivery of an event using a cron expression to the current behavior.</summary>
        /// <typeparam name="TEvent">The event type to publish.</typeparam>
        /// <param name="context">The current behavior context.</param>
        /// <param name="cronExpression">The cron schedule expression.</param>
        /// <param name="event">The event instance to deliver on each occurrence.</param>
        /// <param name="headers">Optional headers to attach to each scheduled event.</param>
        /// <returns>A task containing whether the schedule was accepted and its identifier; a rejected request returns an empty identifier.</returns>
        public static Task<(bool Success, Guid ScheduleId)> ScheduleCronAsync<TEvent>(this IBehaviorContext context, string cronExpression, TEvent @event, IDictionary<string, EventHeader>? headers = null)
            => ScheduleAsync(context, new ScheduleCron()
            {
                CronExpressions = new List<string>() { cronExpression }, 
                Events = new List<EventHolder>() { @event.ToEventHolder(headers) },
                Recipients = new List<BehaviorId>() { context.Id }
            });

        /// <summary>Removes a previously accepted schedule.</summary>
        /// <param name="context">The current behavior context.</param>
        /// <param name="scheduleId">The identifier returned by a scheduling operation.</param>
        /// <returns>A task that completes when the unschedule request has been sent.</returns>
        /// <remarks>If the scheduler behavior cannot be located, this method completes without removing a schedule.</remarks>
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