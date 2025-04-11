using System.ComponentModel.DataAnnotations;

namespace SageWebApi.Models.DTO;

public class EcheanceChangeDto : ChangeDto
{
    [Required]
    public string NumEcheance { get; set; } = "";
}