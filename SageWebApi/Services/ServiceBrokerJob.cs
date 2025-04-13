using Microsoft.EntityFrameworkCore;
using Quartz;
using SageWebApi.Models;
using Serilog;
using Dapper;
using Dapper.Contrib.Extensions;

namespace SageWebApi.Services;

public class ServiceBrokerJob(IDbContextFactory<DataContext> prodwareFactory,
SqlConnectionFactory sageFactory,
IConfiguration configuration) : IJob
{
    private readonly IDbContextFactory<DataContext> _prodwareFactory = prodwareFactory;
    private readonly SqlConnectionFactory _sageFactory = sageFactory;
    private readonly IConfiguration _configuration = configuration;

    public Task Execute(IJobExecutionContext context)
    {
        try 
        {
            Log.Information("Documents modifiés :");

            using var cnProd = _prodwareFactory.CreateDbContext();
            using var cnSage = _sageFactory.Create();
            var tiersList = cnProd.TiersChangeDtos.ToList();

            foreach (var tiers in tiersList)
            {
                var query = @"SELECT cbMarq AS Id
                        , CT_Num AS NumTiers
                        , CT_Intitule AS Intitule
                        , CT_Type AS Type
                        , CG_NumPrinc AS ComptePrincipal
                        , CT_Contact AS Contact
                        , CT_Adresse AS Adresse
                        , CT_Complement AS Complement
                        , CT_CodePostal AS CodePostal
                        , CT_Ville AS Ville
                        , CT_Pays AS Pays
                        , CT_Sommeil AS Sommeil
                    FROM F_COMPTET 
                    WHERE CT_Num = @NumTiers";
                var param = new { NumTiers = tiers.NumTiers };
                var tiersSage = cnSage.Query<SageTiers>(query, param: param).FirstOrDefault();

                if (tiersSage != null)
                    Log.Information($"   - Tiers ({tiers.Id}): {tiersSage.NumTiers} - {tiersSage.Intitule}");
            }
        }
        catch (Exception ex) 
        {
            Log.Error(ex, "Error during Task execution");
        }
        
        return Task.CompletedTask;
    }
}
