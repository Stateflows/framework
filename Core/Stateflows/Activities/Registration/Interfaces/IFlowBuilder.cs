﻿using System;
using System.Threading.Tasks;
using Stateflows.Activities.Context.Classes;
using Stateflows.Activities.Context.Interfaces;

namespace Stateflows.Activities.Registration.Interfaces
{
    public interface IObjectFlowBuilder<TToken> :
        IFlowWeight<IObjectFlowBuilderWithWeight<TToken>>,
        IObjectFlowGuardBuilderBase<TToken, IObjectFlowBuilder<TToken>>
    {
        IObjectFlowBuilder<TTransformedToken> AddTransformation<TTransformedToken>(TransformationDelegateAsync<TToken, TTransformedToken> transformationAsync);

        IObjectFlowBuilder<TTransformedToken> AddTransformation<TTransformedToken, TTransformation>()
            where TTransformation : class, IFlowTransformation<TToken, TTransformedToken>
            => AddTransformation(async c =>
                await (await ((BaseContext)c).Context.Executor.NodeScope.GetObjectFlowAsync<TTransformation, TToken>(c)).TransformAsync(c.Token)
            );
    }

    public interface IObjectFlowBuilderWithWeight<TToken> :
        IObjectFlowGuardBuilderBase<TToken, IObjectFlowBuilderWithWeight<TToken>>
    {
        IObjectFlowBuilderWithWeight<TTransformedToken> AddTransformation<TTransformedToken>(TransformationDelegateAsync<TToken, TTransformedToken> transformationAsync);

        IObjectFlowBuilderWithWeight<TTransformedToken> AddTransformation<TTransformedToken, TTransformation>()
            where TTransformation : class, IFlowTransformation<TToken, TTransformedToken>
            => AddTransformation(async c
                => await (await ((BaseContext)c).Context.Executor.NodeScope.GetObjectFlowAsync<TTransformation, TToken>(c)).TransformAsync(c.Token)
            );
    }

    public interface IElseObjectFlowBuilderWithWeight<TToken>
    {
        IElseObjectFlowBuilderWithWeight<TTransformedToken> AddTransformation<TTransformedToken>(TransformationDelegateAsync<TToken, TTransformedToken> transformationAsync);

        IElseObjectFlowBuilderWithWeight<TTransformedToken> AddTransformation<TTransformedToken, TTransformation>()
            where TTransformation : class, IFlowTransformation<TToken, TTransformedToken>
            => AddTransformation(async c
                => await (await ((BaseContext)c).Context.Executor.NodeScope.GetObjectFlowAsync<TTransformation, TToken>(c)).TransformAsync(c.Token)
            );
    }

    public interface IElseObjectFlowBuilder<TToken> :
        IFlowWeight<IElseObjectFlowBuilderWithWeight<TToken>>
    {
        IElseObjectFlowBuilder<TTransformedToken> AddTransformation<TTransformedToken>(TransformationDelegateAsync<TToken, TTransformedToken> transformationAsync);

        IElseObjectFlowBuilder<TTransformedToken> AddTransformation<TTransformedToken, TTransformation>()
            where TTransformation : class, IFlowTransformation<TToken, TTransformedToken>
            => AddTransformation(async c
                => await (await ((BaseContext)c).Context.Executor.NodeScope.GetObjectFlowAsync<TTransformation, TToken>(c)).TransformAsync(c.Token)
            );
    }

    public interface IObjectFlowGuardBuilderBase<TToken, out TReturn>
    {
        TReturn AddGuard(params Func<IGuardContext<TToken>, Task<bool>>[]  guardAsync);
        
        TReturn AddGuard<TGuard>()
            where TGuard : class, IFlowGuard<TToken>
            => AddGuard(async c
                => await (await ((BaseContext)c).Context.Executor.NodeScope.GetObjectFlowAsync<TGuard, TToken>(c)).GuardAsync(c.Token)
            );

        TReturn AddGuards<TGuard1, TGuard2>()
            where TGuard1 : class, IFlowGuard<TToken>
            where TGuard2 : class, IFlowGuard<TToken>
            => AddAndGuards<TGuard1, TGuard2>();

        TReturn AddAndGuards<TGuard1, TGuard2>()
            where TGuard1 : class, IFlowGuard<TToken>
            where TGuard2 : class, IFlowGuard<TToken>
            => AddGuard(async c =>
            {
                var executor = ((BaseContext)c).Context.Executor;
                var guard1 = await executor.NodeScope.GetObjectFlowAsync<TGuard1, TToken>(c);
                var guard2 = await executor.NodeScope.GetObjectFlowAsync<TGuard2, TToken>(c);

                return await guard1.GuardAsync(c.Token) && await guard2.GuardAsync(c.Token);
            });

        TReturn AddGuards<TGuard1, TGuard2, TGuard3>()
            where TGuard1 : class, IFlowGuard<TToken>
            where TGuard2 : class, IFlowGuard<TToken>
            where TGuard3 : class, IFlowGuard<TToken>
            => AddAndGuards<TGuard1, TGuard2, TGuard3>();

        TReturn AddAndGuards<TGuard1, TGuard2, TGuard3>()
            where TGuard1 : class, IFlowGuard<TToken>
            where TGuard2 : class, IFlowGuard<TToken>
            where TGuard3 : class, IFlowGuard<TToken>
            => AddGuard(async c =>
            {
                var executor = ((BaseContext)c).Context.Executor;
                var guard1 = await executor.NodeScope.GetObjectFlowAsync<TGuard1, TToken>(c);
                var guard2 = await executor.NodeScope.GetObjectFlowAsync<TGuard2, TToken>(c);
                var guard3 = await executor.NodeScope.GetObjectFlowAsync<TGuard3, TToken>(c);

                return await guard1.GuardAsync(c.Token) && await guard2.GuardAsync(c.Token) && await guard3.GuardAsync(c.Token);
            });

        TReturn AddGuards<TGuard1, TGuard2, TGuard3, TGuard4>()
            where TGuard1 : class, IFlowGuard<TToken>
            where TGuard2 : class, IFlowGuard<TToken>
            where TGuard3 : class, IFlowGuard<TToken>
            where TGuard4 : class, IFlowGuard<TToken>
            => AddAndGuards<TGuard1, TGuard2, TGuard3, TGuard4>();

        TReturn AddAndGuards<TGuard1, TGuard2, TGuard3, TGuard4>()
            where TGuard1 : class, IFlowGuard<TToken>
            where TGuard2 : class, IFlowGuard<TToken>
            where TGuard3 : class, IFlowGuard<TToken>
            where TGuard4 : class, IFlowGuard<TToken>
            => AddGuard(async c =>
            {
                var executor = ((BaseContext)c).Context.Executor;
                var guard1 = await executor.NodeScope.GetObjectFlowAsync<TGuard1, TToken>(c);
                var guard2 = await executor.NodeScope.GetObjectFlowAsync<TGuard2, TToken>(c);
                var guard3 = await executor.NodeScope.GetObjectFlowAsync<TGuard3, TToken>(c);
                var guard4 = await executor.NodeScope.GetObjectFlowAsync<TGuard4, TToken>(c);

                return await guard1.GuardAsync(c.Token) && await guard2.GuardAsync(c.Token) && await guard3.GuardAsync(c.Token) && await guard4.GuardAsync(c.Token);
            });

        TReturn AddGuards<TGuard1, TGuard2, TGuard3, TGuard4, TGuard5>()
            where TGuard1 : class, IFlowGuard<TToken>
            where TGuard2 : class, IFlowGuard<TToken>
            where TGuard3 : class, IFlowGuard<TToken>
            where TGuard4 : class, IFlowGuard<TToken>
            where TGuard5 : class, IFlowGuard<TToken>
            => AddAndGuards<TGuard1, TGuard2, TGuard3, TGuard4, TGuard5>();

        TReturn AddAndGuards<TGuard1, TGuard2, TGuard3, TGuard4, TGuard5>()
            where TGuard1 : class, IFlowGuard<TToken>
            where TGuard2 : class, IFlowGuard<TToken>
            where TGuard3 : class, IFlowGuard<TToken>
            where TGuard4 : class, IFlowGuard<TToken>
            where TGuard5 : class, IFlowGuard<TToken>
            => AddGuard(async c =>
            {
                var executor = ((BaseContext)c).Context.Executor;
                var guard1 = await executor.NodeScope.GetObjectFlowAsync<TGuard1, TToken>(c);
                var guard2 = await executor.NodeScope.GetObjectFlowAsync<TGuard2, TToken>(c);
                var guard3 = await executor.NodeScope.GetObjectFlowAsync<TGuard3, TToken>(c);
                var guard4 = await executor.NodeScope.GetObjectFlowAsync<TGuard4, TToken>(c);
                var guard5 = await executor.NodeScope.GetObjectFlowAsync<TGuard5, TToken>(c);

                return await guard1.GuardAsync(c.Token) && await guard2.GuardAsync(c.Token) && await guard3.GuardAsync(c.Token) && await guard4.GuardAsync(c.Token) && await guard5.GuardAsync(c.Token);
            });

        TReturn AddOrGuards<TGuard1, TGuard2>()
            where TGuard1 : class, IFlowGuard<TToken>
            where TGuard2 : class, IFlowGuard<TToken>
            => AddGuard(async c =>
            {
                var executor = ((BaseContext)c).Context.Executor;
                var guard1 = await executor.NodeScope.GetObjectFlowAsync<TGuard1, TToken>(c);
                var guard2 = await executor.NodeScope.GetObjectFlowAsync<TGuard2, TToken>(c);

                return await guard1.GuardAsync(c.Token) || await guard2.GuardAsync(c.Token);
            });

        TReturn AddOrGuards<TGuard1, TGuard2, TGuard3>()
            where TGuard1 : class, IFlowGuard<TToken>
            where TGuard2 : class, IFlowGuard<TToken>
            where TGuard3 : class, IFlowGuard<TToken>
            => AddGuard(async c =>
            {
                var executor = ((BaseContext)c).Context.Executor;
                var guard1 = await executor.NodeScope.GetObjectFlowAsync<TGuard1, TToken>(c);
                var guard2 = await executor.NodeScope.GetObjectFlowAsync<TGuard2, TToken>(c);
                var guard3 = await executor.NodeScope.GetObjectFlowAsync<TGuard3, TToken>(c);

                return await guard1.GuardAsync(c.Token) || await guard2.GuardAsync(c.Token) || await guard3.GuardAsync(c.Token);
            });

        TReturn AddOrGuards<TGuard1, TGuard2, TGuard3, TGuard4>()
            where TGuard1 : class, IFlowGuard<TToken>
            where TGuard2 : class, IFlowGuard<TToken>
            where TGuard3 : class, IFlowGuard<TToken>
            where TGuard4 : class, IFlowGuard<TToken>
            => AddGuard(async c =>
            {
                var executor = ((BaseContext)c).Context.Executor;
                var guard1 = await executor.NodeScope.GetObjectFlowAsync<TGuard1, TToken>(c);
                var guard2 = await executor.NodeScope.GetObjectFlowAsync<TGuard2, TToken>(c);
                var guard3 = await executor.NodeScope.GetObjectFlowAsync<TGuard3, TToken>(c);
                var guard4 = await executor.NodeScope.GetObjectFlowAsync<TGuard4, TToken>(c);

                return await guard1.GuardAsync(c.Token) || await guard2.GuardAsync(c.Token) || await guard3.GuardAsync(c.Token) || await guard4.GuardAsync(c.Token);
            });

        TReturn AddOrGuards<TGuard1, TGuard2, TGuard3, TGuard4, TGuard5>()
            where TGuard1 : class, IFlowGuard<TToken>
            where TGuard2 : class, IFlowGuard<TToken>
            where TGuard3 : class, IFlowGuard<TToken>
            where TGuard4 : class, IFlowGuard<TToken>
            where TGuard5 : class, IFlowGuard<TToken>
            => AddGuard(async c =>
            {
                var executor = ((BaseContext)c).Context.Executor;
                var guard1 = await executor.NodeScope.GetObjectFlowAsync<TGuard1, TToken>(c);
                var guard2 = await executor.NodeScope.GetObjectFlowAsync<TGuard2, TToken>(c);
                var guard3 = await executor.NodeScope.GetObjectFlowAsync<TGuard3, TToken>(c);
                var guard4 = await executor.NodeScope.GetObjectFlowAsync<TGuard4, TToken>(c);
                var guard5 = await executor.NodeScope.GetObjectFlowAsync<TGuard5, TToken>(c);

                return await guard1.GuardAsync(c.Token) || await guard2.GuardAsync(c.Token) || await guard3.GuardAsync(c.Token) || await guard4.GuardAsync(c.Token) || await guard5.GuardAsync(c.Token);
            });
    }

    public interface IControlFlowBuilderBase<out TReturn>
    {
        TReturn AddGuard(params Func<IGuardContext, Task<bool>>[]  guardsAsync);

        TReturn AddGuard<TGuard>()
            where TGuard : class, IControlFlowGuard
            => AddGuard(async c => await (await ((BaseContext)c).Context.Executor.NodeScope.GetControlFlowAsync<TGuard>(c)).GuardAsync());

        TReturn AddGuards<TGuard1, TGuard2>()
            where TGuard1 : class, IControlFlowGuard
            where TGuard2 : class, IControlFlowGuard
            => AddAndGuards<TGuard1, TGuard2>();

        TReturn AddAndGuards<TGuard1, TGuard2>()
            where TGuard1 : class, IControlFlowGuard
            where TGuard2 : class, IControlFlowGuard
            => AddGuard(async c =>
            {
                var executor = ((BaseContext)c).Context.Executor;
                var guard1 = await executor.NodeScope.GetControlFlowAsync<TGuard1>(c);
                var guard2 = await executor.NodeScope.GetControlFlowAsync<TGuard2>(c);

                return await guard1.GuardAsync() && await guard2.GuardAsync();
            });

        TReturn AddGuards<TGuard1, TGuard2, TGuard3>()
            where TGuard1 : class, IControlFlowGuard
            where TGuard2 : class, IControlFlowGuard
            where TGuard3 : class, IControlFlowGuard
            => AddAndGuards<TGuard1, TGuard2, TGuard3>();

        TReturn AddAndGuards<TGuard1, TGuard2, TGuard3>()
            where TGuard1 : class, IControlFlowGuard
            where TGuard2 : class, IControlFlowGuard
            where TGuard3 : class, IControlFlowGuard
            => AddGuard(async c =>
            {
                var executor = ((BaseContext)c).Context.Executor;
                var guard1 = await executor.NodeScope.GetControlFlowAsync<TGuard1>(c);
                var guard2 = await executor.NodeScope.GetControlFlowAsync<TGuard2>(c);
                var guard3 = await executor.NodeScope.GetControlFlowAsync<TGuard3>(c);

                return await guard1.GuardAsync() && await guard2.GuardAsync() && await guard3.GuardAsync();
            });

        TReturn AddGuards<TGuard1, TGuard2, TGuard3, TGuard4>()
            where TGuard1 : class, IControlFlowGuard
            where TGuard2 : class, IControlFlowGuard
            where TGuard3 : class, IControlFlowGuard
            where TGuard4 : class, IControlFlowGuard
            => AddAndGuards<TGuard1, TGuard2, TGuard3, TGuard4>();

        TReturn AddAndGuards<TGuard1, TGuard2, TGuard3, TGuard4>()
            where TGuard1 : class, IControlFlowGuard
            where TGuard2 : class, IControlFlowGuard
            where TGuard3 : class, IControlFlowGuard
            where TGuard4 : class, IControlFlowGuard
            => AddGuard(async c =>
            {
                var executor = ((BaseContext)c).Context.Executor;
                var guard1 = await executor.NodeScope.GetControlFlowAsync<TGuard1>(c);
                var guard2 = await executor.NodeScope.GetControlFlowAsync<TGuard2>(c);
                var guard3 = await executor.NodeScope.GetControlFlowAsync<TGuard3>(c);
                var guard4 = await executor.NodeScope.GetControlFlowAsync<TGuard4>(c);

                return await guard1.GuardAsync() && await guard2.GuardAsync() && await guard3.GuardAsync() && await guard4.GuardAsync();
            });

        TReturn AddGuards<TGuard1, TGuard2, TGuard3, TGuard4, TGuard5>()
            where TGuard1 : class, IControlFlowGuard
            where TGuard2 : class, IControlFlowGuard
            where TGuard3 : class, IControlFlowGuard
            where TGuard4 : class, IControlFlowGuard
            where TGuard5 : class, IControlFlowGuard
            => AddAndGuards<TGuard1, TGuard2, TGuard3, TGuard4, TGuard5>();

        TReturn AddAndGuards<TGuard1, TGuard2, TGuard3, TGuard4, TGuard5>()
            where TGuard1 : class, IControlFlowGuard
            where TGuard2 : class, IControlFlowGuard
            where TGuard3 : class, IControlFlowGuard
            where TGuard4 : class, IControlFlowGuard
            where TGuard5 : class, IControlFlowGuard
            => AddGuard(async c =>
            {
                var executor = ((BaseContext)c).Context.Executor;
                var guard1 = await executor.NodeScope.GetControlFlowAsync<TGuard1>(c);
                var guard2 = await executor.NodeScope.GetControlFlowAsync<TGuard2>(c);
                var guard3 = await executor.NodeScope.GetControlFlowAsync<TGuard3>(c);
                var guard4 = await executor.NodeScope.GetControlFlowAsync<TGuard4>(c);
                var guard5 = await executor.NodeScope.GetControlFlowAsync<TGuard5>(c);

                return await guard1.GuardAsync() && await guard2.GuardAsync() && await guard3.GuardAsync() && await guard4.GuardAsync() && await guard5.GuardAsync();
            });

        TReturn AddOrGuards<TGuard1, TGuard2>()
            where TGuard1 : class, IControlFlowGuard
            where TGuard2 : class, IControlFlowGuard
            => AddGuard(async c =>
            {
                var executor = ((BaseContext)c).Context.Executor;
                var guard1 = await executor.NodeScope.GetControlFlowAsync<TGuard1>(c);
                var guard2 = await executor.NodeScope.GetControlFlowAsync<TGuard2>(c);

                return await guard1.GuardAsync() || await guard2.GuardAsync();
            });

        TReturn AddOrGuards<TGuard1, TGuard2, TGuard3>()
            where TGuard1 : class, IControlFlowGuard
            where TGuard2 : class, IControlFlowGuard
            where TGuard3 : class, IControlFlowGuard
            => AddGuard(async c =>
            {
                var executor = ((BaseContext)c).Context.Executor;
                var guard1 = await executor.NodeScope.GetControlFlowAsync<TGuard1>(c);
                var guard2 = await executor.NodeScope.GetControlFlowAsync<TGuard2>(c);
                var guard3 = await executor.NodeScope.GetControlFlowAsync<TGuard3>(c);

                return await guard1.GuardAsync() || await guard2.GuardAsync() || await guard3.GuardAsync();
            });

        TReturn AddOrGuards<TGuard1, TGuard2, TGuard3, TGuard4>()
            where TGuard1 : class, IControlFlowGuard
            where TGuard2 : class, IControlFlowGuard
            where TGuard3 : class, IControlFlowGuard
            where TGuard4 : class, IControlFlowGuard
            => AddGuard(async c =>
            {
                var executor = ((BaseContext)c).Context.Executor;
                var guard1 = await executor.NodeScope.GetControlFlowAsync<TGuard1>(c);
                var guard2 = await executor.NodeScope.GetControlFlowAsync<TGuard2>(c);
                var guard3 = await executor.NodeScope.GetControlFlowAsync<TGuard3>(c);
                var guard4 = await executor.NodeScope.GetControlFlowAsync<TGuard4>(c);

                return await guard1.GuardAsync() || await guard2.GuardAsync() || await guard3.GuardAsync() || await guard4.GuardAsync();
            });

        TReturn AddOrGuards<TGuard1, TGuard2, TGuard3, TGuard4, TGuard5>()
            where TGuard1 : class, IControlFlowGuard
            where TGuard2 : class, IControlFlowGuard
            where TGuard3 : class, IControlFlowGuard
            where TGuard4 : class, IControlFlowGuard
            where TGuard5 : class, IControlFlowGuard
            => AddGuard(async c =>
            {
                var executor = ((BaseContext)c).Context.Executor;
                var guard1 = await executor.NodeScope.GetControlFlowAsync<TGuard1>(c);
                var guard2 = await executor.NodeScope.GetControlFlowAsync<TGuard2>(c);
                var guard3 = await executor.NodeScope.GetControlFlowAsync<TGuard3>(c);
                var guard4 = await executor.NodeScope.GetControlFlowAsync<TGuard4>(c);
                var guard5 = await executor.NodeScope.GetControlFlowAsync<TGuard5>(c);

                return await guard1.GuardAsync() || await guard2.GuardAsync() || await guard3.GuardAsync() || await guard4.GuardAsync() || await guard5.GuardAsync();
            });
    }

    public interface IControlFlowBuilder :
        IControlFlowBuilderBase<IControlFlowBuilderWithWeight>,
        IFlowWeight<IControlFlowBuilderWithWeight>
    { }

    public interface IControlFlowBuilderWithWeight : 
        IControlFlowBuilderBase<IControlFlowBuilderWithWeight>
    { }

    public interface IElseControlFlowBuilder : IFlowWeight
    { }

    public interface IElseControlFlowBuilderWithWeight
    { }

    public interface IFlowWeight<out TReturn>
    {
        TReturn SetWeight(int weight);
    }

    public interface IFlowWeight
    {
        void SetWeight(int weight);
    }
}
