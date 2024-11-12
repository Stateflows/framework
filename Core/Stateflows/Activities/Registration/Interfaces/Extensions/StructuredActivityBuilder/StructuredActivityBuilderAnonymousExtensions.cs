using System.Diagnostics;
using Stateflows.Activities.Registration.Interfaces;

namespace Stateflows.Activities
{
    public static class StructuredActivityBuilderAnonymousExtensions
    {
        [DebuggerHidden]
        public static IStructuredActivityBuilder AddStructuredActivity(this IStructuredActivityBuilder builder, StructuredActivityBuildAction buildAction)
            => builder.AddStructuredActivity(StructuredActivityNode.Name, buildAction);
        
        [DebuggerHidden]
        public static IStructuredActivityBuilder AddParallelActivity<TParallelizationToken>(this IStructuredActivityBuilder builder, ParallelActivityBuildAction buildAction, int chunkSize = 1)
            => builder.AddParallelActivity<TParallelizationToken>(ParallelActivityNode<TParallelizationToken>.Name, buildAction, chunkSize);

        [DebuggerHidden]
        public static IStructuredActivityBuilder AddIterativeActivity<TIterationToken>(this IStructuredActivityBuilder builder, IterativeActivityBuildAction buildAction, int chunkSize = 1)
            => builder.AddIterativeActivity<TIterationToken>(IterativeActivityNode<TIterationToken>.Name, buildAction, chunkSize);
    }
}
