using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Stateflows.Storage.EntityFrameworkCore.EntityFrameworkCore.Entities;

/// <summary>Entity Framework row storing one Stateflows behavior's serialized execution context.</summary>
[Table("StateflowsContexts_v1")]
[Index(nameof(BehaviorClass))]
[Index(nameof(BehaviorId), IsUnique = true)]
[Index(nameof(TriggerTime))]
[Index(nameof(TriggerOnStartup))]
public class Context_v1
{
    /// <summary>Gets or sets the database-generated row identifier.</summary>
    public int Id { get; set; }

    /// <summary>Gets or sets the serialized behavior class identity.</summary>
    [StringLength(1024)]
    public string BehaviorClass { get; set; }

    /// <summary>Gets or sets the serialized behavior instance identity.</summary>
    [StringLength(1024)]
    public string BehaviorId { get; set; }

    /// <summary>Gets or sets the next scheduled trigger time, when the behavior has one.</summary>
    public DateTime? TriggerTime { get; set; }

    /// <summary>Gets or sets whether the behavior should be triggered during startup.</summary>
    public bool TriggerOnStartup { get; set; }

    /// <summary>Gets or sets the serialized behavior context payload.</summary>
    public string Data { get; set; }

    /// <summary>Creates a persisted context row from its behavior identity and serialized execution data.</summary>
    /// <param name="behaviorClass">The serialized behavior class identity.</param>
    /// <param name="behaviorId">The serialized behavior instance identity.</param>
    /// <param name="triggerTime">The next scheduled trigger time, if any.</param>
    /// <param name="triggerOnStartup">Whether startup should trigger the behavior.</param>
    /// <param name="data">The serialized behavior context payload.</param>
    public Context_v1(string behaviorClass, string behaviorId, DateTime? triggerTime, bool triggerOnStartup, string data)
    {
        BehaviorClass = behaviorClass;
        BehaviorId = behaviorId;
        TriggerTime = triggerTime;
        TriggerOnStartup = triggerOnStartup;
        Data = data;
    }
}
