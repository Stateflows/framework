namespace Stateflows.Extensions.MinimalAPIs.Attributes;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public sealed class NotificationWatchAttribute<TNotification> : Attribute;