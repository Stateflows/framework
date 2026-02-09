namespace Stateflows;

[GenerateSerializer]
[Alias("Stateflows.OrleansBehaviorId")]
public struct OrleansBehaviorId
{
    [Id(0)]
    public string Type { get; set; }
    
    [Id(1)]
    public string Name { get; set; }
    
    [Id(2)]
    public string Instance { get; set; }
    
    public static implicit operator OrleansBehaviorId(BehaviorId behaviorId)
        => new OrleansBehaviorId
        {
            Type = behaviorId.Type,
            Name = behaviorId.Name,
            Instance = behaviorId.Instance
        };
    
    public static implicit operator BehaviorId(OrleansBehaviorId behaviorId)
        => new BehaviorId
        {
            BehaviorClass = new BehaviorClass()
            {
                Type = behaviorId.Type,
                Name = behaviorId.Name
            },
            Instance = behaviorId.Instance
        };
}