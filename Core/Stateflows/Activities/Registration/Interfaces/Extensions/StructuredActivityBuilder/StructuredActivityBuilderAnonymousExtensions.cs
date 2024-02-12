using Stateflows.Common;
using Stateflows.Activities.Registration.Interfaces;

namespace Stateflows.Activities.Typed
{
    public static class StructuredActivityBuilderAnonymousExtensions
    {
        public static IStructuredActivityBuilder AddStructuredActivity(this IStructuredActivityBuilder builder, StructuredActivityBuildAction buildAction)
            => builder.AddStructuredActivity(ActivityNodeInfo<StructuredActivityNode>.Name, buildAction);
        
        public static IStructuredActivityBuilder AddParallelActivity<TParallelizationToken>(this IStructuredActivityBuilder builder, ParallelActivityBuildAction buildAction)
            where TParallelizationToken : Token, new()
            => builder.AddParallelActivity<TParallelizationToken>(ActivityNodeInfo<ParallelActivityNode<TParallelizationToken>>.Name, buildAction);

        public static IStructuredActivityBuilder AddIterativeActivity<TIterationToken>(this IStructuredActivityBuilder builder, IterativeActivityBuildAction buildAction)
            where TIterationToken : Token, new()
            => builder.AddIterativeActivity<TIterationToken>(ActivityNodeInfo<IterativeActivityNode<TIterationToken>>.Name, buildAction);
    }
}
