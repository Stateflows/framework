using System.Collections.Generic;
using System.Threading.Tasks;
using Stateflows.Common;
using Stateflows.Common.Attributes;
using Stateflows.Scheduler.Classes;
using Stateflows.StateMachines;

namespace Stateflows.Scheduler.StateMachine.Guards
{
    public class AnySchedules : ITransitionGuard
    {
        private readonly IValue<List<Entry>> Schedules;
        public AnySchedules([GlobalValue(required: false)] IValue<List<Entry>> schedules)
        {
            Schedules = schedules;
        }
        
        public async Task<bool> GuardAsync()
        {
            var (result, schedules) = await Schedules.TryGetAsync();
            
            return result && schedules.Count > 0;
        }
    }
}