using Microsoft.Extensions.DependencyInjection;
using Stateflows.Common;
using Stateflows.Common.Extensions;

namespace Stateflows.StateMachines.Extensions
{
    internal static class IServiceCollectionExtensions
    {
        public static IServiceCollection RegisterState<TState>(this IServiceCollection services)
            where TState : State
            => services.AddServiceType<TState>();

        public static IServiceCollection RegisterTransition<TTransition, TEvent>(this IServiceCollection services)
            where TTransition : Transition<TEvent>
            where TEvent : Event
            => services.AddServiceType<TTransition>();

        public static IServiceCollection RegisterExceptionHandler<TExceptionHandler>(this IServiceCollection services)
            where TExceptionHandler : class, IStateMachineExceptionHandler
            => services.AddServiceType<TExceptionHandler>();

        public static IServiceCollection RegisterObserver<TObserver>(this IServiceCollection services)
            where TObserver : class, IStateMachineObserver
            => services.AddServiceType<TObserver>();

        public static IServiceCollection RegisterInterceptor<TInterceptor>(this IServiceCollection services)
            where TInterceptor : class, IStateMachineInterceptor
            => services.AddServiceType<TInterceptor>();
    }
}
