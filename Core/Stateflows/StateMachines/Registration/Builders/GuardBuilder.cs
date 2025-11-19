using System;
using System.Collections.Generic;
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
        IVertexBuilder
    {
        private readonly List<Func<ITransitionContext<TEvent>, Task<bool>>> Guards = [];

        public Vertex Vertex { get; private set; }
        
        public GuardBuilder(Vertex vertex)
        {
            Vertex = vertex;
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
        
        public IGuardBuilder<TEvent> AddGuard(params Func<ITransitionContext<TEvent>, Task<bool>>[] guardsAsync)
        {
            Guards.AddRange(guardsAsync);

            return this;
        }

        public IGuardBuilder<TEvent> AddAndExpression(Action<IGuardBuilder<TEvent>> guardExpression)
        {
            var builder = new GuardBuilder<TEvent>(Vertex);
            guardExpression.Invoke(builder);
            
            Guards.Add(builder.GetAndGuard());

            return this;
        }

        public IGuardBuilder<TEvent> AddOrExpression(Action<IGuardBuilder<TEvent>> guardExpression)
        {
            var builder = new GuardBuilder<TEvent>(Vertex);
            guardExpression.Invoke(builder);
            
            Guards.Add(builder.GetOrGuard());

            return this;
        }

        IDefaultGuardBuilder IBaseDefaultGuard<IDefaultGuardBuilder>.AddGuard(
            params Func<ITransitionContext<Completion>, Task<bool>>[] guardsAsync)
            => (this as GuardBuilder<Completion>)!.AddGuard(guardsAsync) as IDefaultGuardBuilder;

        IDefaultGuardBuilder IDefaultGuardBuilder.AddAndExpression(Action<IDefaultGuardBuilder> guardExpression)
            => AddAndExpression(b => guardExpression.Invoke(b as IDefaultGuardBuilder)) as IDefaultGuardBuilder;

        IDefaultGuardBuilder IDefaultGuardBuilder.AddOrExpression(Action<IDefaultGuardBuilder> guardExpression)
            => AddOrExpression(b => guardExpression.Invoke(b as IDefaultGuardBuilder)) as IDefaultGuardBuilder;
    }
}