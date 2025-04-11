namespace SageWebApi.Models;

public class EcheanceChangeEventArgs(string recordId, TableChangeType changeType, DateTime timestamp) : ChangeEventArgs(recordId, changeType, timestamp)
{
    
}