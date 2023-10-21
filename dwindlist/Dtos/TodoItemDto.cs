using System.ComponentModel.DataAnnotations;

namespace dwindlist.Dtos
{
    public class TodoItemDto
    {
        [Required]
        [MaxLength(64), MinLength(1)]
        public string Label { get; set; } = string.Empty;
    }
}
