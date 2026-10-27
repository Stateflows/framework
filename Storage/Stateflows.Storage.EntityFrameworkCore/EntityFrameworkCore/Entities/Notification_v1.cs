using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Stateflows.Common;
using Stateflows.Common.Utilities;

namespace Stateflows.Storage.EntityFrameworkCore.EntityFrameworkCore.Entities;

/// <summary>Entity Framework row storing a serialized Stateflows notification and its retention metadata.</summary>
[Table("StateflowsNotifications_v1")]
[Index(nameof(SenderType))]
[Index(nameof(SenderName))]
[Index(nameof(SenderInstance))]
public class Notification_v1
{
    /// <summary>Gets or sets the database-generated row identifier.</summary>
    public int Id { get; set; }
    
    /// <summary>Gets or sets the sender behavior type.</summary>
    public string SenderType { get; set; }
    /// <summary>Gets or sets the sender behavior name.</summary>
    public string SenderName { get; set; }
    /// <summary>Gets or sets the sender behavior instance key.</summary>
    public string SenderInstance { get; set; }

    /// <summary>Gets or sets the notification event name.</summary>
    public string Name { get; set; }
    
    /// <summary>Gets or sets the serialized event holder payload.</summary>
    public string Data { get; set; }
    
    /// <summary>Gets or sets the notification time-to-live in seconds.</summary>
    public int TimeToLive { get; set; }
        
    /// <summary>Gets or sets whether the notification is retained after its time-to-live expires.</summary>
    public bool Retained { get; set; }

    /// <summary>Gets or sets the time the notification was sent.</summary>
    public DateTime SentAt { get; set; }

    /// <summary>Creates a notification row from its sender identity, payload, and retention settings.</summary>
    /// <param name="senderType">The sender behavior type.</param>
    /// <param name="senderName">The sender behavior name.</param>
    /// <param name="senderInstance">The sender behavior instance key.</param>
    /// <param name="name">The notification event name.</param>
    /// <param name="data">The serialized notification payload.</param>
    /// <param name="timeToLive">The time-to-live in seconds.</param>
    /// <param name="retained">Whether the notification is retained.</param>
    /// <param name="sentAt">The notification send time.</param>
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

    /// <summary>Creates a notification row by serializing an event holder.</summary>
    /// <param name="eventHolder">The notification holder to persist, including sender and retention metadata.</param>
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