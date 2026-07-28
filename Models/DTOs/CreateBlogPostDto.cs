using System.ComponentModel.DataAnnotations;

namespace BlogManager.Models.DTOs;

public class CreateBlogPostDto
{
    [Required(ErrorMessage = "Title is required")]
    [StringLength(200, MinimumLength = 3, ErrorMessage = "Title must be between 3 and 200 characters")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Content is required")]
    [MinLength(10, ErrorMessage = "Content must be at least 10 characters")]
    public string Content { get; set; } = string.Empty;

    [Url(ErrorMessage = "Cover image must be a valid URL")]
    [StringLength(500, ErrorMessage = "Cover image URL must be at most 500 characters")]
    public string? CoverImage { get; set; }

    [Required(ErrorMessage = "Author ID is required")]
    public Guid AuthorId { get; set; }

    public List<string> Tags { get; set; } = new();
}
