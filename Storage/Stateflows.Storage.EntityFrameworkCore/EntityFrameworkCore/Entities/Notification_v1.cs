using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Stateflows.Common;
using Stateflows.Common.Utilities;

namespace Stateflows.Storage.EntityFrameworkCore.EntityFrameworkCore.Entities;

[Table("StateflowsNotifications_v1")]
[Index(nameof(SenderType))]
[Index(nameof(SenderName))]
[Index(nameof(SenderInstance))]
public class Notification_v1
{
    public int Id { get; set; }
    
    public string SenderType { get; set; }
    public string SenderName { get; set; }
    public string SenderInstance { get; set; }

    public string Name { get; set; }
    
    public string Data { get; set; }
    
    public int TimeToLive { get; set; }
        
    public bool Retained { get; set; }

    public DateTime SentAt { get; set; }

    public Notification_v1(
        string senderType,
        string senderName,
        string senderInstance,
        string name,
        string data,
        int timeToLive,
        bool retained,
        DateTime sentAt
    )
    {
        SenderType = senderType;
        SenderName = senderName;
        SenderInstance = senderInstance;
        Name = name;
        Data = data;
        TimeToLive = timeToLive;
        Retained = retained;
        SentAt = sentAt;   
    }

    public Notification_v1(EventHolder eventHolder)
    {
        Name = eventHolder.Name;
        Data = StateflowsJsonConverter.SerializePolymorphicObject(eventHolder);
        TimeToLive = eventHolder.TimeToLive;
        Retained = eventHolder.Retained;
        SentAt = eventHolder.SentAt;
        SenderType = eventHolder.SenderId.Value.Type;
        SenderName = eventHolder.SenderId.Value.Name;
        SenderInstance = eventHolder.SenderId.Value.Instance;
    }
}