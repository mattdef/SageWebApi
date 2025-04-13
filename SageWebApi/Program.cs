using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Quartz;
using SageWebApi.Models;
using SageWebApi.Services;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);

builder.Host.ConfigureSerilog();

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddDbContextFactory<DataContext>(opt => 
    opt.UseSqlServer(builder.Configuration.GetConnectionString("ProdwareConnection"))
    .EnableSensitiveDataLogging());
builder.Services.AddSingleton<ServiceBrokerService>();
builder.Services.AddHostedService(provider => provider.GetRequiredService<ServiceBrokerService>());

builder.Services.AddQuartz(q => 
{
    var jobKey = new JobKey("ServiceBrokerJob");
    q.AddJob<ServiceBrokerJob>(opts => opts.WithIdentity(jobKey));

    q.AddTrigger(opts => opts
    .ForJob(jobKey)
    .WithIdentity("ServiceBrokerJob-trigger")
    .WithCronSchedule("0 * * ? * *")
    );
});
builder.Services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapGet("/documents", async ([FromServices]DataContext context) => {
    return await context.DocumentChangeDtos.ToListAsync();
})
.WithName("GetDocuments");

app.MapGet("/ecritures", async ([FromServices]DataContext context) => {
    return await context.EcritureChangeDtos.ToListAsync();
})
.WithName("GetEcritures");

app.MapGet("/tiers", async ([FromServices]DataContext context) => {
    return await context.TiersChangeDtos.ToListAsync();
})
.WithName("GetTiers");

app.MapGet("/echeances", async ([FromServices]DataContext context) => {
    return await context.EcheanceChangeDtos.ToListAsync();
})
.WithName("GetEcheances");

app.Run();