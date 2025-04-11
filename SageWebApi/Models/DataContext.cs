using Microsoft.EntityFrameworkCore;
using SageWebApi.Models.DTO;

namespace SageWebApi.Models;

public class DataContext(DbContextOptions<DataContext> options) : DbContext(options)
{
    public DbSet<DocumentChangeDto> DocumentChangeDtos { get; set; }
    public DbSet<TiersChangeDto> TiersChangeDtos { get; set; }
    public DbSet<EcritureChangeDto> EcritureChangeDtos { get; set; }
    public DbSet<EcheanceChangeDto> EcheanceChangeDtos { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
    }
}
