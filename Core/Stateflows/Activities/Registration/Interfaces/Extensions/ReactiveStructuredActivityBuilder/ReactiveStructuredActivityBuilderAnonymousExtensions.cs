using Stateflows.Common;
using Stateflows.Activities.Registration.Interfaces;

namespace Stateflows.Activities.Typed
{
    public static class ReactiveStructuredActivityBuilderAnonymousExtensions
    {
        public static IReactiveStructuredActivityBuilder AddStructuredActivity(this IReactiveStructuredActivityBuilder builder, ReactiveStructuredActivityBuildAction buildAction)
            => builder.AddStructuredActivity(ActivityNodeInfo<StructuredActivityNode>.Name, buildAction);
        
        public static IReactiveStructuredActivityBuilder AddParallelActivity<TParallelizationToken>(this IReactiveStructuredActivityBuilder builder, ParallelActivityBuildAction buildAction, int chunkSize = 1)
            where TParallelizationToken : Token, new()
            => builder.AddParallelActivity<TParallelizationToken>(ActivityNodeInfo<ParallelActivityNode<TParallelizationToken>>.Name, buildAction, chunkSize);

        public static IReactiveStructuredActivityBuilder AddIterativeActivity<TIterationToken>(this IReactiveStructuredActivityBuilder builder, IterativeActivityBuildAction buildAction, int chunkSize = 1)
            where TIterationToken : Token, new()
            => builder.AddIterativeActivity<TIterationToken>(ActivityNodeInfo<IterativeActivityNode<TIterationToken>>.Name, buildAction, chunkSize);
    }
}
