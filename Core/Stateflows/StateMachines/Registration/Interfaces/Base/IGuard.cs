using System;
using Stateflows.Common.Extensions;
using Stateflows.StateMachines.Context.Classes;
using Stateflows.StateMachines.Registration.Builders;
using Stateflows.StateMachines.Registration.Interfaces.Internal;

namespace Stateflows.StateMachines.Registration.Interfaces.Base
{
    public interface IGuard<TEvent, TReturn> : IBaseGuard<TEvent, TReturn>
    {
        TReturn AddGuardExpression(Func<IGuardBuilder<TEvent>, IGuardBuilder<TEvent>> guardExpression)
        {
            var builder = new GuardBuilder<TEvent>(this as IInternal, (this as TransitionBuilder<TEvent>).Edge);
            guardExpression.Invoke(builder);
            
            return AddGuard(builder.GetAndGuard());
        }

        TReturn AddGuards<TGuard1, TGuard2>()
            where TGuard1 : class, ITransitionGuard<TEvent>
            where TGuard2 : class, ITransitionGuard<TEvent>
            => AddGuardExpression(b => b
                .AddGuard<TGuard1>()
                .AddGuard<TGuard2>()
            );

        [Obsolete("AddAndGuards method is obsolete. Use AddGuardExpression instead")]
        TReturn AddAndGuards<TGuard1, TGuard2>()
            where TGuard1 : class, ITransitionGuard<TEvent>
            where TGuard2 : class, ITransitionGuard<TEvent>
        {
            (this as IInternal).Services.AddServiceType<TGuard1>();
            (this as IInternal).Services.AddServiceType<TGuard2>();

            return AddGuard(async c =>
            {
                var executor = (c as BaseContext).Context.Executor;
                var guard1 = executor.GetTransitionGuard<TGuard1, TEvent>(c);
                var guard2 = executor.GetTransitionGuard<TGuard2, TEvent>(c);

                return await guard1.GuardAsync(c.Event) && await guard2.GuardAsync(c.Event);
            });
        }

        TReturn AddGuards<TGuard1, TGuard2, TGuard3>()
            where TGuard1 : class, ITransitionGuard<TEvent>
            where TGuard2 : class, ITransitionGuard<TEvent>
            where TGuard3 : class, ITransitionGuard<TEvent>
            => AddGuardExpression(b => b
                .AddGuard<TGuard1>()
                .AddGuard<TGuard2>()
                .AddGuard<TGuard3>()
            );

        [Obsolete("AddAndGuards method is obsolete. Use AddGuardExpression instead")]
        TReturn AddAndGuards<TGuard1, TGuard2, TGuard3>()
            where TGuard1 : class, ITransitionGuard<TEvent>
            where TGuard2 : class, ITransitionGuard<TEvent>
            where TGuard3 : class, ITransitionGuard<TEvent>
        {
            (this as IInternal).Services.AddServiceType<TGuard1>();
            (this as IInternal).Services.AddServiceType<TGuard2>();
            (this as IInternal).Services.AddServiceType<TGuard3>();

            return AddGuard(async c =>
            {
                var executor = (c as BaseContext).Context.Executor;
                var guard1 = executor.GetTransitionGuard<TGuard1, TEvent>(c);
                var guard2 = executor.GetTransitionGuard<TGuard2, TEvent>(c);
                var guard3 = executor.GetTransitionGuard<TGuard3, TEvent>(c);

                return await guard1.GuardAsync(c.Event) && await guard2.GuardAsync(c.Event) && await guard3.GuardAsync(c.Event);
            });
        }

        TReturn AddGuards<TGuard1, TGuard2, TGuard3, TGuard4>()
            where TGuard1 : class, ITransitionGuard<TEvent>
            where TGuard2 : class, ITransitionGuard<TEvent>
            where TGuard3 : class, ITransitionGuard<TEvent>
            where TGuard4 : class, ITransitionGuard<TEvent>
            => AddGuardExpression(b => b
                .AddGuard<TGuard1>()
                .AddGuard<TGuard2>()
                .AddGuard<TGuard3>()
                .AddGuard<TGuard4>()
            );

        [Obsolete("AddAndGuards method is obsolete. Use AddGuardExpression instead")]
        TReturn AddAndGuards<TGuard1, TGuard2, TGuard3, TGuard4>()
            where TGuard1 : class, ITransitionGuard<TEvent>
            where TGuard2 : class, ITransitionGuard<TEvent>
            where TGuard3 : class, ITransitionGuard<TEvent>
            where TGuard4 : class, ITransitionGuard<TEvent>
        {
            (this as IInternal).Services.AddServiceType<TGuard1>();
            (this as IInternal).Services.AddServiceType<TGuard2>();
            (this as IInternal).Services.AddServiceType<TGuard3>();
            (this as IInternal).Services.AddServiceType<TGuard4>();

            return AddGuard(async c =>
            {
                var executor = (c as BaseContext).Context.Executor;
                var guard1 = executor.GetTransitionGuard<TGuard1, TEvent>(c);
                var guard2 = executor.GetTransitionGuard<TGuard2, TEvent>(c);
                var guard3 = executor.GetTransitionGuard<TGuard3, TEvent>(c);
                var guard4 = executor.GetTransitionGuard<TGuard4, TEvent>(c);

                return await guard1.GuardAsync(c.Event) && await guard2.GuardAsync(c.Event) && await guard3.GuardAsync(c.Event) && await guard4.GuardAsync(c.Event);
            });
        }

        TReturn AddGuards<TGuard1, TGuard2, TGuard3, TGuard4, TGuard5>()
            where TGuard1 : class, ITransitionGuard<TEvent>
            where TGuard2 : class, ITransitionGuard<TEvent>
            where TGuard3 : class, ITransitionGuard<TEvent>
            where TGuard4 : class, ITransitionGuard<TEvent>
            where TGuard5 : class, ITransitionGuard<TEvent>
            => AddGuardExpression(b => b
                .AddGuard<TGuard1>()
                .AddGuard<TGuard2>()
                .AddGuard<TGuard3>()
                .AddGuard<TGuard4>()
                .AddGuard<TGuard5>()
            );

        [Obsolete("AddAndGuards method is obsolete. Use AddGuardExpression instead")]
        TReturn AddAndGuards<TGuard1, TGuard2, TGuard3, TGuard4, TGuard5>()
            where TGuard1 : class, ITransitionGuard<TEvent>
            where TGuard2 : class, ITransitionGuard<TEvent>
            where TGuard3 : class, ITransitionGuard<TEvent>
            where TGuard4 : class, ITransitionGuard<TEvent>
            where TGuard5 : class, ITransitionGuard<TEvent>
        {
            (this as IInternal).Services.AddServiceType<TGuard1>();
            (this as IInternal).Services.AddServiceType<TGuard2>();
            (this as IInternal).Services.AddServiceType<TGuard3>();
            (this as IInternal).Services.AddServiceType<TGuard4>();
            (this as IInternal).Services.AddServiceType<TGuard5>();

            return AddGuard(async c =>
            {
                var executor = (c as BaseContext).Context.Executor;
                var guard1 = executor.GetTransitionGuard<TGuard1, TEvent>(c);
                var guard2 = executor.GetTransitionGuard<TGuard2, TEvent>(c);
                var guard3 = executor.GetTransitionGuard<TGuard3, TEvent>(c);
                var guard4 = executor.GetTransitionGuard<TGuard4, TEvent>(c);
                var guard5 = executor.GetTransitionGuard<TGuard5, TEvent>(c);

                return await guard1.GuardAsync(c.Event) && await guard2.GuardAsync(c.Event) && await guard3.GuardAsync(c.Event) && await guard4.GuardAsync(c.Event) && await guard5.GuardAsync(c.Event);
            });
        }

        [Obsolete("AddOrGuards method is obsolete. Use AddGuardExpression instead")]
        TReturn AddOrGuards<TGuard1, TGuard2>()
            where TGuard1 : class, ITransitionGuard<TEvent>
            where TGuard2 : class, ITransitionGuard<TEvent>
        {
            (this as IInternal).Services.AddServiceType<TGuard1>();
            (this as IInternal).Services.AddServiceType<TGuard2>();

            return AddGuard(async c =>
            {
                var executor = (c as BaseContext).Context.Executor;
                var guard1 = executor.GetTransitionGuard<TGuard1, TEvent>(c);
                var guard2 = executor.GetTransitionGuard<TGuard2, TEvent>(c);

                return await guard1.GuardAsync(c.Event) || await guard2.GuardAsync(c.Event);
            });
        }

        [Obsolete("AddOrGuards method is obsolete. Use AddGuardExpression instead")]
        TReturn AddOrGuards<TGuard1, TGuard2, TGuard3>()
            where TGuard1 : class, ITransitionGuard<TEvent>
            where TGuard2 : class, ITransitionGuard<TEvent>
            where TGuard3 : class, ITransitionGuard<TEvent>
        {
            (this as IInternal).Services.AddServiceType<TGuard1>();
            (this as IInternal).Services.AddServiceType<TGuard2>();
            (this as IInternal).Services.AddServiceType<TGuard3>();

            return AddGuard(async c =>
            {
                var executor = (c as BaseContext).Context.Executor;
                var guard1 = executor.GetTransitionGuard<TGuard1, TEvent>(c);
                var guard2 = executor.GetTransitionGuard<TGuard2, TEvent>(c);
                var guard3 = executor.GetTransitionGuard<TGuard3, TEvent>(c);

                return await guard1.GuardAsync(c.Event) || await guard2.GuardAsync(c.Event) || await guard3.GuardAsync(c.Event);
            });
        }

        [Obsolete("AddOrGuards method is obsolete. Use AddGuardExpression instead")]
        TReturn AddOrGuards<TGuard1, TGuard2, TGuard3, TGuard4>()
            where TGuard1 : class, ITransitionGuard<TEvent>
            where TGuard2 : class, ITransitionGuard<TEvent>
            where TGuard3 : class, ITransitionGuard<TEvent>
            where TGuard4 : class, ITransitionGuard<TEvent>
        {
            (this as IInternal).Services.AddServiceType<TGuard1>();
            (this as IInternal).Services.AddServiceType<TGuard2>();
            (this as IInternal).Services.AddServiceType<TGuard3>();
            (this as IInternal).Services.AddServiceType<TGuard4>();

            return AddGuard(async c =>
            {
                var executor = (c as BaseContext).Context.Executor;
                var guard1 = executor.GetTransitionGuard<TGuard1, TEvent>(c);
                var guard2 = executor.GetTransitionGuard<TGuard2, TEvent>(c);
                var guard3 = executor.GetTransitionGuard<TGuard3, TEvent>(c);
                var guard4 = executor.GetTransitionGuard<TGuard4, TEvent>(c);

                return await guard1.GuardAsync(c.Event) || await guard2.GuardAsync(c.Event) || await guard3.GuardAsync(c.Event) || await guard4.GuardAsync(c.Event);
            });
        }

        [Obsolete("AddOrGuards method is obsolete. Use AddGuardExpression instead")]
        TReturn AddOrGuards<TGuard1, TGuard2, TGuard3, TGuard4, TGuard5>()
            where TGuard1 : class, ITransitionGuard<TEvent>
            where TGuard2 : class, ITransitionGuard<TEvent>
            where TGuard3 : class, ITransitionGuard<TEvent>
            where TGuard4 : class, ITransitionGuard<TEvent>
            where TGuard5 : class, ITransitionGuard<TEvent>
        {
            (this as IInternal).Services.AddServiceType<TGuard1>();
            (this as IInternal).Services.AddServiceType<TGuard2>();
            (this as IInternal).Services.AddServiceType<TGuard3>();
            (this as IInternal).Services.AddServiceType<TGuard4>();
            (this as IInternal).Services.AddServiceType<TGuard5>();

            return AddGuard(async c =>
            {
                var executor = (c as BaseContext).Context.Executor;
                var guard1 = executor.GetTransitionGuard<TGuard1, TEvent>(c);
                var guard2 = executor.GetTransitionGuard<TGuard2, TEvent>(c);
                var guard3 = executor.GetTransitionGuard<TGuard3, TEvent>(c);
                var guard4 = executor.GetTransitionGuard<TGuard4, TEvent>(c);
                var guard5 = executor.GetTransitionGuard<TGuard5, TEvent>(c);

                return await guard1.GuardAsync(c.Event) || await guard2.GuardAsync(c.Event) || await guard3.GuardAsync(c.Event) || await guard4.GuardAsync(c.Event) || await guard5.GuardAsync(c.Event);
            });
        }
    }
}
