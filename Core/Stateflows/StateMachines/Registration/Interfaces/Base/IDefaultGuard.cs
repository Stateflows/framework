using System;
using Stateflows.Common.Extensions;
using Stateflows.StateMachines.Events;
using Stateflows.StateMachines.Context.Classes;
using Stateflows.StateMachines.Registration.Builders;
using Stateflows.StateMachines.Registration.Interfaces.Internal;

namespace Stateflows.StateMachines.Registration.Interfaces.Base
{
    public interface IDefaultGuard<TReturn> : IBaseDefaultGuard<TReturn>
    {
        TReturn AddGuardExpression(Func<IDefaultGuardBuilder, IDefaultGuardBuilder> guardExpression)
        {
            var builder = new GuardBuilder<Completion>(this as IInternal, (this as TransitionBuilder<Completion>).Edge);
            guardExpression.Invoke(builder);
            
            return AddGuard(builder.GetAndGuard());
        }

        TReturn AddGuards<TGuard1, TGuard2>()
            where TGuard1 : class, IDefaultTransitionGuard
            where TGuard2 : class, IDefaultTransitionGuard
            => AddGuardExpression(b => b
                .AddGuard<TGuard1>()
                .AddGuard<TGuard2>()
            );

        [Obsolete("AddAndGuards method is obsolete. Use AddGuardExpression instead")]
        TReturn AddAndGuards<TGuard1, TGuard2>()
            where TGuard1 : class, IDefaultTransitionGuard
            where TGuard2 : class, IDefaultTransitionGuard
        {
            (this as IInternal).Services.AddServiceType<TGuard1>();
            (this as IInternal).Services.AddServiceType<TGuard2>();

            return AddGuard(async c =>
            {
                var executor = (c as BaseContext).Context.Executor;
                var guard1 = executor.GetDefaultTransitionGuard<TGuard1>(c);
                var guard2 = executor.GetDefaultTransitionGuard<TGuard2>(c);

                return await guard1.GuardAsync() && await guard2.GuardAsync();
            });
        }

        TReturn AddGuards<TGuard1, TGuard2, TGuard3>()
            where TGuard1 : class, IDefaultTransitionGuard
            where TGuard2 : class, IDefaultTransitionGuard
            where TGuard3 : class, IDefaultTransitionGuard
            => AddGuardExpression(b => b
                .AddGuard<TGuard1>()
                .AddGuard<TGuard2>()
                .AddGuard<TGuard3>()
            );

        [Obsolete("AddAndGuards method is obsolete. Use AddGuardExpression instead")]
        TReturn AddAndGuards<TGuard1, TGuard2, TGuard3>()
            where TGuard1 : class, IDefaultTransitionGuard
            where TGuard2 : class, IDefaultTransitionGuard
            where TGuard3 : class, IDefaultTransitionGuard
        {
            (this as IInternal).Services.AddServiceType<TGuard1>();
            (this as IInternal).Services.AddServiceType<TGuard2>();
            (this as IInternal).Services.AddServiceType<TGuard3>();

            return AddGuard(async c =>
            {
                var executor = (c as BaseContext).Context.Executor;
                var guard1 = executor.GetDefaultTransitionGuard<TGuard1>(c);
                var guard2 = executor.GetDefaultTransitionGuard<TGuard2>(c);
                var guard3 = executor.GetDefaultTransitionGuard<TGuard3>(c);

                return await guard1.GuardAsync() && await guard2.GuardAsync() && await guard3.GuardAsync();
            });
        }

        TReturn AddGuards<TGuard1, TGuard2, TGuard3, TGuard4>()
            where TGuard1 : class, IDefaultTransitionGuard
            where TGuard2 : class, IDefaultTransitionGuard
            where TGuard3 : class, IDefaultTransitionGuard
            where TGuard4 : class, IDefaultTransitionGuard
            => AddGuardExpression(b => b
                .AddGuard<TGuard1>()
                .AddGuard<TGuard2>()
                .AddGuard<TGuard3>()
                .AddGuard<TGuard4>()
            );

        [Obsolete("AddAndGuards method is obsolete. Use AddGuardExpression instead")]
        TReturn AddAndGuards<TGuard1, TGuard2, TGuard3, TGuard4>()
            where TGuard1 : class, IDefaultTransitionGuard
            where TGuard2 : class, IDefaultTransitionGuard
            where TGuard3 : class, IDefaultTransitionGuard
            where TGuard4 : class, IDefaultTransitionGuard
        {
            (this as IInternal).Services.AddServiceType<TGuard1>();
            (this as IInternal).Services.AddServiceType<TGuard2>();
            (this as IInternal).Services.AddServiceType<TGuard3>();
            (this as IInternal).Services.AddServiceType<TGuard4>();

            return AddGuard(async c =>
            {
                var executor = (c as BaseContext).Context.Executor;
                var guard1 = executor.GetDefaultTransitionGuard<TGuard1>(c);
                var guard2 = executor.GetDefaultTransitionGuard<TGuard2>(c);
                var guard3 = executor.GetDefaultTransitionGuard<TGuard3>(c);
                var guard4 = executor.GetDefaultTransitionGuard<TGuard4>(c);

                return await guard1.GuardAsync() && await guard2.GuardAsync() && await guard3.GuardAsync() && await guard4.GuardAsync();
            });
        }

        TReturn AddGuards<TGuard1, TGuard2, TGuard3, TGuard4, TGuard5>()
            where TGuard1 : class, IDefaultTransitionGuard
            where TGuard2 : class, IDefaultTransitionGuard
            where TGuard3 : class, IDefaultTransitionGuard
            where TGuard4 : class, IDefaultTransitionGuard
            where TGuard5 : class, IDefaultTransitionGuard
            => AddGuardExpression(b => b
                .AddGuard<TGuard1>()
                .AddGuard<TGuard2>()
                .AddGuard<TGuard3>()
                .AddGuard<TGuard4>()
                .AddGuard<TGuard5>()
            );
        
        [Obsolete("AddAndGuards method is obsolete. Use AddGuardExpression instead")]
        TReturn AddAndGuards<TGuard1, TGuard2, TGuard3, TGuard4, TGuard5>()
            where TGuard1 : class, IDefaultTransitionGuard
            where TGuard2 : class, IDefaultTransitionGuard
            where TGuard3 : class, IDefaultTransitionGuard
            where TGuard4 : class, IDefaultTransitionGuard
            where TGuard5 : class, IDefaultTransitionGuard
        {
            (this as IInternal).Services.AddServiceType<TGuard1>();
            (this as IInternal).Services.AddServiceType<TGuard2>();
            (this as IInternal).Services.AddServiceType<TGuard3>();
            (this as IInternal).Services.AddServiceType<TGuard4>();
            (this as IInternal).Services.AddServiceType<TGuard5>();

            return AddGuard(async c =>
            {
                var executor = (c as BaseContext).Context.Executor;
                var guard1 = executor.GetDefaultTransitionGuard<TGuard1>(c);
                var guard2 = executor.GetDefaultTransitionGuard<TGuard2>(c);
                var guard3 = executor.GetDefaultTransitionGuard<TGuard3>(c);
                var guard4 = executor.GetDefaultTransitionGuard<TGuard4>(c);
                var guard5 = executor.GetDefaultTransitionGuard<TGuard5>(c);

                return await guard1.GuardAsync() && await guard2.GuardAsync() && await guard3.GuardAsync() && await guard4.GuardAsync() && await guard5.GuardAsync();
            });
        }

        [Obsolete("AddOrGuards method is obsolete. Use AddGuardExpression instead")]
        TReturn AddOrGuards<TGuard1, TGuard2>()
            where TGuard1 : class, IDefaultTransitionGuard
            where TGuard2 : class, IDefaultTransitionGuard
        {
            (this as IInternal).Services.AddServiceType<TGuard1>();
            (this as IInternal).Services.AddServiceType<TGuard2>();

            return AddGuard(async c =>
            {
                var executor = (c as BaseContext).Context.Executor;
                var guard1 = executor.GetDefaultTransitionGuard<TGuard1>(c);
                var guard2 = executor.GetDefaultTransitionGuard<TGuard2>(c);

                return await guard1.GuardAsync() || await guard2.GuardAsync();
            });
        }

        [Obsolete("AddOrGuards method is obsolete. Use AddGuardExpression instead")]
        TReturn AddOrGuards<TGuard1, TGuard2, TGuard3>()
            where TGuard1 : class, IDefaultTransitionGuard
            where TGuard2 : class, IDefaultTransitionGuard
            where TGuard3 : class, IDefaultTransitionGuard
        {
            (this as IInternal).Services.AddServiceType<TGuard1>();
            (this as IInternal).Services.AddServiceType<TGuard2>();
            (this as IInternal).Services.AddServiceType<TGuard3>();

            return AddGuard(async c =>
            {
                var executor = (c as BaseContext).Context.Executor;
                var guard1 = executor.GetDefaultTransitionGuard<TGuard1>(c);
                var guard2 = executor.GetDefaultTransitionGuard<TGuard2>(c);
                var guard3 = executor.GetDefaultTransitionGuard<TGuard3>(c);

                return await guard1.GuardAsync() || await guard2.GuardAsync() || await guard3.GuardAsync();
            });
        }

        [Obsolete("AddOrGuards method is obsolete. Use AddGuardExpression instead")]
        TReturn AddOrGuards<TGuard1, TGuard2, TGuard3, TGuard4>()
            where TGuard1 : class, IDefaultTransitionGuard
            where TGuard2 : class, IDefaultTransitionGuard
            where TGuard3 : class, IDefaultTransitionGuard
            where TGuard4 : class, IDefaultTransitionGuard
        {
            (this as IInternal).Services.AddServiceType<TGuard1>();
            (this as IInternal).Services.AddServiceType<TGuard2>();
            (this as IInternal).Services.AddServiceType<TGuard3>();
            (this as IInternal).Services.AddServiceType<TGuard4>();

            return AddGuard(async c =>
            {
                var executor = (c as BaseContext).Context.Executor;
                var guard1 = executor.GetDefaultTransitionGuard<TGuard1>(c);
                var guard2 = executor.GetDefaultTransitionGuard<TGuard2>(c);
                var guard3 = executor.GetDefaultTransitionGuard<TGuard3>(c);
                var guard4 = executor.GetDefaultTransitionGuard<TGuard4>(c);

                return await guard1.GuardAsync() || await guard2.GuardAsync() || await guard3.GuardAsync() || await guard4.GuardAsync();
            });
        }

        [Obsolete("AddOrGuards method is obsolete. Use AddGuardExpression instead")]
        TReturn AddOrGuards<TGuard1, TGuard2, TGuard3, TGuard4, TGuard5>()
            where TGuard1 : class, IDefaultTransitionGuard
            where TGuard2 : class, IDefaultTransitionGuard
            where TGuard3 : class, IDefaultTransitionGuard
            where TGuard4 : class, IDefaultTransitionGuard
            where TGuard5 : class, IDefaultTransitionGuard
        {
            (this as IInternal).Services.AddServiceType<TGuard1>();
            (this as IInternal).Services.AddServiceType<TGuard2>();
            (this as IInternal).Services.AddServiceType<TGuard3>();
            (this as IInternal).Services.AddServiceType<TGuard4>();
            (this as IInternal).Services.AddServiceType<TGuard5>();

            return AddGuard(async c =>
            {
                var executor = (c as BaseContext).Context.Executor;
                var guard1 = executor.GetDefaultTransitionGuard<TGuard1>(c);
                var guard2 = executor.GetDefaultTransitionGuard<TGuard2>(c);
                var guard3 = executor.GetDefaultTransitionGuard<TGuard3>(c);
                var guard4 = executor.GetDefaultTransitionGuard<TGuard4>(c);
                var guard5 = executor.GetDefaultTransitionGuard<TGuard5>(c);

                return await guard1.GuardAsync() || await guard2.GuardAsync() || await guard3.GuardAsync() || await guard4.GuardAsync() || await guard5.GuardAsync();
            });
        }
    }
}
