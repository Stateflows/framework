using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Stateflows.StateMachines.Models;
using Stateflows.StateMachines.Context.Interfaces;
using Stateflows.StateMachines.Registration.Interfaces.Base;
using Stateflows.StateMachines.Registration.Interfaces.Internal;

namespace Stateflows.StateMachines.Registration.Builders
{
    internal class GuardBuilder<TEvent> :
        IGuardBuilder<TEvent>,
        IDefaultGuardBuilder,
        IEdgeBuilder,
        IVertexBuilder
    {
        private readonly List<Func<ITransitionContext<TEvent>, Task<bool>>> Guards = [];
        private readonly Dictionary<int, OperationRegistration> registrations = [];

        internal void RecordType(Type? type, bool negated = false)
            => registrations[Guards.Count] = OperationRegistration.Leaf(type, negated);
        internal void RecordBehavior(Type? type, BehaviorClass behavior)
            => registrations[Guards.Count] = new OperationRegistration { ImplementationType = type, Behavior = behavior };
        internal string RegistrationAddress => $"{path}.{Guards.Count}";

        internal OperationRegistration Expression(GuardKind kind)
        {
            var expression = new OperationRegistration { Kind = kind };
            for (var index = 0; index < Guards.Count; index++)
                expression.Children.Add(registrations.TryGetValue(index, out var registration) ? registration : OperationRegistration.Leaf());
            return expression;
        }

        public Vertex Vertex => Edge.Source;
        public Edge Edge { get; }
        private readonly string path;
        
        public GuardBuilder(Edge edge, string? path = null)
        {
            Edge = edge;
            this.path = path ?? edge.Guards.Actions.Count.ToString(System.Globalization.CultureInfo.InvariantCulture);
        }
        
        public Func<ITransitionContext<TEvent>, Task<bool>> GetAndGuard()
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
        
        public Func<ITransitionContext<TEvent>, Task<bool>> GetOrGuard()
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
        
        public IGuardBuilder<TEvent> AddGuards(params Func<ITransitionContext<TEvent>, Task<bool>>[] guardsAsync)
        {
            foreach (var guard in guardsAsync)
            {
                if (!registrations.TryGetValue(Guards.Count, out var registration))
                    registrations[Guards.Count] = registration = OperationRegistration.Leaf();
                Guards.Add(context => registration.ObserveAsync(((Context.Classes.BaseContext)context).Context, () => guard(context)));
            }

            return this;
        }

        public IGuardBuilder<TEvent> AddAndExpression(Action<IGuardBuilder<TEvent>> guardExpression)
        {
            var builder = new GuardBuilder<TEvent>(Edge, RegistrationAddress);
            guardExpression.Invoke(builder);
            
            registrations[Guards.Count] = builder.Expression(GuardKind.And);
            AddGuards(builder.GetAndGuard());

            return this;
        }

        public IGuardBuilder<TEvent> AddOrExpression(Action<IGuardBuilder<TEvent>> guardExpression)
        {
            var builder = new GuardBuilder<TEvent>(Edge, RegistrationAddress);
            guardExpression.Invoke(builder);
            
            registrations[Guards.Count] = builder.Expression(GuardKind.Or);
            AddGuards(builder.GetOrGuard());

            return this;
        }

        IDefaultGuardBuilder IBaseDefaultGuard<IDefaultGuardBuilder>.AddGuards(
            params Func<ITransitionContext<Completion>, Task<bool>>[] guardsAsync)
            => (this as GuardBuilder<Completion>)!.AddGuards(guardsAsync) as IDefaultGuardBuilder;

        IDefaultGuardBuilder IDefaultGuardBuilder.AddAndExpression(Action<IDefaultGuardBuilder> guardExpression)
            => AddAndExpression(b => guardExpression.Invoke(b as IDefaultGuardBuilder)) as IDefaultGuardBuilder;

        IDefaultGuardBuilder IDefaultGuardBuilder.AddOrExpression(Action<IDefaultGuardBuilder> guardExpression)
            => AddOrExpression(b => guardExpression.Invoke(b as IDefaultGuardBuilder)) as IDefaultGuardBuilder;
    }
}