namespace Stateflows.Common;

/// <summary>Persists an Activity's values and marking without delivering input to its network.</summary>
/// <remarks>Supported by Orleans only. Waits for active firing bodies to reach the checkpoint barrier;
/// successful completion returns <see cref="EventStatus.Consumed"/>. Does not implicitly initialize the Activity.</remarks>
[NoImplicitInitialization]
public sealed class ActivityCheckpoint;
