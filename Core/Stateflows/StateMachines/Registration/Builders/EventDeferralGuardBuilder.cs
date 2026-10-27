using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Stateflows.Common.Models;
using Stateflows.StateMachines.Models;
using Stateflows.StateMachines.Context.Interfaces;
using Stateflows.StateMachines.Interfaces;
using Stateflows.StateMachines.Registration.Interfaces.Base;
using Stateflows.StateMachines.Registration.Interfaces.Internal;

namespace Stateflows.StateMachines.Registration.Builders
{
    internal class EventDeferralGuardBuilder<TEvent> :
        IEventDeferralGuardBuilder<TEvent>,
        IDeferralBuilder
    {
        
        private List<Func<IDeferralContext<TEvent>, Task<bool>>> GuardsList { get; } = [];

        public Type EventType => typeof(TEvent);
        public Vertex Vertex { get; private set; }
        public Logic<StateMachinePredicateAsync> Guards { get; private set; }

        public EventDeferralGuardBuilder(Vertex vertex, Logic<StateMachinePredicateAsync> guards)
        {
            Vertex = vertex;
            Guards = guards;
        }
        
        public Func<IDeferralContext<TEvent>, Task<bool>> GetAndGuard()
            => async c =>
            {
                var result = true;
                foreach (var guard in GuardsList)
                {
                    if (await guard(c)) continue;

                    result = false;
                    break;
                }

                return result;
            };
        
        public Func<IDeferralContext<TEvent>, Task<bool>> GetOrGuard()
            => async c =>
            {
                var result = false;
                foreach (var guard in GuardsList)
                {
                    if (!await guard(c)) continue;
                    
                    result = true;
                    break;
                }
                
                return result;
            };
        
        public IEventDeferralGuardBuilder<TEvent> AddGuards(params Func<IDeferralContext<TEvent>, Task<bool>>[] guardsAsync)
        {
            GuardsList.AddRange(guardsAsync);

            return this;
        }

        public IEventDeferralGuardBuilder<TEvent> AddAndExpression(Action<IEventDeferralGuardBuilder<TEvent>> guardExpression)
        {
            var builder = new EventDeferralGuardBuilder<TEvent>(Vertex, Guards);
            guardExpression.Invoke(builder);
            
            GuardsList.Add(builder.GetAndGuard());

            return this;
        }

        public IEventDeferralGuardBuilder<TEvent> AddOrExpression(Action<IEventDeferralGuardBuilder<TEvent>> guardExpression)
        {
            var builder = new EventDeferralGuardBuilder<TEvent>(Vertex, Guards);
            guardExpression.Invoke(builder);
            
            GuardsList.Add(builder.GetOrGuard());

            return this;
        }
    }
}