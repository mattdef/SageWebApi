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
            Log.Information("ServiceBrokerJob running Job");
        }
        catch (Exception ex) 
        {
            Log.Error(ex, "Error during Task execution");
        }
        
        return Task.CompletedTask;
    }
}
