using System.ComponentModel.DataAnnotations;

namespace dwindlist.Models.DB;

public class TodoItem
{
    [Key]
    public int Id { get; set; }
    [Required]
    [MaxLength(450)]
    public string UserId { get; set; }
    [Required]
    [MaxLength(256), MinLength(1)]
    public string Label { get; set; }
    [Required]
    public int ParentId { get; set; }
    [Required]
    public char Status { get; set; }
}
