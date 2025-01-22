using Stateflows.Activities.Context.Classes;

namespace Stateflows.Activities.Registration.Interfaces
{
    public interface IObjectFlowBuilder<TToken> :
        IFlowWeight<IObjectFlowBuilderWithWeight<TToken>>,
        IObjectFlowGuardBuilderBase<TToken, IObjectFlowBuilder<TToken>>
    {
        IObjectFlowBuilder<TTransformedToken> AddTransformation<TTransformedToken>(TransformationDelegateAsync<TToken, TTransformedToken> transformationAsync);

        IObjectFlowBuilder<TTransformedToken> AddTransformation<TTransformedToken, TTransformation>()
            where TTransformation : class, IFlowTransformation<TToken, TTransformedToken>
            => AddTransformation(c =>
                ((BaseContext)c).Context.Executor.NodeScope.GetObjectFlow<TTransformation, TToken>(c)?.TransformAsync(c.Token)
            );
    }

    public interface IObjectFlowBuilderWithWeight<TToken> :
        IObjectFlowGuardBuilderBase<TToken, IObjectFlowBuilderWithWeight<TToken>>
    {
        IObjectFlowBuilderWithWeight<TTransformedToken> AddTransformation<TTransformedToken>(TransformationDelegateAsync<TToken, TTransformedToken> transformationAsync);

        IObjectFlowBuilderWithWeight<TTransformedToken> AddTransformation<TTransformedToken, TTransformation>()
            where TTransformation : class, IFlowTransformation<TToken, TTransformedToken>
            => AddTransformation(c
                => ((BaseContext)c).Context.Executor.NodeScope.GetObjectFlow<TTransformation, TToken>(c)?.TransformAsync(c.Token)
            );
    }

    public interface IElseObjectFlowBuilderWithWeight<TToken>
    {
        IElseObjectFlowBuilderWithWeight<TTransformedToken> AddTransformation<TTransformedToken>(TransformationDelegateAsync<TToken, TTransformedToken> transformationAsync);

        IElseObjectFlowBuilderWithWeight<TTransformedToken> AddTransformation<TTransformedToken, TTransformation>()
            where TTransformation : class, IFlowTransformation<TToken, TTransformedToken>
            => AddTransformation(c
                => ((BaseContext)c).Context.Executor.NodeScope.GetObjectFlow<TTransformation, TToken>(c)?.TransformAsync(c.Token)
            );
    }

    public interface IElseObjectFlowBuilder<TToken> :
        IFlowWeight<IElseObjectFlowBuilderWithWeight<TToken>>
    {
        IElseObjectFlowBuilder<TTransformedToken> AddTransformation<TTransformedToken>(TransformationDelegateAsync<TToken, TTransformedToken> transformationAsync);

        IElseObjectFlowBuilder<TTransformedToken> AddTransformation<TTransformedToken, TTransformation>()
            where TTransformation : class, IFlowTransformation<TToken, TTransformedToken>
            => AddTransformation(c
                => ((BaseContext)c).Context.Executor.NodeScope.GetObjectFlow<TTransformation, TToken>(c)?.TransformAsync(c.Token)
            );
    }

    public interface IObjectFlowGuardBuilderBase<TToken, out TReturn>
    {
        TReturn AddGuard(GuardDelegateAsync<TToken> guardAsync);

        TReturn AddGuard<TGuard>()
            where TGuard : class, IFlowGuard<TToken>
            => AddGuard(c
                => ((BaseContext)c).Context.Executor.NodeScope.GetObjectFlow<TGuard, TToken>(c)?.GuardAsync(c.Token)
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
                var guard1 = executor.NodeScope.GetObjectFlow<TGuard1, TToken>(c);
                var guard2 = executor.NodeScope.GetObjectFlow<TGuard2, TToken>(c);

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
                var guard1 = executor.NodeScope.GetObjectFlow<TGuard1, TToken>(c);
                var guard2 = executor.NodeScope.GetObjectFlow<TGuard2, TToken>(c);
                var guard3 = executor.NodeScope.GetObjectFlow<TGuard3, TToken>(c);

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
                var guard1 = executor.NodeScope.GetObjectFlow<TGuard1, TToken>(c);
                var guard2 = executor.NodeScope.GetObjectFlow<TGuard2, TToken>(c);
                var guard3 = executor.NodeScope.GetObjectFlow<TGuard3, TToken>(c);
                var guard4 = executor.NodeScope.GetObjectFlow<TGuard4, TToken>(c);

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
                var guard1 = executor.NodeScope.GetObjectFlow<TGuard1, TToken>(c);
                var guard2 = executor.NodeScope.GetObjectFlow<TGuard2, TToken>(c);
                var guard3 = executor.NodeScope.GetObjectFlow<TGuard3, TToken>(c);
                var guard4 = executor.NodeScope.GetObjectFlow<TGuard4, TToken>(c);
                var guard5 = executor.NodeScope.GetObjectFlow<TGuard5, TToken>(c);

                return await guard1.GuardAsync(c.Token) && await guard2.GuardAsync(c.Token) && await guard3.GuardAsync(c.Token) && await guard4.GuardAsync(c.Token) && await guard5.GuardAsync(c.Token);
            });

        TReturn AddOrGuards<TGuard1, TGuard2>()
            where TGuard1 : class, IFlowGuard<TToken>
            where TGuard2 : class, IFlowGuard<TToken>
            => AddGuard(async c =>
            {
                var executor = ((BaseContext)c).Context.Executor;
                var guard1 = executor.NodeScope.GetObjectFlow<TGuard1, TToken>(c);
                var guard2 = executor.NodeScope.GetObjectFlow<TGuard2, TToken>(c);

                return await guard1.GuardAsync(c.Token) || await guard2.GuardAsync(c.Token);
            });

        TReturn AddOrGuards<TGuard1, TGuard2, TGuard3>()
            where TGuard1 : class, IFlowGuard<TToken>
            where TGuard2 : class, IFlowGuard<TToken>
            where TGuard3 : class, IFlowGuard<TToken>
            => AddGuard(async c =>
            {
                var executor = ((BaseContext)c).Context.Executor;
                var guard1 = executor.NodeScope.GetObjectFlow<TGuard1, TToken>(c);
                var guard2 = executor.NodeScope.GetObjectFlow<TGuard2, TToken>(c);
                var guard3 = executor.NodeScope.GetObjectFlow<TGuard3, TToken>(c);

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
                var guard1 = executor.NodeScope.GetObjectFlow<TGuard1, TToken>(c);
                var guard2 = executor.NodeScope.GetObjectFlow<TGuard2, TToken>(c);
                var guard3 = executor.NodeScope.GetObjectFlow<TGuard3, TToken>(c);
                var guard4 = executor.NodeScope.GetObjectFlow<TGuard4, TToken>(c);

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
                var guard1 = executor.NodeScope.GetObjectFlow<TGuard1, TToken>(c);
                var guard2 = executor.NodeScope.GetObjectFlow<TGuard2, TToken>(c);
                var guard3 = executor.NodeScope.GetObjectFlow<TGuard3, TToken>(c);
                var guard4 = executor.NodeScope.GetObjectFlow<TGuard4, TToken>(c);
                var guard5 = executor.NodeScope.GetObjectFlow<TGuard5, TToken>(c);

                return await guard1.GuardAsync(c.Token) || await guard2.GuardAsync(c.Token) || await guard3.GuardAsync(c.Token) || await guard4.GuardAsync(c.Token) || await guard5.GuardAsync(c.Token);
            });
    }

    public interface IControlFlowBuilderBase<out TReturn>
    {
        TReturn AddGuard(GuardDelegateAsync guardAsync);

        TReturn AddGuard<TGuard>()
            where TGuard : class, IControlFlowGuard
            => AddGuard(c => ((BaseContext)c).Context.Executor.NodeScope.GetControlFlow<TGuard>(c)?.GuardAsync());

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
                var guard1 = executor.NodeScope.GetControlFlow<TGuard1>(c);
                var guard2 = executor.NodeScope.GetControlFlow<TGuard2>(c);

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
                var guard1 = executor.NodeScope.GetControlFlow<TGuard1>(c);
                var guard2 = executor.NodeScope.GetControlFlow<TGuard2>(c);
                var guard3 = executor.NodeScope.GetControlFlow<TGuard3>(c);

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
                var guard1 = executor.NodeScope.GetControlFlow<TGuard1>(c);
                var guard2 = executor.NodeScope.GetControlFlow<TGuard2>(c);
                var guard3 = executor.NodeScope.GetControlFlow<TGuard3>(c);
                var guard4 = executor.NodeScope.GetControlFlow<TGuard4>(c);

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
                var guard1 = executor.NodeScope.GetControlFlow<TGuard1>(c);
                var guard2 = executor.NodeScope.GetControlFlow<TGuard2>(c);
                var guard3 = executor.NodeScope.GetControlFlow<TGuard3>(c);
                var guard4 = executor.NodeScope.GetControlFlow<TGuard4>(c);
                var guard5 = executor.NodeScope.GetControlFlow<TGuard5>(c);

                return await guard1.GuardAsync() && await guard2.GuardAsync() && await guard3.GuardAsync() && await guard4.GuardAsync() && await guard5.GuardAsync();
            });

        TReturn AddOrGuards<TGuard1, TGuard2>()
            where TGuard1 : class, IControlFlowGuard
            where TGuard2 : class, IControlFlowGuard
            => AddGuard(async c =>
            {
                var executor = ((BaseContext)c).Context.Executor;
                var guard1 = executor.NodeScope.GetControlFlow<TGuard1>(c);
                var guard2 = executor.NodeScope.GetControlFlow<TGuard2>(c);

                return await guard1.GuardAsync() || await guard2.GuardAsync();
            });

        TReturn AddOrGuards<TGuard1, TGuard2, TGuard3>()
            where TGuard1 : class, IControlFlowGuard
            where TGuard2 : class, IControlFlowGuard
            where TGuard3 : class, IControlFlowGuard
            => AddGuard(async c =>
            {
                var executor = ((BaseContext)c).Context.Executor;
                var guard1 = executor.NodeScope.GetControlFlow<TGuard1>(c);
                var guard2 = executor.NodeScope.GetControlFlow<TGuard2>(c);
                var guard3 = executor.NodeScope.GetControlFlow<TGuard3>(c);

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
                var guard1 = executor.NodeScope.GetControlFlow<TGuard1>(c);
                var guard2 = executor.NodeScope.GetControlFlow<TGuard2>(c);
                var guard3 = executor.NodeScope.GetControlFlow<TGuard3>(c);
                var guard4 = executor.NodeScope.GetControlFlow<TGuard4>(c);

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
                var guard1 = executor.NodeScope.GetControlFlow<TGuard1>(c);
                var guard2 = executor.NodeScope.GetControlFlow<TGuard2>(c);
                var guard3 = executor.NodeScope.GetControlFlow<TGuard3>(c);
                var guard4 = executor.NodeScope.GetControlFlow<TGuard4>(c);
                var guard5 = executor.NodeScope.GetControlFlow<TGuard5>(c);

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
