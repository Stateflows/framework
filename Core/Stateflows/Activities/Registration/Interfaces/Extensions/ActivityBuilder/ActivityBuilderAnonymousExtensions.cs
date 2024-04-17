using Stateflows.Common;
using Stateflows.Activities.Registration.Interfaces;

namespace Stateflows.Activities.Typed
{
    public static class ActivityBuilderAnonymousExtensions
    {
        public static IActivityBuilder AddStructuredActivity(this IActivityBuilder builder, ReactiveStructuredActivityBuildAction buildAction)
            => builder.AddStructuredActivity(ActivityNodeInfo<StructuredActivityNode>.Name, buildAction);
        
        public static IActivityBuilder AddParallelActivity<TParallelizationToken>(this IActivityBuilder builder, ParallelActivityBuildAction buildAction, int chunkSize = 1)
            where TParallelizationToken : Token, new()
            => builder.AddParallelActivity<TParallelizationToken>(ActivityNodeInfo<ParallelActivityNode<TParallelizationToken>>.Name, buildAction, chunkSize);

        public static IActivityBuilder AddIterativeActivity<TIterationToken>(this IActivityBuilder builder, IterativeActivityBuildAction buildAction, int chunkSize = 1)
            where TIterationToken : Token, new()
            => builder.AddIterativeActivity<TIterationToken>(ActivityNodeInfo<IterativeActivityNode<TIterationToken>>.Name, buildAction, chunkSize);
    }
}
