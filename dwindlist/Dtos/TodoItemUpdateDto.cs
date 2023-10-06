using System.ComponentModel.DataAnnotations;

namespace dwindlist.Dtos;

public class TodoItemUpdateDto
{
    [Required]
    [MaxLength(256), MinLength(1)]
    public string Label { get; set; } = "";
}
