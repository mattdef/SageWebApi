using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SageWebApi.Models;

namespace SageWebApi.Endpoints;

public static class EcritureEndpoints
{
    public static void MapEcritureEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/ecritures", async ([FromServices]DataContext context) => {
            return await context.EcritureChangeDtos.ToListAsync();
        })
        .WithName("GetEcritures");
    }
}
