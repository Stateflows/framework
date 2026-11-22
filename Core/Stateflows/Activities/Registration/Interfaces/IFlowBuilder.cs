using System;
using System.Collections.Generic;
using Stateflows.Common;
using Stateflows.Activities.Registration.Builders;
using Stateflows.Activities.Models;
using System.Threading.Tasks;
using Stateflows.Activities.Context.Classes;
using Stateflows.Activities.Context.Interfaces;

namespace Stateflows.Activities.Registration.Interfaces
{
    /// <summary>Configures a typed object flow, including its guard, weight, and optional transformation.</summary>
    /// <typeparam name="TToken">Token type carried by the object flow.</typeparam>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/5.5.2.2.-Flows" />
    public interface IObjectFlowBuilder<out TToken> :
        IFlowWeight<IObjectFlowBuilderWithWeight<TToken>>,
        IObjectFlowGuardBuilderBase<TToken, IObjectFlowBuilder<TToken>>
    {
        /// <summary>Transforms each token transferred by this flow.</summary>
        /// <typeparam name="TTransformedToken">Token type produced by the object-flow transformation.</typeparam>
        /// <param name="transformationAsync">Asynchronous callback mapping the input token in the flow context to the
        /// output token.</param>
        /// <returns>A flow builder for the transformed token type.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/5.5.2.2.-Flows" />
        IObjectFlowBuilder<TTransformedToken> AddTransformation<TTransformedToken>(TransformationDelegateAsync<TToken, TTransformedToken> transformationAsync);

        /// <summary>Transforms each token using a registered transformation implementation.</summary>
        /// <typeparam name="TTransformedToken">Token type produced by the object-flow transformation.</typeparam>
        /// <typeparam name="TTransformation">Typed flow transformation implementing
        /// <see cref="IFlowTransformation{TToken, TTransformedToken}" />.</typeparam>
        /// <returns>A flow builder for the transformed token type.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/5.5.2.2.-Flows" />
        IObjectFlowBuilder<TTransformedToken> AddTransformation<TTransformedToken, TTransformation>()
            where TTransformation : class, IFlowTransformation<TToken, TTransformedToken>
            => RegistrationMetadata.Transformation(() => AddTransformation(async c =>
                await (await ((BaseContext)c).Context.Executor.NodeScope.GetObjectFlowAsync<TTransformation, TToken>(c)).TransformAsync(c.Token)
            ), typeof(TTransformation));

        /// <summary>Adds a transformation that converts the current token to a payload token carrying one of the given
        /// names. The transformed names become the names this flow delivers to its target.</summary>
        /// <typeparam name="TTransformedToken">Payload token type produced by the transformation.</typeparam>
        /// <param name="transformedNames">Names of the payload tokens the transformation may produce.</param>
        /// <param name="transformationAsync">Transformation delegate; it must return a payload whose name is one of
        /// <paramref name="transformedNames"/>, otherwise the flow fails with <see cref="ArgumentException"/>.</param>
        /// <returns>The flow builder for the transformed token type.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/4.6.-Payloads" />
        IObjectFlowBuilder<TTransformedToken> AddTransformation<TTransformedToken>(IReadOnlyList<string> transformedNames, TransformationDelegateAsync<TToken, TTransformedToken> transformationAsync)
            where TTransformedToken : Payload
            => PayloadFlowScope.With(transformedNames, nameof(transformedNames), () => AddTransformation(transformationAsync));

        /// <summary>Adds a transformation that converts the current token to a payload token with the given name.
        /// </summary>
        /// <typeparam name="TTransformedToken">Payload token type produced by the transformation.</typeparam>
        /// <param name="transformedName">Name of the payload token the transformation produces.</param>
        /// <param name="transformationAsync">Transformation delegate; it must return a payload named
        /// <paramref name="transformedName"/>.</param>
        /// <returns>The flow builder for the transformed token type.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/4.6.-Payloads" />
        IObjectFlowBuilder<TTransformedToken> AddTransformation<TTransformedToken>(string transformedName, TransformationDelegateAsync<TToken, TTransformedToken> transformationAsync)
            where TTransformedToken : Payload
            => AddTransformation([transformedName], transformationAsync);

        /// <summary>Adds a class-based transformation that converts the current token to a payload token carrying one
        /// of the given names.</summary>
        /// <typeparam name="TTransformedToken">Payload token type produced by the transformation.</typeparam>
        /// <typeparam name="TTransformation">Transformation class.</typeparam>
        /// <param name="transformedNames">Names of the payload tokens the transformation may produce.</param>
        /// <returns>The flow builder for the transformed token type.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/4.6.-Payloads" />
        IObjectFlowBuilder<TTransformedToken> AddTransformation<TTransformedToken, TTransformation>(IReadOnlyList<string> transformedNames)
            where TTransformedToken : Payload
            where TTransformation : class, IFlowTransformation<TToken, TTransformedToken>
            => PayloadFlowScope.With(transformedNames, nameof(transformedNames), () => AddTransformation<TTransformedToken, TTransformation>());

        /// <summary>Adds a class-based transformation that converts the current token to a payload token with the
        /// given name.</summary>
        /// <typeparam name="TTransformedToken">Payload token type produced by the transformation.</typeparam>
        /// <typeparam name="TTransformation">Transformation class.</typeparam>
        /// <param name="transformedName">Name of the payload token the transformation produces.</param>
        /// <returns>The flow builder for the transformed token type.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/4.6.-Payloads" />
        IObjectFlowBuilder<TTransformedToken> AddTransformation<TTransformedToken, TTransformation>(string transformedName)
            where TTransformedToken : Payload
            where TTransformation : class, IFlowTransformation<TToken, TTransformedToken>
            => AddTransformation<TTransformedToken, TTransformation>([transformedName]);
    }

    /// <summary>Configures a weighted typed object flow and its guard or transformation.</summary>
    /// <typeparam name="TToken">Token type carried by the object flow.</typeparam>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/5.5.2.2.-Flows" />
    public interface IObjectFlowBuilderWithWeight<out TToken> :
        IObjectFlowGuardBuilderBase<TToken, IObjectFlowBuilderWithWeight<TToken>>
    {
        /// <summary>Transforms each token transferred by this flow.</summary>
        /// <typeparam name="TTransformedToken">Token type produced by the object-flow transformation.</typeparam>
        /// <param name="transformationAsync">Asynchronous callback mapping the input token in the flow context to the
        /// output token.</param>
        /// <returns>A weighted flow builder for the transformed token type.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/5.5.2.2.-Flows" />
        IObjectFlowBuilderWithWeight<TTransformedToken> AddTransformation<TTransformedToken>(TransformationDelegateAsync<TToken, TTransformedToken> transformationAsync);

        /// <summary>Transforms each token using a registered transformation implementation.</summary>
        /// <typeparam name="TTransformedToken">Token type produced by the object-flow transformation.</typeparam>
        /// <typeparam name="TTransformation">Typed flow transformation implementing
        /// <see cref="IFlowTransformation{TToken, TTransformedToken}" />.</typeparam>
        /// <returns>A weighted flow builder for the transformed token type.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/5.5.2.2.-Flows" />
        IObjectFlowBuilderWithWeight<TTransformedToken> AddTransformation<TTransformedToken, TTransformation>()
            where TTransformation : class, IFlowTransformation<TToken, TTransformedToken>
            => RegistrationMetadata.Transformation(() => AddTransformation(async c
                => await (await ((BaseContext)c).Context.Executor.NodeScope.GetObjectFlowAsync<TTransformation, TToken>(c)).TransformAsync(c.Token)
            ), typeof(TTransformation));

        /// <summary>Adds a transformation that converts the current token to a payload token carrying one of the given
        /// names. The transformed names become the names this flow delivers to its target.</summary>
        /// <typeparam name="TTransformedToken">Payload token type produced by the transformation.</typeparam>
        /// <param name="transformedNames">Names of the payload tokens the transformation may produce.</param>
        /// <param name="transformationAsync">Transformation delegate; it must return a payload whose name is one of
        /// <paramref name="transformedNames"/>, otherwise the flow fails with <see cref="ArgumentException"/>.</param>
        /// <returns>The flow builder for the transformed token type.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/4.6.-Payloads" />
        IObjectFlowBuilderWithWeight<TTransformedToken> AddTransformation<TTransformedToken>(IReadOnlyList<string> transformedNames, TransformationDelegateAsync<TToken, TTransformedToken> transformationAsync)
            where TTransformedToken : Payload
            => PayloadFlowScope.With(transformedNames, nameof(transformedNames), () => AddTransformation(transformationAsync));

        /// <summary>Adds a transformation that converts the current token to a payload token with the given name.
        /// </summary>
        /// <typeparam name="TTransformedToken">Payload token type produced by the transformation.</typeparam>
        /// <param name="transformedName">Name of the payload token the transformation produces.</param>
        /// <param name="transformationAsync">Transformation delegate; it must return a payload named
        /// <paramref name="transformedName"/>.</param>
        /// <returns>The flow builder for the transformed token type.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/4.6.-Payloads" />
        IObjectFlowBuilderWithWeight<TTransformedToken> AddTransformation<TTransformedToken>(string transformedName, TransformationDelegateAsync<TToken, TTransformedToken> transformationAsync)
            where TTransformedToken : Payload
            => AddTransformation([transformedName], transformationAsync);

        /// <summary>Adds a class-based transformation that converts the current token to a payload token carrying one
        /// of the given names.</summary>
        /// <typeparam name="TTransformedToken">Payload token type produced by the transformation.</typeparam>
        /// <typeparam name="TTransformation">Transformation class.</typeparam>
        /// <param name="transformedNames">Names of the payload tokens the transformation may produce.</param>
        /// <returns>The flow builder for the transformed token type.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/4.6.-Payloads" />
        IObjectFlowBuilderWithWeight<TTransformedToken> AddTransformation<TTransformedToken, TTransformation>(IReadOnlyList<string> transformedNames)
            where TTransformedToken : Payload
            where TTransformation : class, IFlowTransformation<TToken, TTransformedToken>
            => PayloadFlowScope.With(transformedNames, nameof(transformedNames), () => AddTransformation<TTransformedToken, TTransformation>());

        /// <summary>Adds a class-based transformation that converts the current token to a payload token with the
        /// given name.</summary>
        /// <typeparam name="TTransformedToken">Payload token type produced by the transformation.</typeparam>
        /// <typeparam name="TTransformation">Transformation class.</typeparam>
        /// <param name="transformedName">Name of the payload token the transformation produces.</param>
        /// <returns>The flow builder for the transformed token type.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/4.6.-Payloads" />
        IObjectFlowBuilderWithWeight<TTransformedToken> AddTransformation<TTransformedToken, TTransformation>(string transformedName)
            where TTransformedToken : Payload
            where TTransformation : class, IFlowTransformation<TToken, TTransformedToken>
            => AddTransformation<TTransformedToken, TTransformation>([transformedName]);
    }

    /// <summary>Configures a weighted fallback object flow, including optional transformation.</summary>
    /// <typeparam name="TToken">Token type carried by the object flow.</typeparam>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/5.5.2.2.-Flows" />
    public interface IElseObjectFlowBuilderWithWeight<out TToken>
    {
        /// <summary>Transforms each token transferred by this fallback flow.</summary>
        /// <typeparam name="TTransformedToken">Token type produced by the object-flow transformation.</typeparam>
        /// <param name="transformationAsync">Asynchronous callback mapping the input token in the flow context to the
        /// output token.</param>
        /// <returns>A weighted fallback flow builder for the transformed token type.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/5.5.2.2.-Flows" />
        IElseObjectFlowBuilderWithWeight<TTransformedToken> AddTransformation<TTransformedToken>(TransformationDelegateAsync<TToken, TTransformedToken> transformationAsync);

        /// <summary>Transforms each token using a registered transformation implementation.</summary>
        /// <typeparam name="TTransformedToken">Token type produced by the object-flow transformation.</typeparam>
        /// <typeparam name="TTransformation">Typed flow transformation implementing
        /// <see cref="IFlowTransformation{TToken, TTransformedToken}" />.</typeparam>
        /// <returns>A weighted fallback flow builder for the transformed token type.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/5.5.2.2.-Flows" />
        IElseObjectFlowBuilderWithWeight<TTransformedToken> AddTransformation<TTransformedToken, TTransformation>()
            where TTransformation : class, IFlowTransformation<TToken, TTransformedToken>
            => RegistrationMetadata.Transformation(() => AddTransformation(async c
                => await (await ((BaseContext)c).Context.Executor.NodeScope.GetObjectFlowAsync<TTransformation, TToken>(c)).TransformAsync(c.Token)
            ), typeof(TTransformation));

        /// <summary>Adds a transformation that converts the current token to a payload token carrying one of the given
        /// names. The transformed names become the names this flow delivers to its target.</summary>
        /// <typeparam name="TTransformedToken">Payload token type produced by the transformation.</typeparam>
        /// <param name="transformedNames">Names of the payload tokens the transformation may produce.</param>
        /// <param name="transformationAsync">Transformation delegate; it must return a payload whose name is one of
        /// <paramref name="transformedNames"/>, otherwise the flow fails with <see cref="ArgumentException"/>.</param>
        /// <returns>The flow builder for the transformed token type.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/4.6.-Payloads" />
        IElseObjectFlowBuilderWithWeight<TTransformedToken> AddTransformation<TTransformedToken>(IReadOnlyList<string> transformedNames, TransformationDelegateAsync<TToken, TTransformedToken> transformationAsync)
            where TTransformedToken : Payload
            => PayloadFlowScope.With(transformedNames, nameof(transformedNames), () => AddTransformation(transformationAsync));

        /// <summary>Adds a transformation that converts the current token to a payload token with the given name.
        /// </summary>
        /// <typeparam name="TTransformedToken">Payload token type produced by the transformation.</typeparam>
        /// <param name="transformedName">Name of the payload token the transformation produces.</param>
        /// <param name="transformationAsync">Transformation delegate; it must return a payload named
        /// <paramref name="transformedName"/>.</param>
        /// <returns>The flow builder for the transformed token type.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/4.6.-Payloads" />
        IElseObjectFlowBuilderWithWeight<TTransformedToken> AddTransformation<TTransformedToken>(string transformedName, TransformationDelegateAsync<TToken, TTransformedToken> transformationAsync)
            where TTransformedToken : Payload
            => AddTransformation([transformedName], transformationAsync);

        /// <summary>Adds a class-based transformation that converts the current token to a payload token carrying one
        /// of the given names.</summary>
        /// <typeparam name="TTransformedToken">Payload token type produced by the transformation.</typeparam>
        /// <typeparam name="TTransformation">Transformation class.</typeparam>
        /// <param name="transformedNames">Names of the payload tokens the transformation may produce.</param>
        /// <returns>The flow builder for the transformed token type.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/4.6.-Payloads" />
        IElseObjectFlowBuilderWithWeight<TTransformedToken> AddTransformation<TTransformedToken, TTransformation>(IReadOnlyList<string> transformedNames)
            where TTransformedToken : Payload
            where TTransformation : class, IFlowTransformation<TToken, TTransformedToken>
            => PayloadFlowScope.With(transformedNames, nameof(transformedNames), () => AddTransformation<TTransformedToken, TTransformation>());

        /// <summary>Adds a class-based transformation that converts the current token to a payload token with the
        /// given name.</summary>
        /// <typeparam name="TTransformedToken">Payload token type produced by the transformation.</typeparam>
        /// <typeparam name="TTransformation">Transformation class.</typeparam>
        /// <param name="transformedName">Name of the payload token the transformation produces.</param>
        /// <returns>The flow builder for the transformed token type.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/4.6.-Payloads" />
        IElseObjectFlowBuilderWithWeight<TTransformedToken> AddTransformation<TTransformedToken, TTransformation>(string transformedName)
            where TTransformedToken : Payload
            where TTransformation : class, IFlowTransformation<TToken, TTransformedToken>
            => AddTransformation<TTransformedToken, TTransformation>([transformedName]);
    }

    /// <summary>Configures a fallback object flow, including its weight and optional transformation.</summary>
    /// <typeparam name="TToken">Token type carried by the object flow.</typeparam>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/5.5.2.2.-Flows" />
    public interface IElseObjectFlowBuilder<out TToken> :
        IFlowWeight<IElseObjectFlowBuilderWithWeight<TToken>>
    {
        /// <summary>Transforms each token transferred by this fallback flow.</summary>
        /// <typeparam name="TTransformedToken">Token type produced by the object-flow transformation.</typeparam>
        /// <param name="transformationAsync">Asynchronous callback mapping the input token in the flow context to the
        /// output token.</param>
        /// <returns>A fallback flow builder for the transformed token type.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/5.5.2.2.-Flows" />
        IElseObjectFlowBuilder<TTransformedToken> AddTransformation<TTransformedToken>(TransformationDelegateAsync<TToken, TTransformedToken> transformationAsync);

        /// <summary>Transforms each token using a registered transformation implementation.</summary>
        /// <typeparam name="TTransformedToken">Token type produced by the object-flow transformation.</typeparam>
        /// <typeparam name="TTransformation">Typed flow transformation implementing
        /// <see cref="IFlowTransformation{TToken, TTransformedToken}" />.</typeparam>
        /// <returns>A fallback flow builder for the transformed token type.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/5.5.2.2.-Flows" />
        IElseObjectFlowBuilder<TTransformedToken> AddTransformation<TTransformedToken, TTransformation>()
            where TTransformation : class, IFlowTransformation<TToken, TTransformedToken>
            => RegistrationMetadata.Transformation(() => AddTransformation(async c
                => await (await ((BaseContext)c).Context.Executor.NodeScope.GetObjectFlowAsync<TTransformation, TToken>(c)).TransformAsync(c.Token)
            ), typeof(TTransformation));

        /// <summary>Adds a transformation that converts the current token to a payload token carrying one of the given
        /// names. The transformed names become the names this flow delivers to its target.</summary>
        /// <typeparam name="TTransformedToken">Payload token type produced by the transformation.</typeparam>
        /// <param name="transformedNames">Names of the payload tokens the transformation may produce.</param>
        /// <param name="transformationAsync">Transformation delegate; it must return a payload whose name is one of
        /// <paramref name="transformedNames"/>, otherwise the flow fails with <see cref="ArgumentException"/>.</param>
        /// <returns>The flow builder for the transformed token type.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/4.6.-Payloads" />
        IElseObjectFlowBuilder<TTransformedToken> AddTransformation<TTransformedToken>(IReadOnlyList<string> transformedNames, TransformationDelegateAsync<TToken, TTransformedToken> transformationAsync)
            where TTransformedToken : Payload
            => PayloadFlowScope.With(transformedNames, nameof(transformedNames), () => AddTransformation(transformationAsync));

        /// <summary>Adds a transformation that converts the current token to a payload token with the given name.
        /// </summary>
        /// <typeparam name="TTransformedToken">Payload token type produced by the transformation.</typeparam>
        /// <param name="transformedName">Name of the payload token the transformation produces.</param>
        /// <param name="transformationAsync">Transformation delegate; it must return a payload named
        /// <paramref name="transformedName"/>.</param>
        /// <returns>The flow builder for the transformed token type.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/4.6.-Payloads" />
        IElseObjectFlowBuilder<TTransformedToken> AddTransformation<TTransformedToken>(string transformedName, TransformationDelegateAsync<TToken, TTransformedToken> transformationAsync)
            where TTransformedToken : Payload
            => AddTransformation([transformedName], transformationAsync);

        /// <summary>Adds a class-based transformation that converts the current token to a payload token carrying one
        /// of the given names.</summary>
        /// <typeparam name="TTransformedToken">Payload token type produced by the transformation.</typeparam>
        /// <typeparam name="TTransformation">Transformation class.</typeparam>
        /// <param name="transformedNames">Names of the payload tokens the transformation may produce.</param>
        /// <returns>The flow builder for the transformed token type.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/4.6.-Payloads" />
        IElseObjectFlowBuilder<TTransformedToken> AddTransformation<TTransformedToken, TTransformation>(IReadOnlyList<string> transformedNames)
            where TTransformedToken : Payload
            where TTransformation : class, IFlowTransformation<TToken, TTransformedToken>
            => PayloadFlowScope.With(transformedNames, nameof(transformedNames), () => AddTransformation<TTransformedToken, TTransformation>());

        /// <summary>Adds a class-based transformation that converts the current token to a payload token with the
        /// given name.</summary>
        /// <typeparam name="TTransformedToken">Payload token type produced by the transformation.</typeparam>
        /// <typeparam name="TTransformation">Transformation class.</typeparam>
        /// <param name="transformedName">Name of the payload token the transformation produces.</param>
        /// <returns>The flow builder for the transformed token type.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/4.6.-Payloads" />
        IElseObjectFlowBuilder<TTransformedToken> AddTransformation<TTransformedToken, TTransformation>(string transformedName)
            where TTransformedToken : Payload
            where TTransformation : class, IFlowTransformation<TToken, TTransformedToken>
            => AddTransformation<TTransformedToken, TTransformation>([transformedName]);
    }

    /// <summary>Configures guards for a typed object flow.</summary>
    /// <typeparam name="TToken">Token type carried by the object flow.</typeparam>
    /// <typeparam name="TReturn">Fluent builder type returned after this Activity element is configured.</typeparam>
    /// <remarks>Multiple guards combined by the AND methods must all return true for this flow to be eligible.
    /// </remarks>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/5.5.2.2.-Flows" />
    public interface IObjectFlowGuardBuilderBase<out TToken, out TReturn>
    {
        /// <summary>Adds one or more asynchronous guards evaluated for this token flow.</summary>
        /// <param name="guardAsync">Asynchronous predicate evaluated for each candidate token or control activation; true
        /// permits the flow.</param>
        /// <returns>The current flow builder.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/5.5.2.2.-Flows" />
        TReturn AddGuard(params Func<IGuardContext<TToken>, Task<bool>>[]  guardAsync);
        
        /// <summary>Adds a guard implementation type for this flow.</summary>
        /// <typeparam name="TGuard">Typed flow guard implementing <see cref="IFlowGuard&lt;TToken&gt;" />; return true to
        /// permit this flow.</typeparam>
        /// <returns>The current flow builder.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/5.5.2.2.-Flows" />
        TReturn AddGuard<TGuard>()
            where TGuard : class, IFlowGuard<TToken>
            => RegistrationMetadata.Guard(() => AddGuard(async c
                => await (await ((BaseContext)c).Context.Executor.NodeScope.GetObjectFlowAsync<TGuard, TToken>(c)).GuardAsync(c.Token)
            ), ActivityGuardKind.Leaf, typeof(TGuard));

        /// <summary>Adds two guard types combined with logical AND.</summary>
        /// <typeparam name="TGuard1">Typed flow guard implementing <see cref="IFlowGuard&lt;TToken&gt;" />; return true to
        /// permit this flow.</typeparam>
        /// <typeparam name="TGuard2">Typed flow guard implementing <see cref="IFlowGuard&lt;TToken&gt;" />; return true to
        /// permit this flow.</typeparam>
        /// <returns>The current flow builder.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/5.5.2.2.-Flows" />
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
        /// <seealso href="https://github.com/Stateflows/framework/wiki/5.5.2.2.-Flows" />
        TReturn AddAndGuards<TGuard1, TGuard2>()
            where TGuard1 : class, IFlowGuard<TToken>
            where TGuard2 : class, IFlowGuard<TToken>
            => RegistrationMetadata.Guard(() => AddGuard(async c =>
            {
                var executor = ((BaseContext)c).Context.Executor;
                var guard1 = await executor.NodeScope.GetObjectFlowAsync<TGuard1, TToken>(c);
                var guard2 = await executor.NodeScope.GetObjectFlowAsync<TGuard2, TToken>(c);

                return await RegistrationMetadata.ObserveGuardLeafAsync(c, 0, () => guard1.GuardAsync(c.Token)) && await RegistrationMetadata.ObserveGuardLeafAsync(c, 1, () => guard2.GuardAsync(c.Token));
            }), ActivityGuardKind.And, typeof(TGuard1), typeof(TGuard2));

        /// <summary>Adds three guard types combined with logical AND.</summary>
        /// <typeparam name="TGuard1">Typed flow guard implementing <see cref="IFlowGuard&lt;TToken&gt;" />; return true to
        /// permit this flow.</typeparam>
        /// <typeparam name="TGuard2">Typed flow guard implementing <see cref="IFlowGuard&lt;TToken&gt;" />; return true to
        /// permit this flow.</typeparam>
        /// <typeparam name="TGuard3">Typed flow guard implementing <see cref="IFlowGuard&lt;TToken&gt;" />; return true to
        /// permit this flow.</typeparam>
        /// <returns>The current flow builder.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/5.5.2.2.-Flows" />
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
        /// <seealso href="https://github.com/Stateflows/framework/wiki/5.5.2.2.-Flows" />
        TReturn AddAndGuards<TGuard1, TGuard2, TGuard3>()
            where TGuard1 : class, IFlowGuard<TToken>
            where TGuard2 : class, IFlowGuard<TToken>
            where TGuard3 : class, IFlowGuard<TToken>
            => RegistrationMetadata.Guard(() => AddGuard(async c =>
            {
                var executor = ((BaseContext)c).Context.Executor;
                var guard1 = await executor.NodeScope.GetObjectFlowAsync<TGuard1, TToken>(c);
                var guard2 = await executor.NodeScope.GetObjectFlowAsync<TGuard2, TToken>(c);
                var guard3 = await executor.NodeScope.GetObjectFlowAsync<TGuard3, TToken>(c);

                return await RegistrationMetadata.ObserveGuardLeafAsync(c, 0, () => guard1.GuardAsync(c.Token)) && await RegistrationMetadata.ObserveGuardLeafAsync(c, 1, () => guard2.GuardAsync(c.Token)) && await RegistrationMetadata.ObserveGuardLeafAsync(c, 2, () => guard3.GuardAsync(c.Token));
            }), ActivityGuardKind.And, typeof(TGuard1), typeof(TGuard2), typeof(TGuard3));

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
        /// <seealso href="https://github.com/Stateflows/framework/wiki/5.5.2.2.-Flows" />
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
        /// <seealso href="https://github.com/Stateflows/framework/wiki/5.5.2.2.-Flows" />
        TReturn AddAndGuards<TGuard1, TGuard2, TGuard3, TGuard4>()
            where TGuard1 : class, IFlowGuard<TToken>
            where TGuard2 : class, IFlowGuard<TToken>
            where TGuard3 : class, IFlowGuard<TToken>
            where TGuard4 : class, IFlowGuard<TToken>
            => RegistrationMetadata.Guard(() => AddGuard(async c =>
            {
                var executor = ((BaseContext)c).Context.Executor;
                var guard1 = await executor.NodeScope.GetObjectFlowAsync<TGuard1, TToken>(c);
                var guard2 = await executor.NodeScope.GetObjectFlowAsync<TGuard2, TToken>(c);
                var guard3 = await executor.NodeScope.GetObjectFlowAsync<TGuard3, TToken>(c);
                var guard4 = await executor.NodeScope.GetObjectFlowAsync<TGuard4, TToken>(c);

                return await RegistrationMetadata.ObserveGuardLeafAsync(c, 0, () => guard1.GuardAsync(c.Token)) && await RegistrationMetadata.ObserveGuardLeafAsync(c, 1, () => guard2.GuardAsync(c.Token)) && await RegistrationMetadata.ObserveGuardLeafAsync(c, 2, () => guard3.GuardAsync(c.Token)) && await RegistrationMetadata.ObserveGuardLeafAsync(c, 3, () => guard4.GuardAsync(c.Token));
            }), ActivityGuardKind.And, typeof(TGuard1), typeof(TGuard2), typeof(TGuard3), typeof(TGuard4));

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
        /// <seealso href="https://github.com/Stateflows/framework/wiki/5.5.2.2.-Flows" />
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
        /// <seealso href="https://github.com/Stateflows/framework/wiki/5.5.2.2.-Flows" />
        TReturn AddAndGuards<TGuard1, TGuard2, TGuard3, TGuard4, TGuard5>()
            where TGuard1 : class, IFlowGuard<TToken>
            where TGuard2 : class, IFlowGuard<TToken>
            where TGuard3 : class, IFlowGuard<TToken>
            where TGuard4 : class, IFlowGuard<TToken>
            where TGuard5 : class, IFlowGuard<TToken>
            => RegistrationMetadata.Guard(() => AddGuard(async c =>
            {
                var executor = ((BaseContext)c).Context.Executor;
                var guard1 = await executor.NodeScope.GetObjectFlowAsync<TGuard1, TToken>(c);
                var guard2 = await executor.NodeScope.GetObjectFlowAsync<TGuard2, TToken>(c);
                var guard3 = await executor.NodeScope.GetObjectFlowAsync<TGuard3, TToken>(c);
                var guard4 = await executor.NodeScope.GetObjectFlowAsync<TGuard4, TToken>(c);
                var guard5 = await executor.NodeScope.GetObjectFlowAsync<TGuard5, TToken>(c);

                return await RegistrationMetadata.ObserveGuardLeafAsync(c, 0, () => guard1.GuardAsync(c.Token)) && await RegistrationMetadata.ObserveGuardLeafAsync(c, 1, () => guard2.GuardAsync(c.Token)) && await RegistrationMetadata.ObserveGuardLeafAsync(c, 2, () => guard3.GuardAsync(c.Token)) && await RegistrationMetadata.ObserveGuardLeafAsync(c, 3, () => guard4.GuardAsync(c.Token)) && await RegistrationMetadata.ObserveGuardLeafAsync(c, 4, () => guard5.GuardAsync(c.Token));
            }), ActivityGuardKind.And, typeof(TGuard1), typeof(TGuard2), typeof(TGuard3), typeof(TGuard4), typeof(TGuard5));

        /// <summary>Adds two guard types combined with logical OR.</summary>
        /// <typeparam name="TGuard1">Typed flow guard implementing <see cref="IFlowGuard&lt;TToken&gt;" />; return true to
        /// permit this flow.</typeparam>
        /// <typeparam name="TGuard2">Typed flow guard implementing <see cref="IFlowGuard&lt;TToken&gt;" />; return true to
        /// permit this flow.</typeparam>
        /// <returns>The current flow builder.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/5.5.2.2.-Flows" />
        TReturn AddOrGuards<TGuard1, TGuard2>()
            where TGuard1 : class, IFlowGuard<TToken>
            where TGuard2 : class, IFlowGuard<TToken>
            => RegistrationMetadata.Guard(() => AddGuard(async c =>
            {
                var executor = ((BaseContext)c).Context.Executor;
                var guard1 = await executor.NodeScope.GetObjectFlowAsync<TGuard1, TToken>(c);
                var guard2 = await executor.NodeScope.GetObjectFlowAsync<TGuard2, TToken>(c);

                return await RegistrationMetadata.ObserveGuardLeafAsync(c, 0, () => guard1.GuardAsync(c.Token)) || await RegistrationMetadata.ObserveGuardLeafAsync(c, 1, () => guard2.GuardAsync(c.Token));
            }), ActivityGuardKind.Or, typeof(TGuard1), typeof(TGuard2));

        /// <summary>Adds three guard types combined with logical OR.</summary>
        /// <typeparam name="TGuard1">Typed flow guard implementing <see cref="IFlowGuard&lt;TToken&gt;" />; return true to
        /// permit this flow.</typeparam>
        /// <typeparam name="TGuard2">Typed flow guard implementing <see cref="IFlowGuard&lt;TToken&gt;" />; return true to
        /// permit this flow.</typeparam>
        /// <typeparam name="TGuard3">Typed flow guard implementing <see cref="IFlowGuard&lt;TToken&gt;" />; return true to
        /// permit this flow.</typeparam>
        /// <returns>The current flow builder.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/5.5.2.2.-Flows" />
        TReturn AddOrGuards<TGuard1, TGuard2, TGuard3>()
            where TGuard1 : class, IFlowGuard<TToken>
            where TGuard2 : class, IFlowGuard<TToken>
            where TGuard3 : class, IFlowGuard<TToken>
            => RegistrationMetadata.Guard(() => AddGuard(async c =>
            {
                var executor = ((BaseContext)c).Context.Executor;
                var guard1 = await executor.NodeScope.GetObjectFlowAsync<TGuard1, TToken>(c);
                var guard2 = await executor.NodeScope.GetObjectFlowAsync<TGuard2, TToken>(c);
                var guard3 = await executor.NodeScope.GetObjectFlowAsync<TGuard3, TToken>(c);

                return await RegistrationMetadata.ObserveGuardLeafAsync(c, 0, () => guard1.GuardAsync(c.Token)) || await RegistrationMetadata.ObserveGuardLeafAsync(c, 1, () => guard2.GuardAsync(c.Token)) || await RegistrationMetadata.ObserveGuardLeafAsync(c, 2, () => guard3.GuardAsync(c.Token));
            }), ActivityGuardKind.Or, typeof(TGuard1), typeof(TGuard2), typeof(TGuard3));

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
        /// <seealso href="https://github.com/Stateflows/framework/wiki/5.5.2.2.-Flows" />
        TReturn AddOrGuards<TGuard1, TGuard2, TGuard3, TGuard4>()
            where TGuard1 : class, IFlowGuard<TToken>
            where TGuard2 : class, IFlowGuard<TToken>
            where TGuard3 : class, IFlowGuard<TToken>
            where TGuard4 : class, IFlowGuard<TToken>
            => RegistrationMetadata.Guard(() => AddGuard(async c =>
            {
                var executor = ((BaseContext)c).Context.Executor;
                var guard1 = await executor.NodeScope.GetObjectFlowAsync<TGuard1, TToken>(c);
                var guard2 = await executor.NodeScope.GetObjectFlowAsync<TGuard2, TToken>(c);
                var guard3 = await executor.NodeScope.GetObjectFlowAsync<TGuard3, TToken>(c);
                var guard4 = await executor.NodeScope.GetObjectFlowAsync<TGuard4, TToken>(c);

                return await RegistrationMetadata.ObserveGuardLeafAsync(c, 0, () => guard1.GuardAsync(c.Token)) || await RegistrationMetadata.ObserveGuardLeafAsync(c, 1, () => guard2.GuardAsync(c.Token)) || await RegistrationMetadata.ObserveGuardLeafAsync(c, 2, () => guard3.GuardAsync(c.Token)) || await RegistrationMetadata.ObserveGuardLeafAsync(c, 3, () => guard4.GuardAsync(c.Token));
            }), ActivityGuardKind.Or, typeof(TGuard1), typeof(TGuard2), typeof(TGuard3), typeof(TGuard4));

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
        /// <seealso href="https://github.com/Stateflows/framework/wiki/5.5.2.2.-Flows" />
        TReturn AddOrGuards<TGuard1, TGuard2, TGuard3, TGuard4, TGuard5>()
            where TGuard1 : class, IFlowGuard<TToken>
            where TGuard2 : class, IFlowGuard<TToken>
            where TGuard3 : class, IFlowGuard<TToken>
            where TGuard4 : class, IFlowGuard<TToken>
            where TGuard5 : class, IFlowGuard<TToken>
            => RegistrationMetadata.Guard(() => AddGuard(async c =>
            {
                var executor = ((BaseContext)c).Context.Executor;
                var guard1 = await executor.NodeScope.GetObjectFlowAsync<TGuard1, TToken>(c);
                var guard2 = await executor.NodeScope.GetObjectFlowAsync<TGuard2, TToken>(c);
                var guard3 = await executor.NodeScope.GetObjectFlowAsync<TGuard3, TToken>(c);
                var guard4 = await executor.NodeScope.GetObjectFlowAsync<TGuard4, TToken>(c);
                var guard5 = await executor.NodeScope.GetObjectFlowAsync<TGuard5, TToken>(c);

                return await RegistrationMetadata.ObserveGuardLeafAsync(c, 0, () => guard1.GuardAsync(c.Token)) || await RegistrationMetadata.ObserveGuardLeafAsync(c, 1, () => guard2.GuardAsync(c.Token)) || await RegistrationMetadata.ObserveGuardLeafAsync(c, 2, () => guard3.GuardAsync(c.Token)) || await RegistrationMetadata.ObserveGuardLeafAsync(c, 3, () => guard4.GuardAsync(c.Token)) || await RegistrationMetadata.ObserveGuardLeafAsync(c, 4, () => guard5.GuardAsync(c.Token));
            }), ActivityGuardKind.Or, typeof(TGuard1), typeof(TGuard2), typeof(TGuard3), typeof(TGuard4), typeof(TGuard5));
    }

    /// <summary>Configures guard predicates on a control flow.</summary>
    /// <typeparam name="TReturn">Fluent builder type returned after this Activity element is configured.</typeparam>
    /// <remarks>Conjunctive guard helpers require all listed guard types to return true; disjunctive helpers require
    /// any one to return true.</remarks>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/5.5.2.2.-Flows" />
    public interface IControlFlowBuilderBase<out TReturn>
    {
        /// <summary>Adds asynchronous guard callbacks to the control flow.</summary>
        /// <param name="guardsAsync">The guard callbacks; each must pass for the flow to be eligible.</param>
        /// <returns>The current flow builder.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/5.5.2.2.-Flows" />
        TReturn AddGuard(params Func<IGuardContext, Task<bool>>[]  guardsAsync);

        /// <summary>Adds a guard implementation type to the control flow.</summary>
        /// <typeparam name="TGuard">Typed flow guard implementing <see cref="IControlFlowGuard" />; return true to permit
        /// this flow.</typeparam>
        /// <returns>The current flow builder.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/5.5.2.2.-Flows" />
        TReturn AddGuard<TGuard>()
            where TGuard : class, IControlFlowGuard
            => RegistrationMetadata.Guard(() => AddGuard(async c => await (await ((BaseContext)c).Context.Executor.NodeScope.GetControlFlowAsync<TGuard>(c)).GuardAsync()), ActivityGuardKind.Leaf, typeof(TGuard));

        /// <summary>Adds two guard types combined with logical AND.</summary>
        /// <typeparam name="TGuard1">Typed flow guard implementing <see cref="IControlFlowGuard" />; return true to permit
        /// this flow.</typeparam>
        /// <typeparam name="TGuard2">Typed flow guard implementing <see cref="IControlFlowGuard" />; return true to permit
        /// this flow.</typeparam>
        /// <returns>The current flow builder.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/5.5.2.2.-Flows" />
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
        /// <seealso href="https://github.com/Stateflows/framework/wiki/5.5.2.2.-Flows" />
        TReturn AddAndGuards<TGuard1, TGuard2>()
            where TGuard1 : class, IControlFlowGuard
            where TGuard2 : class, IControlFlowGuard
            => RegistrationMetadata.Guard(() => AddGuard(async c =>
            {
                var executor = ((BaseContext)c).Context.Executor;
                var guard1 = await executor.NodeScope.GetControlFlowAsync<TGuard1>(c);
                var guard2 = await executor.NodeScope.GetControlFlowAsync<TGuard2>(c);

                return await RegistrationMetadata.ObserveGuardLeafAsync(c, 0, () => guard1.GuardAsync()) && await RegistrationMetadata.ObserveGuardLeafAsync(c, 1, () => guard2.GuardAsync());
            }), ActivityGuardKind.And, typeof(TGuard1), typeof(TGuard2));

        /// <summary>Adds three guard types combined with logical AND.</summary>
        /// <typeparam name="TGuard1">Typed flow guard implementing <see cref="IControlFlowGuard" />; return true to permit
        /// this flow.</typeparam>
        /// <typeparam name="TGuard2">Typed flow guard implementing <see cref="IControlFlowGuard" />; return true to permit
        /// this flow.</typeparam>
        /// <typeparam name="TGuard3">Typed flow guard implementing <see cref="IControlFlowGuard" />; return true to permit
        /// this flow.</typeparam>
        /// <returns>The current flow builder.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/5.5.2.2.-Flows" />
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
        /// <seealso href="https://github.com/Stateflows/framework/wiki/5.5.2.2.-Flows" />
        TReturn AddAndGuards<TGuard1, TGuard2, TGuard3>()
            where TGuard1 : class, IControlFlowGuard
            where TGuard2 : class, IControlFlowGuard
            where TGuard3 : class, IControlFlowGuard
            => RegistrationMetadata.Guard(() => AddGuard(async c =>
            {
                var executor = ((BaseContext)c).Context.Executor;
                var guard1 = await executor.NodeScope.GetControlFlowAsync<TGuard1>(c);
                var guard2 = await executor.NodeScope.GetControlFlowAsync<TGuard2>(c);
                var guard3 = await executor.NodeScope.GetControlFlowAsync<TGuard3>(c);

                return await RegistrationMetadata.ObserveGuardLeafAsync(c, 0, () => guard1.GuardAsync()) && await RegistrationMetadata.ObserveGuardLeafAsync(c, 1, () => guard2.GuardAsync()) && await RegistrationMetadata.ObserveGuardLeafAsync(c, 2, () => guard3.GuardAsync());
            }), ActivityGuardKind.And, typeof(TGuard1), typeof(TGuard2), typeof(TGuard3));

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
        /// <seealso href="https://github.com/Stateflows/framework/wiki/5.5.2.2.-Flows" />
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
        /// <seealso href="https://github.com/Stateflows/framework/wiki/5.5.2.2.-Flows" />
        TReturn AddAndGuards<TGuard1, TGuard2, TGuard3, TGuard4>()
            where TGuard1 : class, IControlFlowGuard
            where TGuard2 : class, IControlFlowGuard
            where TGuard3 : class, IControlFlowGuard
            where TGuard4 : class, IControlFlowGuard
            => RegistrationMetadata.Guard(() => AddGuard(async c =>
            {
                var executor = ((BaseContext)c).Context.Executor;
                var guard1 = await executor.NodeScope.GetControlFlowAsync<TGuard1>(c);
                var guard2 = await executor.NodeScope.GetControlFlowAsync<TGuard2>(c);
                var guard3 = await executor.NodeScope.GetControlFlowAsync<TGuard3>(c);
                var guard4 = await executor.NodeScope.GetControlFlowAsync<TGuard4>(c);

                return await RegistrationMetadata.ObserveGuardLeafAsync(c, 0, () => guard1.GuardAsync()) && await RegistrationMetadata.ObserveGuardLeafAsync(c, 1, () => guard2.GuardAsync()) && await RegistrationMetadata.ObserveGuardLeafAsync(c, 2, () => guard3.GuardAsync()) && await RegistrationMetadata.ObserveGuardLeafAsync(c, 3, () => guard4.GuardAsync());
            }), ActivityGuardKind.And, typeof(TGuard1), typeof(TGuard2), typeof(TGuard3), typeof(TGuard4));

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
        /// <seealso href="https://github.com/Stateflows/framework/wiki/5.5.2.2.-Flows" />
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
        /// <seealso href="https://github.com/Stateflows/framework/wiki/5.5.2.2.-Flows" />
        TReturn AddAndGuards<TGuard1, TGuard2, TGuard3, TGuard4, TGuard5>()
            where TGuard1 : class, IControlFlowGuard
            where TGuard2 : class, IControlFlowGuard
            where TGuard3 : class, IControlFlowGuard
            where TGuard4 : class, IControlFlowGuard
            where TGuard5 : class, IControlFlowGuard
            => RegistrationMetadata.Guard(() => AddGuard(async c =>
            {
                var executor = ((BaseContext)c).Context.Executor;
                var guard1 = await executor.NodeScope.GetControlFlowAsync<TGuard1>(c);
                var guard2 = await executor.NodeScope.GetControlFlowAsync<TGuard2>(c);
                var guard3 = await executor.NodeScope.GetControlFlowAsync<TGuard3>(c);
                var guard4 = await executor.NodeScope.GetControlFlowAsync<TGuard4>(c);
                var guard5 = await executor.NodeScope.GetControlFlowAsync<TGuard5>(c);

                return await RegistrationMetadata.ObserveGuardLeafAsync(c, 0, () => guard1.GuardAsync()) && await RegistrationMetadata.ObserveGuardLeafAsync(c, 1, () => guard2.GuardAsync()) && await RegistrationMetadata.ObserveGuardLeafAsync(c, 2, () => guard3.GuardAsync()) && await RegistrationMetadata.ObserveGuardLeafAsync(c, 3, () => guard4.GuardAsync()) && await RegistrationMetadata.ObserveGuardLeafAsync(c, 4, () => guard5.GuardAsync());
            }), ActivityGuardKind.And, typeof(TGuard1), typeof(TGuard2), typeof(TGuard3), typeof(TGuard4), typeof(TGuard5));

        /// <summary>Adds two guard types combined with logical OR.</summary>
        /// <typeparam name="TGuard1">Typed flow guard implementing <see cref="IControlFlowGuard" />; return true to permit
        /// this flow.</typeparam>
        /// <typeparam name="TGuard2">Typed flow guard implementing <see cref="IControlFlowGuard" />; return true to permit
        /// this flow.</typeparam>
        /// <returns>The current flow builder.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/5.5.2.2.-Flows" />
        TReturn AddOrGuards<TGuard1, TGuard2>()
            where TGuard1 : class, IControlFlowGuard
            where TGuard2 : class, IControlFlowGuard
            => RegistrationMetadata.Guard(() => AddGuard(async c =>
            {
                var executor = ((BaseContext)c).Context.Executor;
                var guard1 = await executor.NodeScope.GetControlFlowAsync<TGuard1>(c);
                var guard2 = await executor.NodeScope.GetControlFlowAsync<TGuard2>(c);

                return await RegistrationMetadata.ObserveGuardLeafAsync(c, 0, () => guard1.GuardAsync()) || await RegistrationMetadata.ObserveGuardLeafAsync(c, 1, () => guard2.GuardAsync());
            }), ActivityGuardKind.Or, typeof(TGuard1), typeof(TGuard2));

        /// <summary>Adds three guard types combined with logical OR.</summary>
        /// <typeparam name="TGuard1">Typed flow guard implementing <see cref="IControlFlowGuard" />; return true to permit
        /// this flow.</typeparam>
        /// <typeparam name="TGuard2">Typed flow guard implementing <see cref="IControlFlowGuard" />; return true to permit
        /// this flow.</typeparam>
        /// <typeparam name="TGuard3">Typed flow guard implementing <see cref="IControlFlowGuard" />; return true to permit
        /// this flow.</typeparam>
        /// <returns>The current flow builder.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/5.5.2.2.-Flows" />
        TReturn AddOrGuards<TGuard1, TGuard2, TGuard3>()
            where TGuard1 : class, IControlFlowGuard
            where TGuard2 : class, IControlFlowGuard
            where TGuard3 : class, IControlFlowGuard
            => RegistrationMetadata.Guard(() => AddGuard(async c =>
            {
                var executor = ((BaseContext)c).Context.Executor;
                var guard1 = await executor.NodeScope.GetControlFlowAsync<TGuard1>(c);
                var guard2 = await executor.NodeScope.GetControlFlowAsync<TGuard2>(c);
                var guard3 = await executor.NodeScope.GetControlFlowAsync<TGuard3>(c);

                return await RegistrationMetadata.ObserveGuardLeafAsync(c, 0, () => guard1.GuardAsync()) || await RegistrationMetadata.ObserveGuardLeafAsync(c, 1, () => guard2.GuardAsync()) || await RegistrationMetadata.ObserveGuardLeafAsync(c, 2, () => guard3.GuardAsync());
            }), ActivityGuardKind.Or, typeof(TGuard1), typeof(TGuard2), typeof(TGuard3));

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
        /// <seealso href="https://github.com/Stateflows/framework/wiki/5.5.2.2.-Flows" />
        TReturn AddOrGuards<TGuard1, TGuard2, TGuard3, TGuard4>()
            where TGuard1 : class, IControlFlowGuard
            where TGuard2 : class, IControlFlowGuard
            where TGuard3 : class, IControlFlowGuard
            where TGuard4 : class, IControlFlowGuard
            => RegistrationMetadata.Guard(() => AddGuard(async c =>
            {
                var executor = ((BaseContext)c).Context.Executor;
                var guard1 = await executor.NodeScope.GetControlFlowAsync<TGuard1>(c);
                var guard2 = await executor.NodeScope.GetControlFlowAsync<TGuard2>(c);
                var guard3 = await executor.NodeScope.GetControlFlowAsync<TGuard3>(c);
                var guard4 = await executor.NodeScope.GetControlFlowAsync<TGuard4>(c);

                return await RegistrationMetadata.ObserveGuardLeafAsync(c, 0, () => guard1.GuardAsync()) || await RegistrationMetadata.ObserveGuardLeafAsync(c, 1, () => guard2.GuardAsync()) || await RegistrationMetadata.ObserveGuardLeafAsync(c, 2, () => guard3.GuardAsync()) || await RegistrationMetadata.ObserveGuardLeafAsync(c, 3, () => guard4.GuardAsync());
            }), ActivityGuardKind.Or, typeof(TGuard1), typeof(TGuard2), typeof(TGuard3), typeof(TGuard4));

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
        /// <seealso href="https://github.com/Stateflows/framework/wiki/5.5.2.2.-Flows" />
        TReturn AddOrGuards<TGuard1, TGuard2, TGuard3, TGuard4, TGuard5>()
            where TGuard1 : class, IControlFlowGuard
            where TGuard2 : class, IControlFlowGuard
            where TGuard3 : class, IControlFlowGuard
            where TGuard4 : class, IControlFlowGuard
            where TGuard5 : class, IControlFlowGuard
            => RegistrationMetadata.Guard(() => AddGuard(async c =>
            {
                var executor = ((BaseContext)c).Context.Executor;
                var guard1 = await executor.NodeScope.GetControlFlowAsync<TGuard1>(c);
                var guard2 = await executor.NodeScope.GetControlFlowAsync<TGuard2>(c);
                var guard3 = await executor.NodeScope.GetControlFlowAsync<TGuard3>(c);
                var guard4 = await executor.NodeScope.GetControlFlowAsync<TGuard4>(c);
                var guard5 = await executor.NodeScope.GetControlFlowAsync<TGuard5>(c);

                return await RegistrationMetadata.ObserveGuardLeafAsync(c, 0, () => guard1.GuardAsync()) || await RegistrationMetadata.ObserveGuardLeafAsync(c, 1, () => guard2.GuardAsync()) || await RegistrationMetadata.ObserveGuardLeafAsync(c, 2, () => guard3.GuardAsync()) || await RegistrationMetadata.ObserveGuardLeafAsync(c, 3, () => guard4.GuardAsync()) || await RegistrationMetadata.ObserveGuardLeafAsync(c, 4, () => guard5.GuardAsync());
            }), ActivityGuardKind.Or, typeof(TGuard1), typeof(TGuard2), typeof(TGuard3), typeof(TGuard4), typeof(TGuard5));
    }

    /// <summary>Configures a control flow's guards and weight.</summary>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/5.5.2.2.-Flows" />
    public interface IControlFlowBuilder :
        IControlFlowBuilderBase<IControlFlowBuilderWithWeight>,
        IFlowWeight<IControlFlowBuilderWithWeight>;

    /// <summary>Configures guards on a control flow after assigning its weight.</summary>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/5.5.2.2.-Flows" />
    public interface IControlFlowBuilderWithWeight : 
        IControlFlowBuilderBase<IControlFlowBuilderWithWeight>;

    /// <summary>Configures the fallback control flow of a decision and its weight.</summary>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/5.5.2.2.-Flows" />
    public interface IElseControlFlowBuilder : IFlowWeight;

    /// <summary>Continues configuring a fallback control flow after its weight is assigned.</summary>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/5.5.2.2.-Flows" />
    public interface IElseControlFlowBuilderWithWeight;

    /// <summary>Sets the weight of a flow and returns a builder for continued configuration.</summary>
    /// <typeparam name="TReturn">Fluent builder type returned after this Activity element is configured.</typeparam>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/5.5.2.2.-Flows" />
    public interface IFlowWeight<out TReturn>
    {
        /// <summary>Sets the flow's selection weight.</summary>
        /// <param name="weight">Flow selection weight used by the runtime when choosing among eligible outgoing flows.
        /// </param>
        /// <returns>A builder for continued configuration.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/5.5.2.2.-Flows" />
        TReturn SetWeight(int weight);
    }

    /// <summary>Sets the weight of a flow without returning a continuation builder.</summary>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/5.5.2.2.-Flows" />
    public interface IFlowWeight
    {
        /// <summary>Sets the flow's selection weight.</summary>
        /// <param name="weight">Flow selection weight used by the runtime when choosing among eligible outgoing flows.
        /// </param>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/5.5.2.2.-Flows" />
        void SetWeight(int weight);
    }
}
