using System.ComponentModel.DataAnnotations;

namespace BlogManager.Models;

public class BlogPost
{
    public Guid Id { get; set; }

    [Required(ErrorMessage = "Title is required")]
    [StringLength(200, MinimumLength = 3, ErrorMessage = "Title must be between 3 and 200 characters")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Content is required")]
    [MinLength(10, ErrorMessage = "Content must be at least 10 characters")]
    public string Content { get; set; } = string.Empty;

    public DateTime PublishedDate { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    [Url(ErrorMessage = "Cover image must be a valid URL")]
    [StringLength(500, ErrorMessage = "Cover image URL must be at most 500 characters")]
    public string? CoverImage { get; set; }

    public Guid AuthorId { get; set; }

    // Navigation
    public Author Author { get; set; } = null!;
    public ICollection<Tag> Tags { get; set; } = new List<Tag>();
}
