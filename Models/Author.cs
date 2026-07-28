using System.ComponentModel.DataAnnotations;

namespace BlogManager.Models;

public class Author
{
    public Guid Id { get; set; }

    [Required(ErrorMessage = "Author name is required")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Name must be between 2 and 100 characters")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    [StringLength(200, ErrorMessage = "Email must be at most 200 characters")]
    public string EmailAddress { get; set; } = string.Empty;

    // Navigation
    public ICollection<BlogPost> Posts { get; set; } = new List<BlogPost>();
}
