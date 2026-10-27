namespace Stateflows.Extensions.MinimalAPIs.Attributes;

/// <summary>Declares a notification type that HTTP clients can request while interacting with a behavior.</summary>
/// <typeparam name="TNotification">Notification payload type exposed by the generated API.</typeparam>
/// <remarks>Apply multiple instances to expose several notification types.</remarks>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public sealed class NotificationWatchAttribute<TNotification> : Attribute;