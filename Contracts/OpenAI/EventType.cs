using System;
using System.Text.Json.Serialization;

public enum EventType
{
    Message,
    CopilotConfirmation
}

public static class EventTypeExtensions
{
    public static string ToSerializedString(this EventType eventType)
    {
        return eventType switch
        {
            EventType.Message => "message",
            EventType.CopilotConfirmation => "copilot_confirmation",
            _ => throw new ArgumentOutOfRangeException(nameof(eventType), eventType, null)
        };
    }
}