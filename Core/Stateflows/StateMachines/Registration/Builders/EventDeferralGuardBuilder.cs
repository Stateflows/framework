using System;
using System.Collections.Generic;
using System.Linq;
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
        private readonly Dictionary<int, OperationRegistration> registrations = [];

        public void RecordGuard(OperationRegistration registration) => registrations[GuardsList.Count] = registration;

        internal OperationRegistration Expression(GuardKind kind)
        {
            var expression = new OperationRegistration { Kind = kind };
            for (var index = 0; index < GuardsList.Count; index++)
                expression.Children.Add(registrations.TryGetValue(index, out var registration) ? registration : OperationRegistration.Leaf());
            return expression;
        }

        public Type EventType => typeof(TEvent);
        public string RegistrationAddress => $"{path}.{GuardsList.Count}";
        private readonly string path;
        public Vertex Vertex { get; private set; }
        public Logic<StateMachinePredicateAsync> Guards { get; private set; }

        public EventDeferralGuardBuilder(Vertex vertex, Logic<StateMachinePredicateAsync> guards, string? path = null)
        {
            Vertex = vertex;
            Guards = guards;
            this.path = path ?? guards.Actions.Count.ToString(System.Globalization.CultureInfo.InvariantCulture);
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
            foreach (var guard in guardsAsync)
            {
                if (!registrations.TryGetValue(GuardsList.Count, out var registration))
                    registrations[GuardsList.Count] = registration = OperationRegistration.Leaf();
                GuardsList.Add(context => registration.ObserveAsync(((Context.Classes.BaseContext)context).Context, () => guard(context)));
            }

            return this;
        }

        public IEventDeferralGuardBuilder<TEvent> AddAndExpression(Action<IEventDeferralGuardBuilder<TEvent>> guardExpression)
        {
            var builder = new EventDeferralGuardBuilder<TEvent>(Vertex, Guards, RegistrationAddress);
            guardExpression.Invoke(builder);
            
            RecordGuard(builder.Expression(GuardKind.And));
            AddGuards(builder.GetAndGuard());

            return this;
        }

        public IEventDeferralGuardBuilder<TEvent> AddOrExpression(Action<IEventDeferralGuardBuilder<TEvent>> guardExpression)
        {
            var builder = new EventDeferralGuardBuilder<TEvent>(Vertex, Guards, RegistrationAddress);
            guardExpression.Invoke(builder);
            
            RecordGuard(builder.Expression(GuardKind.Or));
            AddGuards(builder.GetOrGuard());

            return this;
        }
    }
}