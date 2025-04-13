using Dapper.Contrib.Extensions;

namespace SageWebApi.Models;

[Table("F_COMPTET")]
public class SageTiers
{
    public int Id { get; set; }
    public string? NumTiers { get; set; }
    public string? Intitule { get; set; }
    public short Type { get; set; }
    public string? ComptePrincipal { get; set; }
    public string? Contact { get; set; }
    public string? Adresse { get; set; }
    public string? Complement { get; set; }
    public string? CodePostal { get; set; }
    public string? Ville { get; set; }
    public string? Pays { get; set; }
    public bool Sommeil { get; set; }
}
