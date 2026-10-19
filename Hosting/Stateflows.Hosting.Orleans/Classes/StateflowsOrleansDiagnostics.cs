using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace Stateflows;

/// <summary>
/// Operational counters of the Stateflows Orleans runtime, published on the <see cref="MeterName"/> meter.
/// Tags are low-cardinality only (behavior class, operation, resource, reason); behavior instance ids are never used as tags.
/// </summary>
public static class StateflowsOrleansDiagnostics
{
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

    public static void DiscoveryFailed()
        => DiscoveryFailures.Add(1);

    public static void PersistenceFailed(string stateName, BehaviorClass? behaviorClass = null)
        => PersistenceFailures.Add(1, Tags(("state", stateName), ("behavior.class", behaviorClass?.ToString())));

    public static void ReminderFailed(string operation, BehaviorClass? behaviorClass = null)
        => ReminderFailures.Add(1, Tags(("operation", operation), ("behavior.class", behaviorClass?.ToString())));

    public static void IndexFailed(string operation, BehaviorClass behaviorClass)
        => IndexFailures.Add(1, Tags(("operation", operation), ("behavior.class", behaviorClass.ToString())));

    public static void PublicationFailed(string operation)
        => PublicationFailures.Add(1, Tags(("operation", operation)));

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