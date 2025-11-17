using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Stateflows.StateMachines.Models;
using Stateflows.StateMachines.Context.Interfaces;
using Stateflows.StateMachines.Registration.Interfaces.Base;
using Stateflows.StateMachines.Registration.Interfaces.Internal;

namespace Stateflows.StateMachines.Registration.Builders
{
    internal class DeferralGuardBuilder<TEvent> :
        IDeferralGuardBuilder<TEvent>,
        IVertexBuilder
    {
        private readonly List<Func<IDeferralContext<TEvent>, Task<bool>>> Guards = [];

        public Vertex Vertex { get; private set; }
        
        public DeferralGuardBuilder(Vertex vertex)
        {
            Vertex = vertex;
        }
        
        public Func<IDeferralContext<TEvent>, Task<bool>> GetAndGuard()
            => async c =>
            {
                var result = true;
                foreach (var guard in Guards)
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
                foreach (var guard in Guards)
                {
                    if (!await guard(c)) continue;
                    
                    result = true;
                    break;
                }
                
                return result;
            };
        
        public IDeferralGuardBuilder<TEvent> AddGuard(params Func<IDeferralContext<TEvent>, Task<bool>>[] guardsAsync)
        {
            Guards.AddRange(guardsAsync);

            return this;
        }

        public IDeferralGuardBuilder<TEvent> AddAndExpression(Action<IDeferralGuardBuilder<TEvent>> guardExpression)
        {
            var builder = new DeferralGuardBuilder<TEvent>(Vertex);
            guardExpression.Invoke(builder);
            
            Guards.Add(builder.GetAndGuard());

            return this;
        }

        public IDeferralGuardBuilder<TEvent> AddOrExpression(Action<IDeferralGuardBuilder<TEvent>> guardExpression)
        {
            var builder = new DeferralGuardBuilder<TEvent>(Vertex);
            guardExpression.Invoke(builder);
            
            Guards.Add(builder.GetOrGuard());

            return this;
        }
    }
}