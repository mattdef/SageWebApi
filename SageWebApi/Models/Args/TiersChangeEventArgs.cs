namespace SageWebApi.Models;

public class TiersChangeEventArgs(string recordId, TableChangeType changeType, DateTime timestamp, int type) : ChangeEventArgs(recordId, changeType, timestamp)
{
    public int Type { get; } = type;
}
