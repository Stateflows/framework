namespace Stateflows;

[GenerateSerializer]
[Alias("Stateflows.OrleansBehaviorClass")]
public struct OrleansBehaviorClass
{
    [Id(0)]
    public string Type { get; set; }
    
    [Id(1)]
    public string Name { get; set; }
    
    public static implicit operator OrleansBehaviorClass(BehaviorClass behaviorClass)
        => new OrleansBehaviorClass
        {
            Type = behaviorClass.Type,
            Name = behaviorClass.Name
        };
    
    public static implicit operator BehaviorClass(OrleansBehaviorClass behaviorClass)
        => new BehaviorClass
        {
            Type = behaviorClass.Type,
            Name = behaviorClass.Name
        };
}