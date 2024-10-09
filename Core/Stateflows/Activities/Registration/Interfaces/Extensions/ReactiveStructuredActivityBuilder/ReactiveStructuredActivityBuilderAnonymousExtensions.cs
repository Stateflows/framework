using System.Diagnostics;
using Stateflows.Activities.Registration.Interfaces;

namespace Stateflows.Activities
{
    public static class ReactiveStructuredActivityBuilderAnonymousExtensions
    {
        [DebuggerHidden]
        public static IReactiveStructuredActivityBuilder AddStructuredActivity(this IReactiveStructuredActivityBuilder builder, ReactiveStructuredActivityBuildAction buildAction)
            => builder.AddStructuredActivity(StructuredActivityNode.Name, buildAction);
        
        [DebuggerHidden]
        public static IReactiveStructuredActivityBuilder AddParallelActivity<TParallelizationToken>(this IReactiveStructuredActivityBuilder builder, ParallelActivityBuildAction buildAction, int chunkSize = 1)
            => builder.AddParallelActivity<TParallelizationToken>(ParallelActivityNode<TParallelizationToken>.Name, buildAction, chunkSize);

        [DebuggerHidden]
        public static IReactiveStructuredActivityBuilder AddIterativeActivity<TIterationToken>(this IReactiveStructuredActivityBuilder builder, IterativeActivityBuildAction buildAction, int chunkSize = 1)
            => builder.AddIterativeActivity<TIterationToken>(IterativeActivityNode<TIterationToken>.Name, buildAction, chunkSize);
    }
}
