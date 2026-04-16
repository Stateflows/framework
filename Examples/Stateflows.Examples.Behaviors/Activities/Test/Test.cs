using Stateflows.Activities;
using Stateflows.Examples.Common.Events;

namespace Stateflows.Examples.Behaviors.Activities.Test;

public class Test : IActivity
{
    public static void Build(IActivityBuilder builder) => builder
        .AddAcceptEventAction<GetData>(b => b
            .AddControlFlow("getDataAction")
        )
        .AddAction(
            "getDataAction",
            async c =>
            {
                for (var i = 0; i < 10; i++)
                {
                    await Task.Delay(1000);
                    
                    c.Behavior.Publish(new DataNotification() { Data = "Lorem ipsum" });
                }
            }
        )
    ;
}