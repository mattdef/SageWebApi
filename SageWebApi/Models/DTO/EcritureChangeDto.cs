using System.ComponentModel.DataAnnotations;

namespace SageWebApi.Models.DTO;

public class EcritureChangeDto : ChangeDto
{
    [Required]
    public string NumEcriture { get; set; } = "";
}
