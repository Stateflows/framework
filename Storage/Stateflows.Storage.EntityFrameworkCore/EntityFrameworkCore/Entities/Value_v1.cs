using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Stateflows.Storage.EntityFrameworkCore.EntityFrameworkCore.Entities;

/// <summary>Entity Framework row storing one value associated with a Stateflows behavior instance.</summary>
[Table("StateflowsValues_v1")]
[Index(nameof(BehaviorType))]
[Index(nameof(BehaviorName))]
[Index(nameof(BehaviorInstance))]
public class Value_v1
{
    /// <summary>Gets or sets the database-generated row identifier.</summary>
    public int Id { get; set; }
    
    /// <summary>Gets or sets the behavior type associated with the value.</summary>
    public string BehaviorType { get; set; }
    /// <summary>Gets or sets the behavior name associated with the value.</summary>
    public string BehaviorName { get; set; }
    /// <summary>Gets or sets the behavior instance key associated with the value.</summary>
    public string BehaviorInstance { get; set; }

    /// <summary>Gets or sets the value's key within the behavior's value store.</summary>
    public string Key { get; set; }
    
    /// <summary>Gets or sets the serialized value payload.</summary>
    public string Value { get; set; }

    /// <summary>Creates a persisted value row for the specified behavior identity and key.</summary>
    /// <param name="behaviorType">The behavior type associated with the value.</param>
    /// <param name="behaviorName">The behavior name associated with the value.</param>
    /// <param name="behaviorInstance">The behavior instance key associated with the value.</param>
    /// <param name="key">The value-store key.</param>
    /// <param name="value">The serialized value payload.</param>
    public Value_v1(
        string behaviorType,
        string behaviorName,
        string behaviorInstance,
        string key,
        string value
    )
    {
        BehaviorType = behaviorType;
        BehaviorName = behaviorName;
        BehaviorInstance = behaviorInstance;
        Key = key;
        Value = value;
    }
}