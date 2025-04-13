using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace SageWebApi.Models.DTO;

public class TiersChangeDto : ChangeDto
{
    [Required]
    public string NumTiers { get; set; } = "";

    [DefaultValue(0)]
    public int Type { get; set; }
}
