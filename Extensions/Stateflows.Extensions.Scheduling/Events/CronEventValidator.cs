using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using NCrontab;
using Stateflows.Common;

namespace Stateflows.Events
{
    /// <summary>Validates cron expressions used by <see cref="CronEvent"/> event definitions.</summary>
    public class CronEventValidator : IStateflowsValidator
    {
        /// <summary>Validates an event when it derives from <see cref="CronEvent"/>.</summary>
        /// <typeparam name="TEvent">The declared event type.</typeparam>
        /// <param name="event">The event instance to validate.</param>
        /// <param name="validationResults">Receives a validation error when the cron expression cannot be parsed.</param>
        /// <returns>A task containing whether the event passes cron validation.</returns>
        public Task<bool> ValidateAsync<TEvent>(TEvent @event, List<ValidationResult> validationResults)
        {
            if (!(@event is CronEvent cronEvent))
            {
                return Task.FromResult(true);
            }

            try
            {
                _ = CrontabSchedule.Parse(cronEvent.CronExpression);
            }
            catch (Exception e)
            {
                validationResults.Add(new ValidationResult(e.Message, new[] { nameof(CronEvent.CronExpression) }));

                return Task.FromResult(false);
            }

            return Task.FromResult(true);
        }
    }
}