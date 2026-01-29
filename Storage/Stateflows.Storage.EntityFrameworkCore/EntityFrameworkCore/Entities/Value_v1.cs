using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Stateflows.Storage.EntityFrameworkCore.EntityFrameworkCore.Entities;

[Table("StateflowsValues_v1")]
[Index(nameof(BehaviorType))]
[Index(nameof(BehaviorName))]
[Index(nameof(BehaviorInstance))]
public class Value_v1
{
    public int Id { get; set; }
    
    public string BehaviorType { get; set; }
    public string BehaviorName { get; set; }
    public string BehaviorInstance { get; set; }

    public string Key { get; set; }
    
    public string Value { get; set; }
    
    public int Version { get; set; }

    public Value_v1(
        string behaviorType,
        string behaviorName,
        string behaviorInstance,
        string key,
        string value,
        int version = 0
    )
    {
        BehaviorType = behaviorType;
        BehaviorName = behaviorName;
        BehaviorInstance = behaviorInstance;
        Key = key;
        Value = value;
        Version = version;
    }
}