using System.Diagnostics;
using Stateflows.Activities.Registration.Interfaces;

namespace Stateflows.Activities.Typed
{
    public static class ActivityBuilderAnonymousExtensions
    {
        [DebuggerHidden]
        public static IActivityBuilder AddStructuredActivity(this IActivityBuilder builder, ReactiveStructuredActivityBuildAction buildAction)
            => builder.AddStructuredActivity(StructuredActivityNode.Name, buildAction);
        
        [DebuggerHidden]
        public static IActivityBuilder AddParallelActivity<TParallelizationToken>(this IActivityBuilder builder, ParallelActivityBuildAction buildAction, int chunkSize = 1)
            => builder.AddParallelActivity<TParallelizationToken>(ParallelActivityNode<TParallelizationToken>.Name, buildAction, chunkSize);

        [DebuggerHidden]
        public static IActivityBuilder AddIterativeActivity<TIterationToken>(this IActivityBuilder builder, IterativeActivityBuildAction buildAction, int chunkSize = 1)
            => builder.AddIterativeActivity<TIterationToken>(IterativeActivityNode<TIterationToken>.Name, buildAction, chunkSize);
    }
}
