﻿using System;
using System.Threading.Tasks;
using Stateflows.Common.Extensions;
using Stateflows.StateMachines.Context.Classes;
using Stateflows.StateMachines.Context.Interfaces;
using Stateflows.StateMachines.Registration.Interfaces.Internal;

namespace Stateflows.StateMachines.Registration.Interfaces.Base
{
    public interface IGuard<TEvent, TReturn>
    {
        TReturn AddGuard(Func<ITransitionContext<TEvent>, Task<bool>> guardAsync);

        TReturn AddGuard<TGuard>()
            where TGuard : class, ITransitionGuard<TEvent>
        {
            (this as IInternal).Services.AddServiceType<TGuard>();

            return AddGuard(c => (c as BaseContext).Context.Executor.GetTransitionGuard<TGuard, TEvent>(c)?.GuardAsync(c.Event));
        }

        TReturn AddGuards<TGuard1, TGuard2>()
            where TGuard1 : class, ITransitionGuard<TEvent>
            where TGuard2 : class, ITransitionGuard<TEvent>
            => AddAndGuards<TGuard1, TGuard2>();

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
            => AddAndGuards<TGuard1, TGuard2, TGuard3>();

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
            => AddAndGuards<TGuard1, TGuard2, TGuard3, TGuard4>();

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
            => AddAndGuards<TGuard1, TGuard2, TGuard3, TGuard4, TGuard5>();

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
