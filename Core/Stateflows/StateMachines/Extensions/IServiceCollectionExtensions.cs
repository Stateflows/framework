using System;
using Microsoft.Extensions.DependencyInjection;
using Stateflows.Common;
using Stateflows.Common.Extensions;
using Stateflows.StateMachines.Events;

namespace Stateflows.StateMachines.Extensions
{
    internal static class IServiceCollectionExtensions
    {
        public static IServiceCollection RegisterStateMachine(this IServiceCollection services, Type stateMachineType)
            => services?.AddServiceType(stateMachineType);

        public static IServiceCollection RegisterState2<TState>(this IServiceCollection services)
            where TState : class, IBaseState
            => services?.AddServiceType<TState>();

        public static IServiceCollection RegisterTransition2<TTransition, TEvent>(this IServiceCollection services)
            where TTransition : class, IBaseTransition<TEvent>
            where TEvent : Event, new()
            => services?.AddServiceType<TTransition>();

        public static IServiceCollection RegisterElseTransition2<TElseTransition, TEvent>(this IServiceCollection services)
            where TElseTransition : class, ITransitionEffect<TEvent>
            where TEvent : Event, new()
            => services?.AddServiceType<TElseTransition>();

        public static IServiceCollection RegisterDefaultTransition2<TDefaultTransition>(this IServiceCollection services)
            where TDefaultTransition : class, IBaseDefaultTransition
            => services?.AddServiceType<TDefaultTransition>();

        public static IServiceCollection RegisterDefaultElseTransition2<TDefaultElseTransition>(this IServiceCollection services)
            where TDefaultElseTransition : class, IDefaultTransitionEffect
            => services?.AddServiceType<TDefaultElseTransition>();

        public static IServiceCollection RegisterExceptionHandler<TExceptionHandler>(this IServiceCollection services)
            where TExceptionHandler : class, IStateMachineExceptionHandler
            => services?.AddServiceType<TExceptionHandler>();

        public static IServiceCollection RegisterObserver<TObserver>(this IServiceCollection services)
            where TObserver : class, IStateMachineObserver
            => services?.AddServiceType<TObserver>();

        public static IServiceCollection RegisterInterceptor<TInterceptor>(this IServiceCollection services)
            where TInterceptor : class, IStateMachineInterceptor
            => services?.AddServiceType<TInterceptor>();
    }
}
