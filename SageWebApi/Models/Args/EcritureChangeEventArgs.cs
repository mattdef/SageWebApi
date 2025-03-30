namespace SageWebApi.Models;

public class EcritureChangeEventArgs(string recordId, TableChangeType changeType, DateTime timestamp) : ChangeEventArgs(recordId, changeType, timestamp)
{
    
}
