using System.Xml.Linq;
using Microsoft.Data.SqlClient;
using SageWebApi.Models.DTO;

namespace SageWebApi.Models;

public class ServiceBrokerMonitor(string connectionString) : IDisposable
{
    private readonly string _connectionString = connectionString;
    private SqlConnection? _connection;
    private CancellationTokenSource? _cancellationTokenSource;
    private Task? _monitoringTask;
    private DocumentChangeEventArgs? checkDocDoubleValueMemory;
    private TiersChangeEventArgs? checkTiersDoubleValueMemory;
    private EcritureChangeEventArgs? checkEcrDoubleValueMemory;
    private EcheanceChangeEventArgs? checkEchDoubleValueMemory;
    
    public event EventHandler<DocumentChangeEventArgs>? DocumentTableChanged;
    public event EventHandler<TiersChangeEventArgs>? TiersTableChanged;
    public event EventHandler<EcritureChangeEventArgs>? EcritureTableChanged;
    public event EventHandler<EcheanceChangeEventArgs>? EcheanceTableChanged;

    public void Start()
    {
        if (_monitoringTask != null && !_monitoringTask.IsCompleted)
            return;

        _cancellationTokenSource = new CancellationTokenSource();
        _monitoringTask = Task.Run(() => MonitorChangesAsync(_cancellationTokenSource.Token));
    }

    public void Stop()
    {
        _cancellationTokenSource?.Cancel();
        
        try
        {
            _monitoringTask?.Wait(TimeSpan.FromSeconds(5));
        }
        catch (AggregateException)
        {
            // Ignorer les exceptions liées à l'annulation
        }
        
        _connection?.Close();
    }

    private async Task MonitorChangesAsync(CancellationToken cancellationToken)
    {
        try
        {
            using (_connection = new SqlConnection(_connectionString))
            {
                await _connection.OpenAsync(cancellationToken);
                
                while (!cancellationToken.IsCancellationRequested)
                {
                    // Commande modifiée pour séparer la réception du message de l'affectation de variable
                    using SqlCommand command = _connection.CreateCommand();
                    command.CommandText = @"
                            DECLARE @ConversationGroupId uniqueidentifier;
                            DECLARE @MessageTypeName nvarchar(256);
                            DECLARE @MessageBody varbinary(max);
                            
                            WAITFOR (
                                RECEIVE TOP(1)
                                    @ConversationGroupId = conversation_group_id,
                                    @MessageTypeName = message_type_name,
                                    @MessageBody = message_body
                                FROM TableModificationQueue
                            ), TIMEOUT 30000;
                            
                            -- Renvoyer les résultats séparément
                            IF (@MessageTypeName IS NOT NULL)
                            BEGIN
                                SELECT @MessageTypeName AS MessageType, 
                                    CAST(@MessageBody AS nvarchar(max)) AS MessageBody;
                            END
                            ELSE
                            BEGIN
                                SELECT 'TIMEOUT' AS MessageType, NULL AS MessageBody;
                            END
                        ";

                    command.CommandTimeout = 60; // Secondes

                    using SqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);
                    if (await reader.ReadAsync(cancellationToken))
                    {
                        string messageType = reader.GetString(0);

                        // Si c'est juste un timeout, continuer la boucle
                        if (messageType == "TIMEOUT")
                            continue;

                        // Si c'est un vrai message, traiter les données
                        if (!reader.IsDBNull(1))
                        {
                            string messageBody = reader.GetString(1);
                            ProcessMessage(messageBody);
                        }
                    }
                }
            }
        }
        catch (OperationCanceledException)
        {
            // Surveillance annulée normalement
        }
        catch (Exception ex)
        {
            // Gérer les autres exceptions
            Console.WriteLine($"Erreur lors de la surveillance: {ex.Message}");
            
            // Redémarrer la surveillance après un délai en cas d'erreur
            if (!cancellationToken.IsCancellationRequested)
            {
                await Task.Delay(5000, cancellationToken);
                if (!cancellationToken.IsCancellationRequested)
                    _monitoringTask = Task.Run(() => MonitorChangesAsync(cancellationToken));
            }
        }
    }

    private void ProcessMessage(string messageBody)
    {
        try
        {
            XDocument xmlDoc = XDocument.Parse(messageBody);
            if (xmlDoc.Root is null)
                return;

            XElement rootElement = xmlDoc.Root;

            switch (rootElement.Name.LocalName)
            {
                case "F_DOCENTETE":
                    DocumentProcessMessage(rootElement);
                    break;
                case "F_COMPTET":
                    TiersProcessMessage(rootElement); 
                    break;
                case "F_ECRITUREC":
                    EcritureProcessMessage(rootElement);
                    break;
                case "F_ECHEANCES": 
                    EcheanceProcessMessage(rootElement);
                    break;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erreur lors du traitement du message: {ex.Message}");
        }
    }

    private void EcheanceProcessMessage(XElement element)
    {
        foreach (XElement modificationElement in element.Elements())
        {
            string operationType = modificationElement.Attribute("OperationType")!.Value;
            string recordID = modificationElement.Attribute("RecordID")!.Value;
            string modificationTime = modificationElement.Attribute("ModificationTime")!.Value;

            if (!string.IsNullOrEmpty(operationType) 
                && !string.IsNullOrEmpty(recordID))
            {
                DateTime timeStamp = DateTime.Parse(modificationTime);
                
                // Convertir le type d'opération en enum
                TableChangeType changeType = operationType switch
                {
                    "INSERT" => TableChangeType.Insert,
                    "UPDATE" => TableChangeType.Update,
                    "DELETE" => TableChangeType.Delete,
                    _ => TableChangeType.Unknown
                };

                // Test doublon
                if (checkEchDoubleValueMemory is not null)
                {
                    if (checkEchDoubleValueMemory.RecordId == recordID
                    && checkEchDoubleValueMemory.Timestamp > timeStamp.AddSeconds(-2))
                    continue;
                }
                checkEchDoubleValueMemory = new EcheanceChangeEventArgs(recordID, changeType, timeStamp);
                
                // Déclencher l'événement
               EcheanceTableChanged?.Invoke(this, new EcheanceChangeEventArgs(recordID, changeType, timeStamp));
            }
        }
    }

    private void EcritureProcessMessage(XElement element)
    {
        foreach (XElement modificationElement in element.Elements())
        {
            string operationType = modificationElement.Attribute("OperationType")!.Value;
            string recordID = modificationElement.Attribute("RecordID")!.Value;
            string modificationTime = modificationElement.Attribute("ModificationTime")!.Value;

            if (!string.IsNullOrEmpty(operationType) 
                && !string.IsNullOrEmpty(recordID))
            {
                DateTime timeStamp = DateTime.Parse(modificationTime);
                
                // Convertir le type d'opération en enum
                TableChangeType changeType = operationType switch
                {
                    "INSERT" => TableChangeType.Insert,
                    "UPDATE" => TableChangeType.Update,
                    "DELETE" => TableChangeType.Delete,
                    _ => TableChangeType.Unknown
                };

                // Test doublon
                if (checkEcrDoubleValueMemory is not null)
                {
                    if (checkEcrDoubleValueMemory.RecordId == recordID
                    && checkEcrDoubleValueMemory.Timestamp > timeStamp.AddSeconds(-2))
                    continue;
                }
                checkEcrDoubleValueMemory = new EcritureChangeEventArgs(recordID, changeType, timeStamp);
                
                // Déclencher l'événement
                EcritureTableChanged?.Invoke(this, new EcritureChangeEventArgs(recordID, changeType, timeStamp));
            }
        }
    }

    private void TiersProcessMessage(XElement element)
    {
        foreach (XElement modificationElement in element.Elements())
        {
            string operationType = modificationElement.Attribute("OperationType")!.Value;
            string recordID = modificationElement.Attribute("RecordID")!.Value;
            string modificationTime = modificationElement.Attribute("ModificationTime")!.Value;
            int type = int.Parse(modificationElement.Attribute("Type")!.Value);
            
            if (!string.IsNullOrEmpty(operationType) 
                && !string.IsNullOrEmpty(recordID))
            {
                DateTime timeStamp = DateTime.Parse(modificationTime);
                
                // Convertir le type d'opération en enum
                TableChangeType changeType = operationType switch
                {
                    "INSERT" => TableChangeType.Insert,
                    "UPDATE" => TableChangeType.Update,
                    "DELETE" => TableChangeType.Delete,
                    _ => TableChangeType.Unknown
                };

                // Test doublon
                if (checkTiersDoubleValueMemory is not null)
                {
                    if (checkTiersDoubleValueMemory.RecordId == recordID
                    && checkTiersDoubleValueMemory.Timestamp > timeStamp.AddSeconds(-2))
                    continue;
                }
                checkTiersDoubleValueMemory = new TiersChangeEventArgs(recordID, changeType, timeStamp, type);
                
                // Déclencher l'événement
                TiersTableChanged?.Invoke(this, new TiersChangeEventArgs(recordID, changeType, timeStamp, type));
            }
        }
    }

    private void DocumentProcessMessage(XElement element)
    {
        foreach (XElement modificationElement in element.Elements())
        {
            string operationType = modificationElement.Attribute("OperationType")!.Value;
            string recordID = modificationElement.Attribute("RecordID")!.Value;
            string modificationTime = modificationElement.Attribute("ModificationTime")!.Value;
            int domaine = int.Parse(modificationElement.Attribute("Domaine")!.Value);
            int type = int.Parse(modificationElement.Attribute("Type")!.Value);
            
            if (!string.IsNullOrEmpty(operationType) 
                && !string.IsNullOrEmpty(recordID))
            {
                DateTime timeStamp = DateTime.Parse(modificationTime);
                
                // Convertir le type d'opération en enum
                TableChangeType changeType = operationType switch
                {
                    "INSERT" => TableChangeType.Insert,
                    "UPDATE" => TableChangeType.Update,
                    "DELETE" => TableChangeType.Delete,
                    _ => TableChangeType.Unknown
                };

                // Test doublon
                if (checkDocDoubleValueMemory is not null)
                {
                    if (checkDocDoubleValueMemory.RecordId == recordID
                    && checkDocDoubleValueMemory.Timestamp > timeStamp.AddSeconds(-2))
                    continue;
                }
                checkDocDoubleValueMemory = new DocumentChangeEventArgs(recordID, changeType, timeStamp, domaine, type);
                
                // Déclencher l'événement
                DocumentTableChanged?.Invoke(this, new DocumentChangeEventArgs(recordID, changeType, timeStamp, domaine, type));
            }
        }
    }

    public void Dispose()
    {
        Stop();
        _cancellationTokenSource?.Dispose();
        _connection?.Dispose();
    }
}
