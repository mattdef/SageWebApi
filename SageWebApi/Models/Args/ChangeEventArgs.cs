namespace SageWebApi.Models;

public class ChangeEventArgs(string recordId, TableChangeType changeType, DateTime timestamp)
{
    public string RecordId { get; } = recordId;
    public TableChangeType ChangeType { get; } = changeType;
    public DateTime Timestamp { get; } = timestamp;
}
