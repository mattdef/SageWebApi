using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace SageWebApi.Models.DTO;

public class DocumentChangeDto : ChangeDto
{
    [Required]
    public string NumPiece { get; set; } = "";

    [DefaultValue(0)]
    public int Domaine { get; set; }

    [DefaultValue(0)]
    public int Type { get; set; }
}
