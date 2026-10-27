namespace Stateflows.Interfaces;

[Alias("Stateflows.Interfaces.IBehaviorGrainObserver")]
internal interface IBehaviorGrainObserver : IGrainObserver
{
    [Alias("NotifyAsync")]
    Task NotifyAsync(OrleansEventHolder[] notifications);
}