using Stateflows.Extensions.MinimalAPIs.Attributes;

namespace Stateflows.Examples.Common.Events;

[NotificationWatch<DataNotification>]
public class GetData
{
    public DateTime From { get; set; }
    public DateTime To { get; set; }
}