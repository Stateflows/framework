using Stateflows.Common;
using Stateflows.Activities.Registration.Interfaces;

namespace Stateflows.Activities.Typed
{
    public static class ReactiveStructuredActivityBuilderAnonymousExtensions
    {
        public static IReactiveStructuredActivityBuilder AddStructuredActivity(this IReactiveStructuredActivityBuilder builder, ReactiveStructuredActivityBuildAction buildAction)
            => builder.AddStructuredActivity(ActivityNodeInfo<StructuredActivityNode>.Name, buildAction);
        
        public static IReactiveStructuredActivityBuilder AddParallelActivity<TParallelizationToken>(this IReactiveStructuredActivityBuilder builder, ParallelActivityBuildAction buildAction)
            where TParallelizationToken : Token, new()
            => builder.AddParallelActivity<TParallelizationToken>(ActivityNodeInfo<StructuredActivity<TParallelizationToken>>.Name, buildAction);

        public static IReactiveStructuredActivityBuilder AddIterativeActivity<TIterationToken>(this IReactiveStructuredActivityBuilder builder, IterativeActivityBuildAction buildAction)
            where TIterationToken : Token, new()
            => builder.AddIterativeActivity<TIterationToken>(ActivityNodeInfo<StructuredActivity<TIterationToken>>.Name, buildAction);
    }
}
