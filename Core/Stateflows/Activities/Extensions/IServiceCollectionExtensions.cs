using System;
using Microsoft.Extensions.DependencyInjection;
using Stateflows.Common.Extensions;

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
            where TStructuredActivity : StructuredActivity
            => services?.AddServiceType<TStructuredActivity>();

        public static IServiceCollection RegisterExceptionHandler<TException, TExceptionHandler>(this IServiceCollection services)
            where TException : Exception
            where TExceptionHandler : ExceptionHandler<TException>
            => services?.AddServiceType<TExceptionHandler>();

        public static IServiceCollection RegisterObjectFlow<TObjectFlow, TToken>(this IServiceCollection services)
            where TObjectFlow : ObjectFlow<TToken>
            where TToken : Token, new()
            => services?.AddServiceType<TObjectFlow>();

        public static IServiceCollection RegisterObjectTransformationFlow<TObjectTransformationFlow, TToken, TTransformedToken>(this IServiceCollection services)
            where TObjectTransformationFlow : ObjectTransformationFlow<TToken, TTransformedToken>
            where TToken : Token, new()
            where TTransformedToken : Token, new ()
            => services?.AddServiceType<TObjectTransformationFlow>();

        public static IServiceCollection RegisterControlFlow<TControlFlow>(this IServiceCollection services)
            where TControlFlow : ControlFlow
            => services?.AddServiceType<TControlFlow>();

        //public static IServiceCollection RegisterExceptionHandler<TExceptionHandler>(this IServiceCollection services)
        //    where TExceptionHandler : class, IStateMachineExceptionHandler
        //    => services?.AddServiceType<TExceptionHandler>();

        //public static IServiceCollection RegisterObserver<TObserver>(this IServiceCollection services)
        //    where TObserver : class, IStateMachineObserver
        //    => services?.AddServiceType<TObserver>();

        //public static IServiceCollection RegisterInterceptor<TInterceptor>(this IServiceCollection services)
        //    where TInterceptor : class, IStateMachineInterceptor
        //    => services?.AddServiceType<TInterceptor>();
    }
}
