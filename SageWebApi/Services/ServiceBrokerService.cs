using System.Collections.Concurrent;
using Microsoft.EntityFrameworkCore;
using SageWebApi.Models;
using SageWebApi.Models.DTO;

namespace SageWebApi.Services;

public class ServiceBrokerService(ILogger<ServiceBrokerService> logger,
        IDbContextFactory<DataContext> contextFactory,
        ServiceBrokerMonitor monitor) : BackgroundService
{
    private readonly ILogger<ServiceBrokerService> _logger = logger;
    private ServiceBrokerMonitor? _monitor = monitor;
    private readonly ConcurrentQueue<ChangeEventArgs> _notificationQueue = new();
    private readonly IDbContextFactory<DataContext> _contextFactory = contextFactory;

    // Événement que les composants Blazor peuvent écouter
    public event EventHandler<ChangeEventArgs>? TableChanged;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Service de surveillance des modifications de base de données démarré");

        try
        {
            if (_monitor is not null)
            {
                // S'abonner aux événements de changement
                _monitor.DocumentTableChanged += OnDocumentTableChanged!;
                _monitor.TiersTableChanged += OnTiersTableChanged!;
                _monitor.EcritureTableChanged += OnEcritureTableChanged!;
                _monitor.EcheanceTableChanged += OnEcheanceTableChanged!;

                // Démarrer la surveillance
                _monitor.Start();

                // Maintenir le service en vie jusqu'à l'arrêt de l'application
                while (!stoppingToken.IsCancellationRequested)
                {
                    // Traiter les notifications en attente
                    ProcessNotificationQueue();

                    await Task.Delay(500, stoppingToken);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur dans le service de surveillance des modifications");
        }
        finally
        {
            // Nettoyage
            if (_monitor != null)
            {
                _monitor.DocumentTableChanged -= OnDocumentTableChanged!;
                _monitor.Stop();
                _monitor.Dispose();
            }

            _logger.LogInformation("Service de surveillance des modifications de base de données arrêté");
        }
    }

    private void OnDocumentTableChanged(object sender, DocumentChangeEventArgs e)
    {
        // Mettre la notification dans une file d'attente pour traitement
        _notificationQueue.Enqueue(e);
        _logger.LogInformation($"Modification de document détectée: ID={e.RecordId}, Type={e.ChangeType}");

        // Ajout en BDD
        using var cn = _contextFactory.CreateDbContext();
        cn.DocumentChangeDtos.Add(new DocumentChangeDto
        {
            NumPiece = e.RecordId,
            ChangeType = e.ChangeType,
            UpdatedDate = e.Timestamp,
            Domaine = e.Domaine,
            Type = e.Type
        });
        cn.SaveChanges();
    }

    private void OnTiersTableChanged(object sender, TiersChangeEventArgs e)
    {
        // Mettre la notification dans une file d'attente pour traitement
        _notificationQueue.Enqueue(e);
        _logger.LogInformation($"Modification de tiers détectée: ID={e.RecordId}, Type={e.ChangeType}");

        // Ajout en BDD
        using var cn = _contextFactory.CreateDbContext();
        cn.TiersChangeDtos.Add(new TiersChangeDto
        {
            NumTiers = e.RecordId,
            ChangeType = e.ChangeType,
            UpdatedDate = e.Timestamp,
            Type = e.Type
        });
        cn.SaveChanges();
    }

    private void OnEcritureTableChanged(object sender, EcritureChangeEventArgs e)
    {
        // Mettre la notification dans une file d'attente pour traitement
        _notificationQueue.Enqueue(e);
        _logger.LogInformation($"Modification d'écriture détectée: ID={e.RecordId}, Type={e.ChangeType}");

        // Ajout en BDD
        using var cn = _contextFactory.CreateDbContext();
        cn.EcritureChangeDtos.Add(new EcritureChangeDto
        {
            NumEcriture = e.RecordId,
            ChangeType = e.ChangeType,
            UpdatedDate = e.Timestamp
        });
        cn.SaveChanges();
    }

    private void OnEcheanceTableChanged(object sender, EcheanceChangeEventArgs e)
    {
        // Mettre la notification dans une file d'attente pour traitement
        _notificationQueue.Enqueue(e);
        _logger.LogInformation($"Modification d'échéance détectée: ID={e.RecordId}, Type={e.ChangeType}");

        // Ajout en BDD
        using var cn = _contextFactory.CreateDbContext();
        cn.EcheanceChangeDtos.Add(new EcheanceChangeDto
        {
            NumEcheance = e.RecordId,
            ChangeType = e.ChangeType,
            UpdatedDate = e.Timestamp
        });
        cn.SaveChanges();
    }

    private void ProcessNotificationQueue()
    {
        // Traiter toutes les notifications en attente
        while (_notificationQueue.TryDequeue(out var notification))
        {
            // Déclencher l'événement que les composants Blazor peuvent écouter
            TableChanged?.Invoke(this, notification);
        }
    }

    // Méthode pour obtenir explicitement les dernières modifications
    public async Task<ChangeEventArgs[]> GetRecentChangesAsync()
    {
        return await Task.FromResult<ChangeEventArgs[]>([.. _notificationQueue]);
    }
}
