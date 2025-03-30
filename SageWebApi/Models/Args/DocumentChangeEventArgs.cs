namespace SageWebApi.Models;

public class DocumentChangeEventArgs(string recordId, TableChangeType changeType, DateTime timestamp, int domaine, int type) : ChangeEventArgs(recordId, changeType, timestamp)
{
    
    public int Domaine { get; } = domaine;
    public int Type { get; } = type;
}
