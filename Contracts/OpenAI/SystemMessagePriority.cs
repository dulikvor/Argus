namespace Argus.Contracts.OpenAI
{
    public enum SystemMessagePriority
    {
        Low = 0,    // Routine or background system message
        Medium = 1, // Important but not urgent
        High = 2    // Urgent, critical, or must be addressed immediately
    }
}
