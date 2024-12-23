using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Stateflows.StateMachines.Context.Interfaces;
using Stateflows.StateMachines.Events;
using Stateflows.StateMachines.Models;
using Stateflows.StateMachines.Registration.Interfaces.Base;
using Stateflows.StateMachines.Registration.Interfaces.Internal;

namespace Stateflows.StateMachines.Registration.Builders
{
    internal class GuardBuilder<TEvent> :
        IGuardBuilder<TEvent>,
        IDefaultGuardBuilder,
        IInternal
    {
        private readonly List<Func<ITransitionContext<TEvent>, Task<bool>>> Guards = new List<Func<ITransitionContext<TEvent>, Task<bool>>>();

        private readonly IInternal InternalContext;
        public Edge Edge { get; private set; }
        
        public GuardBuilder(IInternal internalContext, Edge edge)
        {
            InternalContext = internalContext;
            Edge = edge;
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
        
        public IGuardBuilder<TEvent> AddGuard(Func<ITransitionContext<TEvent>, Task<bool>> guardAsync)
        {
            Guards.Add(guardAsync);

            return this;
        }

        public IGuardBuilder<TEvent> AddAndExpression(Action<IGuardBuilder<TEvent>> guardExpression)
        {
            var builder = new GuardBuilder<TEvent>(InternalContext, Edge);
            guardExpression.Invoke(builder);
            
            Guards.Add(builder.GetAndGuard());

            return this;
        }

        public IGuardBuilder<TEvent> AddOrExpression(Action<IGuardBuilder<TEvent>> guardExpression)
        {
            var builder = new GuardBuilder<TEvent>(InternalContext, Edge);
            guardExpression.Invoke(builder);
            
            Guards.Add(builder.GetOrGuard());

            return this;
        }

        public IServiceCollection Services => InternalContext.Services;

        IDefaultGuardBuilder IBaseDefaultGuard<IDefaultGuardBuilder>.AddGuard(
            Func<ITransitionContext<Completion>, Task<bool>> guardAsync)
            => AddGuard(c => guardAsync.Invoke(c as ITransitionContext<Completion>)) as IDefaultGuardBuilder;

        IDefaultGuardBuilder IDefaultGuardBuilder.AddAndExpression(Action<IDefaultGuardBuilder> guardExpression)
            => AddAndExpression(b => guardExpression.Invoke(b as IDefaultGuardBuilder)) as IDefaultGuardBuilder;

        IDefaultGuardBuilder IDefaultGuardBuilder.AddOrExpression(Action<IDefaultGuardBuilder> guardExpression)
            => AddOrExpression(b => guardExpression.Invoke(b as IDefaultGuardBuilder)) as IDefaultGuardBuilder;
    }
}