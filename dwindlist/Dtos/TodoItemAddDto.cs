using System.ComponentModel.DataAnnotations;

namespace dwindlist.Dtos;

public class TodoItemAddDto
{
    public int? Id { get; set; }
    public string? UserId { get; set; }
    [Required]
    [MaxLength(256), MinLength(1)]
    public string Label { get; set; }
    public int ParentId { get; set; } = 0;
    public char Status { get; set; } = 'i';
}
