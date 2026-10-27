using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace Stateflows;

/// <summary>
/// Operational counters of the Stateflows Orleans runtime, published on the <see cref="MeterName"/> meter.
/// Tags are low-cardinality only (behavior class, operation, resource, reason); behavior instance ids are never used as tags.
/// </summary>
public static class StateflowsOrleansDiagnostics
{
    /// <summary>Gets the name of the meter used for Stateflows Orleans runtime counters.</summary>
    public const string MeterName = "Stateflows.Orleans";

    private static readonly Meter Meter = new(MeterName);

    private static readonly Counter<long> PersistenceFailures = Meter.CreateCounter<long>(
        "stateflows.orleans.persistence.failures", description: "Failed writes of Stateflows grain state.");

    private static readonly Counter<long> ReminderFailures = Meter.CreateCounter<long>(
        "stateflows.orleans.reminder.failures", description: "Failed reminder registrations, updates or removals.");

    private static readonly Counter<long> IndexFailures = Meter.CreateCounter<long>(
        "stateflows.orleans.index.failures", description: "Failed instance index synchronizations.");

    private static readonly Counter<long> PublicationFailures = Meter.CreateCounter<long>(
        "stateflows.orleans.publication.failures", description: "Failed notification publications or deliveries.");

    private static readonly Counter<long> AdmissionRejections = Meter.CreateCounter<long>(
        "stateflows.orleans.admission.rejections", description: "Behavior executions rejected with EventStatus.Overloaded.");

    private static readonly Counter<long> DiscoveryFailures = Meter.CreateCounter<long>(
        "stateflows.orleans.discovery.failures", description: "Failed retrievals of silo behavior capabilities.");

    /// <summary>Records a failed retrieval of silo behavior capabilities.</summary>
    public static void DiscoveryFailed()
        => DiscoveryFailures.Add(1);

    /// <summary>Records a failed write of grain state.</summary>
    /// <param name="stateName">The grain state name whose write failed.</param>
    /// <param name="behaviorClass">The affected behavior class, when known.</param>
    public static void PersistenceFailed(string stateName, BehaviorClass? behaviorClass = null)
        => PersistenceFailures.Add(1, Tags(("state", stateName), ("behavior.class", behaviorClass?.ToString())));

    /// <summary>Records a failed reminder operation.</summary>
    /// <param name="operation">The reminder operation that failed, such as registration, update, or removal.</param>
    /// <param name="behaviorClass">The affected behavior class, when known.</param>
    public static void ReminderFailed(string operation, BehaviorClass? behaviorClass = null)
        => ReminderFailures.Add(1, Tags(("operation", operation), ("behavior.class", behaviorClass?.ToString())));

    /// <summary>Records a failed synchronization of the behavior instance index.</summary>
    /// <param name="operation">The index operation that failed.</param>
    /// <param name="behaviorClass">The affected behavior class.</param>
    public static void IndexFailed(string operation, BehaviorClass behaviorClass)
        => IndexFailures.Add(1, Tags(("operation", operation), ("behavior.class", behaviorClass.ToString())));

    /// <summary>Records a failed notification publication or delivery.</summary>
    /// <param name="operation">The publication or delivery operation that failed.</param>
    public static void PublicationFailed(string operation)
        => PublicationFailures.Add(1, Tags(("operation", operation)));

    /// <summary>Records an execution rejected because a runtime resource is overloaded.</summary>
    /// <param name="resource">The constrained resource.</param>
    /// <param name="reason">The reason the execution was rejected.</param>
    /// <param name="behaviorClass">The affected behavior class.</param>
    public static void AdmissionRejected(string resource, string reason, BehaviorClass behaviorClass)
        => AdmissionRejections.Add(1, Tags(("resource", resource), ("reason", reason), ("behavior.class", behaviorClass.ToString())));

    private static TagList Tags(params (string Key, string? Value)[] tags)
    {
        var list = new TagList();
        foreach (var (key, value) in tags)
        {
            if (value != null)
            {
                list.Add(key, value);
            }
        }

        return list;
    }
}