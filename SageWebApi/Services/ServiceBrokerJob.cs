using Microsoft.EntityFrameworkCore;
using Quartz;
using SageWebApi.Models;
using Serilog;

namespace SageWebApi.Services;

public class ServiceBrokerJob(IDbContextFactory<DataContext> contextFactory,
IConfiguration configuration) : IJob
{
    private readonly IDbContextFactory<DataContext> _contextFactory = contextFactory;
    private readonly IConfiguration _configuration = configuration;

    public Task Execute(IJobExecutionContext context)
    {
        try 
        {
            Log.Information("Documents modifiés :");

            using var cn = _contextFactory.CreateDbContext();

            var docs = cn.DocumentChangeDtos.ToList();

            foreach (var doc in docs)
            {
                Log.Information($"   - Document: {doc.NumPiece}");
            }
        }
        catch (Exception ex) 
        {
            Log.Error(ex, "Error during Task execution");
        }
        
        return Task.CompletedTask;
    }
}
