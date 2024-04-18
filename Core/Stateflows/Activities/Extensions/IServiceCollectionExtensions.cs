using System;
using Microsoft.Extensions.DependencyInjection;
using Stateflows.Common.Extensions;
using Stateflows.Common;

namespace Stateflows.Activities.Extensions
{
    internal static class IServiceCollectionExtensions
    {
        public static IServiceCollection RegisterActivity<TActivity>(this IServiceCollection services)
            where TActivity : Activity
            => services?.AddServiceType<TActivity>();

        public static IServiceCollection RegisterActivity(this IServiceCollection services, Type activityType)
            => services?.AddServiceType(activityType);

        public static IServiceCollection RegisterAction<TAction>(this IServiceCollection services)
            where TAction : ActivityNode
            => services?.AddServiceType<TAction>();

        public static IServiceCollection RegisterStructuredActivity<TStructuredActivity>(this IServiceCollection services)
            where TStructuredActivity : StructuredActivityNode
            => services?.AddServiceType<TStructuredActivity>();

        public static IServiceCollection RegisterExceptionHandlerAction<TException, TExceptionHandler>(this IServiceCollection services)
            where TException : Exception
            where TExceptionHandler : ExceptionHandlerNode<TException>
            => services?.AddServiceType<TExceptionHandler>();

        public static IServiceCollection RegisterObjectFlow<TFlow, TToken>(this IServiceCollection services)
            where TFlow : Flow<TToken>
            // where TToken : Token, new()
            => services?.AddServiceType<TFlow>();

        public static IServiceCollection RegisterElseObjectFlow<TFlow, TToken>(this IServiceCollection services)
            where TFlow : ElseFlow<TToken>
            // where TToken : Token, new()
            => services?.AddServiceType<TFlow>();

        public static IServiceCollection RegisterObjectTransformationFlow<TObjectTransformationFlow, TToken, TTransformedToken>(this IServiceCollection services)
            where TObjectTransformationFlow : TransformationFlow<TToken, TTransformedToken>
            // where TToken : Token, new()
            ////where TTransformedToken : Token, new()
            => services?.AddServiceType<TObjectTransformationFlow>();

        public static IServiceCollection RegisterElseObjectTransformationFlow<TObjectTransformationFlow, TToken, TTransformedToken>(this IServiceCollection services)
            where TObjectTransformationFlow : ElseObjectTransformationFlow<TToken, TTransformedToken>
            // where TToken : Token, new()
            ////where TTransformedToken : Token, new()
            => services?.AddServiceType<TObjectTransformationFlow>();

        public static IServiceCollection RegisterControlFlow<TControlFlow>(this IServiceCollection services)
            where TControlFlow : ControlFlow
            => services?.AddServiceType<TControlFlow>();

        public static IServiceCollection RegisterElseControlFlow<TControlFlow>(this IServiceCollection services)
            where TControlFlow : ElseControlFlow
            => services?.AddServiceType<TControlFlow>();

        public static IServiceCollection RegisterObserver<TObserver>(this IServiceCollection services)
            where TObserver : class, IActivityObserver
            => services?.AddServiceType<TObserver>();

        public static IServiceCollection RegisterInterceptor<TInterceptor>(this IServiceCollection services)
            where TInterceptor : class, IActivityInterceptor
            => services?.AddServiceType<TInterceptor>();

        public static IServiceCollection RegisterExceptionHandler<TExceptionHandler>(this IServiceCollection services)
            where TExceptionHandler : class, IActivityExceptionHandler
            => services?.AddServiceType<TExceptionHandler>();
    }
}
