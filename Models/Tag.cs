using System.ComponentModel.DataAnnotations;

namespace BlogManager.Models;

public class Tag
{
    public Guid Id { get; set; }

    [Required(ErrorMessage = "Tag name is required")]
    [StringLength(50, MinimumLength = 1, ErrorMessage = "Tag name must be between 1 and 50 characters")]
    public string Name { get; set; } = string.Empty;

    public Guid BlogPostId { get; set; }

    // Navigation
    public BlogPost BlogPost { get; set; } = null!;
}
