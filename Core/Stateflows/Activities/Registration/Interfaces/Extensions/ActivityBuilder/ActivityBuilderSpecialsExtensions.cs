using System.Threading.Tasks;
using Stateflows.Activities.Registration.Interfaces;

namespace Stateflows.Activities
{
    public static class ActivityBuilderSpecialsExtensions
    {
        public static IActivityBuilder AddJoin(this IActivityBuilder builder, string joinNodeName, JoinBuilderAction joinBuildAction)
            => builder
                .AddAction(
                    joinNodeName,
                    c =>
                    {
                        c.PassAll();
                        return Task.CompletedTask;
                    },
                    b => joinBuildAction(b as IJoinBuilder)
                );

        public static IActivityBuilder AddFork(this IActivityBuilder builder, string forkNodeName, ForkBuilderAction joinBuildAction)
            => builder
                .AddAction(
                    forkNodeName,
                    c =>
                    {
                        c.PassAll();
                        return Task.CompletedTask;
                    },
                    b => joinBuildAction(b as IForkBuilder)
                );

        public static IActivityBuilder AddMerge(this IActivityBuilder builder, string mergeNodeName, MergeBuilderAction mergeBuildAction)
            => builder
                .AddAction(
                    mergeNodeName,
                    c =>
                    {
                        c.PassAll();
                        return Task.CompletedTask;
                    },
                    b => mergeBuildAction(b.SetOptions(NodeOptions.None) as IMergeBuilder)
                );

        //public static IActivityBuilder AddDecision(this IActivityBuilder builder, string joinNodeName, DecisionBuilderAction decisionBuildAction)
        //    => builder
        //        .AddAction(
        //            joinNodeName,
        //            async c => c.PassAll(),
        //            b => decisionBuildAction(b.SetOptions(NodeOptions.None) as IDecisionBuilder)
        //        );

        //public static IActivityBuilder AddTimeEvent<TTimeEvent>(this IActivityBuilder builder, string timeEventNodeName, DecisionBuilderAction timeEventBuildAction)
        //    where TTimeEvent : TimeEvent
        //    => builder
        //        .AddAction(
        //            timeEventNodeName,
        //            async c => { },
        //            b => timeEventBuildAction(b as IDecisionBuilder)
        //        );
    }
}
