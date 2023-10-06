using System.ComponentModel.DataAnnotations;

namespace dwindlist.Dtos;

public class TodoItemToggleDto
{
    [Required]
    public int Id { get; set; }
    public string? UserId { get; set; }
}
