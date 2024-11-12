using System;
using System.Threading.Tasks;
using Stateflows.Common.Extensions;
using Stateflows.StateMachines.Context.Classes;
using Stateflows.StateMachines.Context.Interfaces;
using Stateflows.StateMachines.Events;
using Stateflows.StateMachines.Registration.Interfaces.Internal;

namespace Stateflows.StateMachines.Registration.Interfaces.Base
{
    public interface IDefaultGuard<TReturn>
    {
        TReturn AddGuard(Func<ITransitionContext<Completion>, Task<bool>> guardAsync);

        TReturn AddGuard<TGuard>()
            where TGuard : class, IDefaultTransitionGuard
        {
            (this as IInternal).Services.AddServiceType<TGuard>();

            return AddGuard(c => (c as BaseContext).Context.Executor.GetDefaultTransitionGuard<TGuard>(c)?.GuardAsync());
        }

        TReturn AddGuards<TGuard1, TGuard2>()
            where TGuard1 : class, IDefaultTransitionGuard
            where TGuard2 : class, IDefaultTransitionGuard
            => AddAndGuards<TGuard1, TGuard2>();

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
            => AddAndGuards<TGuard1, TGuard2, TGuard3>();

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
            => AddAndGuards<TGuard1, TGuard2, TGuard3, TGuard4>();

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

        TReturn Adduards<TGuard1, TGuard2, TGuard3, TGuard4, TGuard5>()
            where TGuard1 : class, IDefaultTransitionGuard
            where TGuard2 : class, IDefaultTransitionGuard
            where TGuard3 : class, IDefaultTransitionGuard
            where TGuard4 : class, IDefaultTransitionGuard
            where TGuard5 : class, IDefaultTransitionGuard
            => AddAndGuards<TGuard1, TGuard2, TGuard3, TGuard4, TGuard5>();

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
