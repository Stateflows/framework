using System;
using System.Threading.Tasks;
using Stateflows.Activities.Context.Classes;
using Stateflows.Activities.Context.Interfaces;

namespace Stateflows.Activities.Registration.Interfaces
{
    /// <summary>Configures a typed object flow, including its guard, weight, and optional transformation.</summary>
    /// <typeparam name="TToken">Token type carried by the object flow.</typeparam>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/activity-flows" />
    public interface IObjectFlowBuilder<out TToken> :
        IFlowWeight<IObjectFlowBuilderWithWeight<TToken>>,
        IObjectFlowGuardBuilderBase<TToken, IObjectFlowBuilder<TToken>>
    {
        /// <summary>Transforms each token transferred by this flow.</summary>
        /// <typeparam name="TTransformedToken">Token type produced by the object-flow transformation.</typeparam>
        /// <param name="transformationAsync">Asynchronous callback mapping the input token in the flow context to the
        /// output token.</param>
        /// <returns>A flow builder for the transformed token type.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/activity-flows" />
        IObjectFlowBuilder<TTransformedToken> AddTransformation<TTransformedToken>(TransformationDelegateAsync<TToken, TTransformedToken> transformationAsync);

        /// <summary>Transforms each token using a registered transformation implementation.</summary>
        /// <typeparam name="TTransformedToken">Token type produced by the object-flow transformation.</typeparam>
        /// <typeparam name="TTransformation">Typed flow transformation implementing
        /// <see cref="IFlowTransformation{TToken, TTransformedToken}" />.</typeparam>
        /// <returns>A flow builder for the transformed token type.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/activity-flows" />
        IObjectFlowBuilder<TTransformedToken> AddTransformation<TTransformedToken, TTransformation>()
            where TTransformation : class, IFlowTransformation<TToken, TTransformedToken>
            => AddTransformation(async c =>
                await (await ((BaseContext)c).Context.Executor.NodeScope.GetObjectFlowAsync<TTransformation, TToken>(c)).TransformAsync(c.Token)
            );
    }

    /// <summary>Configures a weighted typed object flow and its guard or transformation.</summary>
    /// <typeparam name="TToken">Token type carried by the object flow.</typeparam>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/activity-flows" />
    public interface IObjectFlowBuilderWithWeight<out TToken> :
        IObjectFlowGuardBuilderBase<TToken, IObjectFlowBuilderWithWeight<TToken>>
    {
        /// <summary>Transforms each token transferred by this flow.</summary>
        /// <typeparam name="TTransformedToken">Token type produced by the object-flow transformation.</typeparam>
        /// <param name="transformationAsync">Asynchronous callback mapping the input token in the flow context to the
        /// output token.</param>
        /// <returns>A weighted flow builder for the transformed token type.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/activity-flows" />
        IObjectFlowBuilderWithWeight<TTransformedToken> AddTransformation<TTransformedToken>(TransformationDelegateAsync<TToken, TTransformedToken> transformationAsync);

        /// <summary>Transforms each token using a registered transformation implementation.</summary>
        /// <typeparam name="TTransformedToken">Token type produced by the object-flow transformation.</typeparam>
        /// <typeparam name="TTransformation">Typed flow transformation implementing
        /// <see cref="IFlowTransformation{TToken, TTransformedToken}" />.</typeparam>
        /// <returns>A weighted flow builder for the transformed token type.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/activity-flows" />
        IObjectFlowBuilderWithWeight<TTransformedToken> AddTransformation<TTransformedToken, TTransformation>()
            where TTransformation : class, IFlowTransformation<TToken, TTransformedToken>
            => AddTransformation(async c
                => await (await ((BaseContext)c).Context.Executor.NodeScope.GetObjectFlowAsync<TTransformation, TToken>(c)).TransformAsync(c.Token)
            );
    }

    /// <summary>Configures a weighted fallback object flow, including optional transformation.</summary>
    /// <typeparam name="TToken">Token type carried by the object flow.</typeparam>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/activity-flows" />
    public interface IElseObjectFlowBuilderWithWeight<out TToken>
    {
        /// <summary>Transforms each token transferred by this fallback flow.</summary>
        /// <typeparam name="TTransformedToken">Token type produced by the object-flow transformation.</typeparam>
        /// <param name="transformationAsync">Asynchronous callback mapping the input token in the flow context to the
        /// output token.</param>
        /// <returns>A weighted fallback flow builder for the transformed token type.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/activity-flows" />
        IElseObjectFlowBuilderWithWeight<TTransformedToken> AddTransformation<TTransformedToken>(TransformationDelegateAsync<TToken, TTransformedToken> transformationAsync);

        /// <summary>Transforms each token using a registered transformation implementation.</summary>
        /// <typeparam name="TTransformedToken">Token type produced by the object-flow transformation.</typeparam>
        /// <typeparam name="TTransformation">Typed flow transformation implementing
        /// <see cref="IFlowTransformation{TToken, TTransformedToken}" />.</typeparam>
        /// <returns>A weighted fallback flow builder for the transformed token type.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/activity-flows" />
        IElseObjectFlowBuilderWithWeight<TTransformedToken> AddTransformation<TTransformedToken, TTransformation>()
            where TTransformation : class, IFlowTransformation<TToken, TTransformedToken>
            => AddTransformation(async c
                => await (await ((BaseContext)c).Context.Executor.NodeScope.GetObjectFlowAsync<TTransformation, TToken>(c)).TransformAsync(c.Token)
            );
    }

    /// <summary>Configures a fallback object flow, including its weight and optional transformation.</summary>
    /// <typeparam name="TToken">Token type carried by the object flow.</typeparam>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/activity-flows" />
    public interface IElseObjectFlowBuilder<out TToken> :
        IFlowWeight<IElseObjectFlowBuilderWithWeight<TToken>>
    {
        /// <summary>Transforms each token transferred by this fallback flow.</summary>
        /// <typeparam name="TTransformedToken">Token type produced by the object-flow transformation.</typeparam>
        /// <param name="transformationAsync">Asynchronous callback mapping the input token in the flow context to the
        /// output token.</param>
        /// <returns>A fallback flow builder for the transformed token type.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/activity-flows" />
        IElseObjectFlowBuilder<TTransformedToken> AddTransformation<TTransformedToken>(TransformationDelegateAsync<TToken, TTransformedToken> transformationAsync);

        /// <summary>Transforms each token using a registered transformation implementation.</summary>
        /// <typeparam name="TTransformedToken">Token type produced by the object-flow transformation.</typeparam>
        /// <typeparam name="TTransformation">Typed flow transformation implementing
        /// <see cref="IFlowTransformation{TToken, TTransformedToken}" />.</typeparam>
        /// <returns>A fallback flow builder for the transformed token type.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/activity-flows" />
        IElseObjectFlowBuilder<TTransformedToken> AddTransformation<TTransformedToken, TTransformation>()
            where TTransformation : class, IFlowTransformation<TToken, TTransformedToken>
            => AddTransformation(async c
                => await (await ((BaseContext)c).Context.Executor.NodeScope.GetObjectFlowAsync<TTransformation, TToken>(c)).TransformAsync(c.Token)
            );
    }

    /// <summary>Configures guards for a typed object flow.</summary>
    /// <typeparam name="TToken">Token type carried by the object flow.</typeparam>
    /// <typeparam name="TReturn">Fluent builder type returned after this Activity element is configured.</typeparam>
    /// <remarks>Multiple guards combined by the AND methods must all return true for this flow to be eligible.
    /// </remarks>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/activity-flows" />
    public interface IObjectFlowGuardBuilderBase<out TToken, out TReturn>
    {
        /// <summary>Adds one or more asynchronous guards evaluated for this token flow.</summary>
        /// <param name="guardAsync">Asynchronous predicate evaluated for each candidate token or control activation; true
        /// permits the flow.</param>
        /// <returns>The current flow builder.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/activity-flows" />
        TReturn AddGuard(params Func<IGuardContext<TToken>, Task<bool>>[]  guardAsync);
        
        /// <summary>Adds a guard implementation type for this flow.</summary>
        /// <typeparam name="TGuard">Typed flow guard implementing <see cref="IFlowGuard&lt;TToken&gt;" />; return true to
        /// permit this flow.</typeparam>
        /// <returns>The current flow builder.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/activity-flows" />
        TReturn AddGuard<TGuard>()
            where TGuard : class, IFlowGuard<TToken>
            => AddGuard(async c
                => await (await ((BaseContext)c).Context.Executor.NodeScope.GetObjectFlowAsync<TGuard, TToken>(c)).GuardAsync(c.Token)
            );

        /// <summary>Adds two guard types combined with logical AND.</summary>
        /// <typeparam name="TGuard1">Typed flow guard implementing <see cref="IFlowGuard&lt;TToken&gt;" />; return true to
        /// permit this flow.</typeparam>
        /// <typeparam name="TGuard2">Typed flow guard implementing <see cref="IFlowGuard&lt;TToken&gt;" />; return true to
        /// permit this flow.</typeparam>
        /// <returns>The current flow builder.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/activity-flows" />
        TReturn AddGuards<TGuard1, TGuard2>()
            where TGuard1 : class, IFlowGuard<TToken>
            where TGuard2 : class, IFlowGuard<TToken>
            => AddAndGuards<TGuard1, TGuard2>();

        /// <summary>Adds two guard types combined with logical AND.</summary>
        /// <typeparam name="TGuard1">Typed flow guard implementing <see cref="IFlowGuard&lt;TToken&gt;" />; return true to
        /// permit this flow.</typeparam>
        /// <typeparam name="TGuard2">Typed flow guard implementing <see cref="IFlowGuard&lt;TToken&gt;" />; return true to
        /// permit this flow.</typeparam>
        /// <returns>The current flow builder.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/activity-flows" />
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

        /// <summary>Adds three guard types combined with logical AND.</summary>
        /// <typeparam name="TGuard1">Typed flow guard implementing <see cref="IFlowGuard&lt;TToken&gt;" />; return true to
        /// permit this flow.</typeparam>
        /// <typeparam name="TGuard2">Typed flow guard implementing <see cref="IFlowGuard&lt;TToken&gt;" />; return true to
        /// permit this flow.</typeparam>
        /// <typeparam name="TGuard3">Typed flow guard implementing <see cref="IFlowGuard&lt;TToken&gt;" />; return true to
        /// permit this flow.</typeparam>
        /// <returns>The current flow builder.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/activity-flows" />
        TReturn AddGuards<TGuard1, TGuard2, TGuard3>()
            where TGuard1 : class, IFlowGuard<TToken>
            where TGuard2 : class, IFlowGuard<TToken>
            where TGuard3 : class, IFlowGuard<TToken>
            => AddAndGuards<TGuard1, TGuard2, TGuard3>();

        /// <summary>Adds three guard types combined with logical AND.</summary>
        /// <typeparam name="TGuard1">Typed flow guard implementing <see cref="IFlowGuard&lt;TToken&gt;" />; return true to
        /// permit this flow.</typeparam>
        /// <typeparam name="TGuard2">Typed flow guard implementing <see cref="IFlowGuard&lt;TToken&gt;" />; return true to
        /// permit this flow.</typeparam>
        /// <typeparam name="TGuard3">Typed flow guard implementing <see cref="IFlowGuard&lt;TToken&gt;" />; return true to
        /// permit this flow.</typeparam>
        /// <returns>The current flow builder.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/activity-flows" />
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

        /// <summary>Adds four guard types combined with logical AND.</summary>
        /// <typeparam name="TGuard1">Typed flow guard implementing <see cref="IFlowGuard&lt;TToken&gt;" />; return true to
        /// permit this flow.</typeparam>
        /// <typeparam name="TGuard2">Typed flow guard implementing <see cref="IFlowGuard&lt;TToken&gt;" />; return true to
        /// permit this flow.</typeparam>
        /// <typeparam name="TGuard3">Typed flow guard implementing <see cref="IFlowGuard&lt;TToken&gt;" />; return true to
        /// permit this flow.</typeparam>
        /// <typeparam name="TGuard4">Typed flow guard implementing <see cref="IFlowGuard&lt;TToken&gt;" />; return true to
        /// permit this flow.</typeparam>
        /// <returns>The current flow builder.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/activity-flows" />
        TReturn AddGuards<TGuard1, TGuard2, TGuard3, TGuard4>()
            where TGuard1 : class, IFlowGuard<TToken>
            where TGuard2 : class, IFlowGuard<TToken>
            where TGuard3 : class, IFlowGuard<TToken>
            where TGuard4 : class, IFlowGuard<TToken>
            => AddAndGuards<TGuard1, TGuard2, TGuard3, TGuard4>();

        /// <summary>Adds four guard types combined with logical AND.</summary>
        /// <typeparam name="TGuard1">Typed flow guard implementing <see cref="IFlowGuard&lt;TToken&gt;" />; return true to
        /// permit this flow.</typeparam>
        /// <typeparam name="TGuard2">Typed flow guard implementing <see cref="IFlowGuard&lt;TToken&gt;" />; return true to
        /// permit this flow.</typeparam>
        /// <typeparam name="TGuard3">Typed flow guard implementing <see cref="IFlowGuard&lt;TToken&gt;" />; return true to
        /// permit this flow.</typeparam>
        /// <typeparam name="TGuard4">Typed flow guard implementing <see cref="IFlowGuard&lt;TToken&gt;" />; return true to
        /// permit this flow.</typeparam>
        /// <returns>The current flow builder.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/activity-flows" />
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

        /// <summary>Adds five guard types combined with logical AND.</summary>
        /// <typeparam name="TGuard1">Typed flow guard implementing <see cref="IFlowGuard&lt;TToken&gt;" />; return true to
        /// permit this flow.</typeparam>
        /// <typeparam name="TGuard2">Typed flow guard implementing <see cref="IFlowGuard&lt;TToken&gt;" />; return true to
        /// permit this flow.</typeparam>
        /// <typeparam name="TGuard3">Typed flow guard implementing <see cref="IFlowGuard&lt;TToken&gt;" />; return true to
        /// permit this flow.</typeparam>
        /// <typeparam name="TGuard4">Typed flow guard implementing <see cref="IFlowGuard&lt;TToken&gt;" />; return true to
        /// permit this flow.</typeparam>
        /// <typeparam name="TGuard5">Typed flow guard implementing <see cref="IFlowGuard&lt;TToken&gt;" />; return true to
        /// permit this flow.</typeparam>
        /// <returns>The current flow builder.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/activity-flows" />
        TReturn AddGuards<TGuard1, TGuard2, TGuard3, TGuard4, TGuard5>()
            where TGuard1 : class, IFlowGuard<TToken>
            where TGuard2 : class, IFlowGuard<TToken>
            where TGuard3 : class, IFlowGuard<TToken>
            where TGuard4 : class, IFlowGuard<TToken>
            where TGuard5 : class, IFlowGuard<TToken>
            => AddAndGuards<TGuard1, TGuard2, TGuard3, TGuard4, TGuard5>();

        /// <summary>Adds five guard types combined with logical AND.</summary>
        /// <typeparam name="TGuard1">Typed flow guard implementing <see cref="IFlowGuard&lt;TToken&gt;" />; return true to
        /// permit this flow.</typeparam>
        /// <typeparam name="TGuard2">Typed flow guard implementing <see cref="IFlowGuard&lt;TToken&gt;" />; return true to
        /// permit this flow.</typeparam>
        /// <typeparam name="TGuard3">Typed flow guard implementing <see cref="IFlowGuard&lt;TToken&gt;" />; return true to
        /// permit this flow.</typeparam>
        /// <typeparam name="TGuard4">Typed flow guard implementing <see cref="IFlowGuard&lt;TToken&gt;" />; return true to
        /// permit this flow.</typeparam>
        /// <typeparam name="TGuard5">Typed flow guard implementing <see cref="IFlowGuard&lt;TToken&gt;" />; return true to
        /// permit this flow.</typeparam>
        /// <returns>The current flow builder.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/activity-flows" />
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

        /// <summary>Adds two guard types combined with logical OR.</summary>
        /// <typeparam name="TGuard1">Typed flow guard implementing <see cref="IFlowGuard&lt;TToken&gt;" />; return true to
        /// permit this flow.</typeparam>
        /// <typeparam name="TGuard2">Typed flow guard implementing <see cref="IFlowGuard&lt;TToken&gt;" />; return true to
        /// permit this flow.</typeparam>
        /// <returns>The current flow builder.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/activity-flows" />
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

        /// <summary>Adds three guard types combined with logical OR.</summary>
        /// <typeparam name="TGuard1">Typed flow guard implementing <see cref="IFlowGuard&lt;TToken&gt;" />; return true to
        /// permit this flow.</typeparam>
        /// <typeparam name="TGuard2">Typed flow guard implementing <see cref="IFlowGuard&lt;TToken&gt;" />; return true to
        /// permit this flow.</typeparam>
        /// <typeparam name="TGuard3">Typed flow guard implementing <see cref="IFlowGuard&lt;TToken&gt;" />; return true to
        /// permit this flow.</typeparam>
        /// <returns>The current flow builder.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/activity-flows" />
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

        /// <summary>Adds four guard types combined with logical OR.</summary>
        /// <typeparam name="TGuard1">Typed flow guard implementing <see cref="IFlowGuard&lt;TToken&gt;" />; return true to
        /// permit this flow.</typeparam>
        /// <typeparam name="TGuard2">Typed flow guard implementing <see cref="IFlowGuard&lt;TToken&gt;" />; return true to
        /// permit this flow.</typeparam>
        /// <typeparam name="TGuard3">Typed flow guard implementing <see cref="IFlowGuard&lt;TToken&gt;" />; return true to
        /// permit this flow.</typeparam>
        /// <typeparam name="TGuard4">Typed flow guard implementing <see cref="IFlowGuard&lt;TToken&gt;" />; return true to
        /// permit this flow.</typeparam>
        /// <returns>The current flow builder.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/activity-flows" />
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

        /// <summary>Adds five guard types combined with logical OR.</summary>
        /// <typeparam name="TGuard1">Typed flow guard implementing <see cref="IFlowGuard&lt;TToken&gt;" />; return true to
        /// permit this flow.</typeparam>
        /// <typeparam name="TGuard2">Typed flow guard implementing <see cref="IFlowGuard&lt;TToken&gt;" />; return true to
        /// permit this flow.</typeparam>
        /// <typeparam name="TGuard3">Typed flow guard implementing <see cref="IFlowGuard&lt;TToken&gt;" />; return true to
        /// permit this flow.</typeparam>
        /// <typeparam name="TGuard4">Typed flow guard implementing <see cref="IFlowGuard&lt;TToken&gt;" />; return true to
        /// permit this flow.</typeparam>
        /// <typeparam name="TGuard5">Typed flow guard implementing <see cref="IFlowGuard&lt;TToken&gt;" />; return true to
        /// permit this flow.</typeparam>
        /// <returns>The current flow builder.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/activity-flows" />
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

    /// <summary>Configures guard predicates on a control flow.</summary>
    /// <typeparam name="TReturn">Fluent builder type returned after this Activity element is configured.</typeparam>
    /// <remarks>Conjunctive guard helpers require all listed guard types to return true; disjunctive helpers require
    /// any one to return true.</remarks>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/activity-flows" />
    public interface IControlFlowBuilderBase<out TReturn>
    {
        /// <summary>Adds asynchronous guard callbacks to the control flow.</summary>
        /// <param name="guardsAsync">The guard callbacks; each must pass for the flow to be eligible.</param>
        /// <returns>The current flow builder.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/activity-flows" />
        TReturn AddGuard(params Func<IGuardContext, Task<bool>>[]  guardsAsync);

        /// <summary>Adds a guard implementation type to the control flow.</summary>
        /// <typeparam name="TGuard">Typed flow guard implementing <see cref="IControlFlowGuard" />; return true to permit
        /// this flow.</typeparam>
        /// <returns>The current flow builder.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/activity-flows" />
        TReturn AddGuard<TGuard>()
            where TGuard : class, IControlFlowGuard
            => AddGuard(async c => await (await ((BaseContext)c).Context.Executor.NodeScope.GetControlFlowAsync<TGuard>(c)).GuardAsync());

        /// <summary>Adds two guard types combined with logical AND.</summary>
        /// <typeparam name="TGuard1">Typed flow guard implementing <see cref="IControlFlowGuard" />; return true to permit
        /// this flow.</typeparam>
        /// <typeparam name="TGuard2">Typed flow guard implementing <see cref="IControlFlowGuard" />; return true to permit
        /// this flow.</typeparam>
        /// <returns>The current flow builder.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/activity-flows" />
        TReturn AddGuards<TGuard1, TGuard2>()
            where TGuard1 : class, IControlFlowGuard
            where TGuard2 : class, IControlFlowGuard
            => AddAndGuards<TGuard1, TGuard2>();

        /// <summary>Adds two guard types combined with logical AND.</summary>
        /// <typeparam name="TGuard1">Typed flow guard implementing <see cref="IControlFlowGuard" />; return true to permit
        /// this flow.</typeparam>
        /// <typeparam name="TGuard2">Typed flow guard implementing <see cref="IControlFlowGuard" />; return true to permit
        /// this flow.</typeparam>
        /// <returns>The current flow builder.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/activity-flows" />
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

        /// <summary>Adds three guard types combined with logical AND.</summary>
        /// <typeparam name="TGuard1">Typed flow guard implementing <see cref="IControlFlowGuard" />; return true to permit
        /// this flow.</typeparam>
        /// <typeparam name="TGuard2">Typed flow guard implementing <see cref="IControlFlowGuard" />; return true to permit
        /// this flow.</typeparam>
        /// <typeparam name="TGuard3">Typed flow guard implementing <see cref="IControlFlowGuard" />; return true to permit
        /// this flow.</typeparam>
        /// <returns>The current flow builder.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/activity-flows" />
        TReturn AddGuards<TGuard1, TGuard2, TGuard3>()
            where TGuard1 : class, IControlFlowGuard
            where TGuard2 : class, IControlFlowGuard
            where TGuard3 : class, IControlFlowGuard
            => AddAndGuards<TGuard1, TGuard2, TGuard3>();

        /// <summary>Adds three guard types combined with logical AND.</summary>
        /// <typeparam name="TGuard1">Typed flow guard implementing <see cref="IControlFlowGuard" />; return true to permit
        /// this flow.</typeparam>
        /// <typeparam name="TGuard2">Typed flow guard implementing <see cref="IControlFlowGuard" />; return true to permit
        /// this flow.</typeparam>
        /// <typeparam name="TGuard3">Typed flow guard implementing <see cref="IControlFlowGuard" />; return true to permit
        /// this flow.</typeparam>
        /// <returns>The current flow builder.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/activity-flows" />
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

        /// <summary>Adds four guard types combined with logical AND.</summary>
        /// <typeparam name="TGuard1">Typed flow guard implementing <see cref="IControlFlowGuard" />; return true to permit
        /// this flow.</typeparam>
        /// <typeparam name="TGuard2">Typed flow guard implementing <see cref="IControlFlowGuard" />; return true to permit
        /// this flow.</typeparam>
        /// <typeparam name="TGuard3">Typed flow guard implementing <see cref="IControlFlowGuard" />; return true to permit
        /// this flow.</typeparam>
        /// <typeparam name="TGuard4">Typed flow guard implementing <see cref="IControlFlowGuard" />; return true to permit
        /// this flow.</typeparam>
        /// <returns>The current flow builder.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/activity-flows" />
        TReturn AddGuards<TGuard1, TGuard2, TGuard3, TGuard4>()
            where TGuard1 : class, IControlFlowGuard
            where TGuard2 : class, IControlFlowGuard
            where TGuard3 : class, IControlFlowGuard
            where TGuard4 : class, IControlFlowGuard
            => AddAndGuards<TGuard1, TGuard2, TGuard3, TGuard4>();

        /// <summary>Adds four guard types combined with logical AND.</summary>
        /// <typeparam name="TGuard1">Typed flow guard implementing <see cref="IControlFlowGuard" />; return true to permit
        /// this flow.</typeparam>
        /// <typeparam name="TGuard2">Typed flow guard implementing <see cref="IControlFlowGuard" />; return true to permit
        /// this flow.</typeparam>
        /// <typeparam name="TGuard3">Typed flow guard implementing <see cref="IControlFlowGuard" />; return true to permit
        /// this flow.</typeparam>
        /// <typeparam name="TGuard4">Typed flow guard implementing <see cref="IControlFlowGuard" />; return true to permit
        /// this flow.</typeparam>
        /// <returns>The current flow builder.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/activity-flows" />
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

        /// <summary>Adds five guard types combined with logical AND.</summary>
        /// <typeparam name="TGuard1">Typed flow guard implementing <see cref="IControlFlowGuard" />; return true to permit
        /// this flow.</typeparam>
        /// <typeparam name="TGuard2">Typed flow guard implementing <see cref="IControlFlowGuard" />; return true to permit
        /// this flow.</typeparam>
        /// <typeparam name="TGuard3">Typed flow guard implementing <see cref="IControlFlowGuard" />; return true to permit
        /// this flow.</typeparam>
        /// <typeparam name="TGuard4">Typed flow guard implementing <see cref="IControlFlowGuard" />; return true to permit
        /// this flow.</typeparam>
        /// <typeparam name="TGuard5">Typed flow guard implementing <see cref="IControlFlowGuard" />; return true to permit
        /// this flow.</typeparam>
        /// <returns>The current flow builder.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/activity-flows" />
        TReturn AddGuards<TGuard1, TGuard2, TGuard3, TGuard4, TGuard5>()
            where TGuard1 : class, IControlFlowGuard
            where TGuard2 : class, IControlFlowGuard
            where TGuard3 : class, IControlFlowGuard
            where TGuard4 : class, IControlFlowGuard
            where TGuard5 : class, IControlFlowGuard
            => AddAndGuards<TGuard1, TGuard2, TGuard3, TGuard4, TGuard5>();

        /// <summary>Adds five guard types combined with logical AND.</summary>
        /// <typeparam name="TGuard1">Typed flow guard implementing <see cref="IControlFlowGuard" />; return true to permit
        /// this flow.</typeparam>
        /// <typeparam name="TGuard2">Typed flow guard implementing <see cref="IControlFlowGuard" />; return true to permit
        /// this flow.</typeparam>
        /// <typeparam name="TGuard3">Typed flow guard implementing <see cref="IControlFlowGuard" />; return true to permit
        /// this flow.</typeparam>
        /// <typeparam name="TGuard4">Typed flow guard implementing <see cref="IControlFlowGuard" />; return true to permit
        /// this flow.</typeparam>
        /// <typeparam name="TGuard5">Typed flow guard implementing <see cref="IControlFlowGuard" />; return true to permit
        /// this flow.</typeparam>
        /// <returns>The current flow builder.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/activity-flows" />
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

        /// <summary>Adds two guard types combined with logical OR.</summary>
        /// <typeparam name="TGuard1">Typed flow guard implementing <see cref="IControlFlowGuard" />; return true to permit
        /// this flow.</typeparam>
        /// <typeparam name="TGuard2">Typed flow guard implementing <see cref="IControlFlowGuard" />; return true to permit
        /// this flow.</typeparam>
        /// <returns>The current flow builder.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/activity-flows" />
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

        /// <summary>Adds three guard types combined with logical OR.</summary>
        /// <typeparam name="TGuard1">Typed flow guard implementing <see cref="IControlFlowGuard" />; return true to permit
        /// this flow.</typeparam>
        /// <typeparam name="TGuard2">Typed flow guard implementing <see cref="IControlFlowGuard" />; return true to permit
        /// this flow.</typeparam>
        /// <typeparam name="TGuard3">Typed flow guard implementing <see cref="IControlFlowGuard" />; return true to permit
        /// this flow.</typeparam>
        /// <returns>The current flow builder.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/activity-flows" />
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

        /// <summary>Adds four guard types combined with logical OR.</summary>
        /// <typeparam name="TGuard1">Typed flow guard implementing <see cref="IControlFlowGuard" />; return true to permit
        /// this flow.</typeparam>
        /// <typeparam name="TGuard2">Typed flow guard implementing <see cref="IControlFlowGuard" />; return true to permit
        /// this flow.</typeparam>
        /// <typeparam name="TGuard3">Typed flow guard implementing <see cref="IControlFlowGuard" />; return true to permit
        /// this flow.</typeparam>
        /// <typeparam name="TGuard4">Typed flow guard implementing <see cref="IControlFlowGuard" />; return true to permit
        /// this flow.</typeparam>
        /// <returns>The current flow builder.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/activity-flows" />
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

        /// <summary>Adds five guard types combined with logical OR.</summary>
        /// <typeparam name="TGuard1">Typed flow guard implementing <see cref="IControlFlowGuard" />; return true to permit
        /// this flow.</typeparam>
        /// <typeparam name="TGuard2">Typed flow guard implementing <see cref="IControlFlowGuard" />; return true to permit
        /// this flow.</typeparam>
        /// <typeparam name="TGuard3">Typed flow guard implementing <see cref="IControlFlowGuard" />; return true to permit
        /// this flow.</typeparam>
        /// <typeparam name="TGuard4">Typed flow guard implementing <see cref="IControlFlowGuard" />; return true to permit
        /// this flow.</typeparam>
        /// <typeparam name="TGuard5">Typed flow guard implementing <see cref="IControlFlowGuard" />; return true to permit
        /// this flow.</typeparam>
        /// <returns>The current flow builder.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/activity-flows" />
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

    /// <summary>Configures a control flow's guards and weight.</summary>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/activity-flows" />
    public interface IControlFlowBuilder :
        IControlFlowBuilderBase<IControlFlowBuilderWithWeight>,
        IFlowWeight<IControlFlowBuilderWithWeight>;

    /// <summary>Configures guards on a control flow after assigning its weight.</summary>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/activity-flows" />
    public interface IControlFlowBuilderWithWeight : 
        IControlFlowBuilderBase<IControlFlowBuilderWithWeight>;

    /// <summary>Configures the fallback control flow of a decision and its weight.</summary>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/activity-flows" />
    public interface IElseControlFlowBuilder : IFlowWeight;

    /// <summary>Continues configuring a fallback control flow after its weight is assigned.</summary>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/activity-flows" />
    public interface IElseControlFlowBuilderWithWeight;

    /// <summary>Sets the weight of a flow and returns a builder for continued configuration.</summary>
    /// <typeparam name="TReturn">Fluent builder type returned after this Activity element is configured.</typeparam>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/activity-flows" />
    public interface IFlowWeight<out TReturn>
    {
        /// <summary>Sets the flow's selection weight.</summary>
        /// <param name="weight">Flow selection weight used by the runtime when choosing among eligible outgoing flows.
        /// </param>
        /// <returns>A builder for continued configuration.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/activity-flows" />
        TReturn SetWeight(int weight);
    }

    /// <summary>Sets the weight of a flow without returning a continuation builder.</summary>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/activity-flows" />
    public interface IFlowWeight
    {
        /// <summary>Sets the flow's selection weight.</summary>
        /// <param name="weight">Flow selection weight used by the runtime when choosing among eligible outgoing flows.
        /// </param>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/activity-flows" />
        void SetWeight(int weight);
    }
}
