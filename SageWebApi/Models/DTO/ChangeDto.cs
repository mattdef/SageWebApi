using System.ComponentModel.DataAnnotations;

namespace SageWebApi.Models.DTO;

public abstract class ChangeDto
{
    public int Id { get; set; }

    [Required]
    public TableChangeType ChangeType { get; set; }

    [Required]
    public DateTime UpdatedDate { get; set; }
}
