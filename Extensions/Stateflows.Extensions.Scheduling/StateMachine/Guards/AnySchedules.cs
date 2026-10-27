using System.Collections.Generic;
using System.Threading.Tasks;
using Stateflows.Common;
using Stateflows.Common.Attributes;
using Stateflows.Scheduler.Classes;
using Stateflows.StateMachines;

namespace Stateflows.Scheduler.StateMachine.Guards
{
    /// <summary>Guards a transition on whether at least one schedule is currently stored.</summary>
    public class AnySchedules : ITransitionGuard
    {
        private readonly IValue<List<Entry>> Schedules;

        /// <summary>Creates a guard that reads the scheduler's global schedule value.</summary>
        /// <param name="schedules">The optional global value containing persisted schedule entries.</param>
        public AnySchedules([GlobalValue(required: false)] IValue<List<Entry>> schedules)
        {
            Schedules = schedules;
        }
        
        /// <summary>Checks whether the schedule value exists and contains at least one entry.</summary>
        /// <returns><see langword="true"/> when at least one schedule is stored; otherwise, <see langword="false"/>.</returns>
        public async Task<bool> GuardAsync()
        {
            var (result, schedules) = await Schedules.TryGetAsync();
            
            return result && schedules.Count > 0;
        }
    }
}