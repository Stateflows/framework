using Stateflows.Common;
using Stateflows.Common.Utilities;

namespace Stateflows;

[GenerateSerializer]
[Alias("Stateflows.OrleansEventHolder")]
public struct OrleansEventHolder
{
    [Id(0)]
    public Guid Id { get; set; }
    
    [Id(1)]
    public string Name { get; set; }
    
    [Id(2)]
    public string Payload { get; set; }
    
    [Id(3)]
    public Dictionary<string, string> Headers { get; set; }
        
    [Id(4)]
    public int TimeToLive { get; set; }
        
    [Id(5)]
    public bool Retained { get; set; }

    [Id(6)]
    public DateTime SentAt { get; set; }

    [Id(7)]
    public OrleansBehaviorId? SenderId { get; set; }
    
    public static implicit operator EventHolder(OrleansEventHolder orleansEventHolder)
    {
        var payload = StateflowsJsonConverter.DeserializeObject(orleansEventHolder.Payload);
        var result = payload.ToTypedEventHolder(orleansEventHolder.Headers.ToDictionary(
            p => p.Key,
            p => (EventHeader)StateflowsJsonConverter.DeserializeObject(p.Value)
        ));
        result.Id = orleansEventHolder.Id;
        result.Retained = orleansEventHolder.Retained;
        result.SenderId = orleansEventHolder.SenderId;
        result.SentAt = orleansEventHolder.SentAt;
        result.TimeToLive = orleansEventHolder.TimeToLive;
     
        return result;
    }

    public static implicit operator OrleansEventHolder(EventHolder eventHolder)
        => new OrleansEventHolder()
        {
            Payload = StateflowsJsonConverter.SerializePolymorphicObject(eventHolder.BoxedPayload),
            Headers = eventHolder.Headers.ToDictionary(
                p => p.Key,
                p => StateflowsJsonConverter.SerializePolymorphicObject(p.Value)
            ),
            Id = eventHolder.Id,
            Name = eventHolder.Name,
            Retained = eventHolder.Retained,
            SenderId = eventHolder.SenderId,
            SentAt = eventHolder.SentAt,
            TimeToLive = eventHolder.TimeToLive
        };
}

[GenerateSerializer]
[Alias("Stateflows.OrleansEventHolder`1")]
public struct OrleansEventHolder<TEvent>
{
    [Id(1)]
    public string Payload { get; set; }
    
    [Id(2)]
    public Dictionary<string, string> Headers { get; set; }
        
    [Id(3)]
    public int TimeToLive { get; set; }
        
    [Id(4)]
    public bool Retained { get; set; }

    [Id(5)]
    public DateTime SentAt { get; set; }

    [Id(6)]
    public OrleansBehaviorId? SenderId { get; set; }
    
    public static implicit operator EventHolder<TEvent>(OrleansEventHolder<TEvent> orleansEventHolder)
    {
        var payload = (TEvent)StateflowsJsonConverter.DeserializeObject(orleansEventHolder.Payload);
        var result = payload.ToEventHolder(orleansEventHolder.Headers.ToDictionary(
            p => p.Key,
            p => (EventHeader)StateflowsJsonConverter.DeserializeObject(p.Value)
        ));
        result.Retained = orleansEventHolder.Retained;
        result.SenderId = orleansEventHolder.SenderId;
        result.SentAt = orleansEventHolder.SentAt;
        result.TimeToLive = orleansEventHolder.TimeToLive;
     
        return result;
    }

    public static implicit operator OrleansEventHolder<TEvent>(EventHolder<TEvent> eventHolder)
        => new OrleansEventHolder<TEvent>()
        {
            Payload = StateflowsJsonConverter.SerializePolymorphicObject(eventHolder.Payload),
            Headers = eventHolder.Headers.ToDictionary(
                p => p.Key,
                p => StateflowsJsonConverter.SerializePolymorphicObject(p.Value)
            ),
            Retained = eventHolder.Retained,
            SenderId = eventHolder.SenderId,
            SentAt = eventHolder.SentAt,
            TimeToLive = eventHolder.TimeToLive
        };
}