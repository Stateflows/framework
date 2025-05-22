using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using NCrontab;
using Stateflows.Common;

namespace Stateflows.Events
{
    public class CronEventValidator : IStateflowsValidator
    {
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