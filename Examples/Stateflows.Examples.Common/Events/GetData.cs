using Stateflows.Extensions.MinimalAPIs.Attributes;

namespace Stateflows.Examples.Common.Events;

public class BaseGetData
{
    public string[] Scope { get; set; } = Array.Empty<string>();
}

[NotificationWatch<List<Dictionary<string, DataNotification>>>]
public class GetData : BaseGetData
{
    public DateTime From { get; set; }
    public DateTime To { get; set; }
}