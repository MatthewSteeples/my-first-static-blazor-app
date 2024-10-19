using System;

namespace BlazorApp.Shared;

public class NotificationSubscription
{
    public int? NotificationSubscriptionId { get; set; }

    public string? UserId { get; set; }

    public string? Url { get; set; }

    public string? P256dh { get; set; }

    public string? Auth { get; set; }

    public DateTime TargetTime { get; set; }

    public string Message { get; set; }
}