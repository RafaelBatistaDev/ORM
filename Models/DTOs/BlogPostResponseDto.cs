namespace BlogManager.Models.DTOs;

public class BlogPostResponseDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public List<string> Tags { get; set; } = new();
    public DateTime PublishedDate { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? CoverImage { get; set; }
    public string AuthorName { get; set; } = string.Empty;
    public Guid AuthorId { get; set; }
}
