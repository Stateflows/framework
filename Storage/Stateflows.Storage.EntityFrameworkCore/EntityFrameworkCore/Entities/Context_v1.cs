using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Stateflows.Storage.EntityFrameworkCore.EntityFrameworkCore.Entities;

[Table("StateflowsContexts_v1")]
[Index(nameof(BehaviorClass))]
[Index(nameof(BehaviorId), IsUnique = true)]
[Index(nameof(TriggerTime))]
[Index(nameof(TriggerOnStartup))]
public class Context_v1
{
    public int Id { get; set; }

    [StringLength(1024)]
    public string BehaviorClass { get; set; }

    [StringLength(1024)]
    public string BehaviorId { get; set; }

    public DateTime? TriggerTime { get; set; }

    public bool TriggerOnStartup { get; set; }

    public string Data { get; set; }

    public Context_v1(string behaviorClass, string behaviorId, DateTime? triggerTime, bool triggerOnStartup, string data)
    {
        BehaviorClass = behaviorClass;
        BehaviorId = behaviorId;
        TriggerTime = triggerTime;
        TriggerOnStartup = triggerOnStartup;
        Data = data;
    }
}
