using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace dwindlist.Models;

[Index("UserId")]
public class TodoItem
{
    [Key]
    public int Id { get; set; }
    [Required]
    public string UserId { get; set; }
    public virtual IdentityUser User { get; set; }
    [Required]
    [MaxLength(256), MinLength(1)]
    public string Label { get; set; }
    public int ParentId { get; set; } = 0;
    public char Status { get; set; } = 'i';
    public char Expanded { get; set; } = 'c';
}
