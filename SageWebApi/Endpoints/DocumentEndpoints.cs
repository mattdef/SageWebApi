using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SageWebApi.Models;

namespace SageWebApi.Endpoints;

public static class DocumentEndpoints
{
    public static void MapDocumentEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/documents", async ([FromServices]DataContext context) => {
            return await context.DocumentChangeDtos.ToListAsync();
        })
        .WithName("GetDocuments");
    }
}
