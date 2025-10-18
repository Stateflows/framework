namespace Stateflows.Common.Engine.Interfaces;

public interface IStateflowsTelemetry
{
    public int EventQueueLength { get; }
    public int BehaviorExecutionsCount { get; }
    
}