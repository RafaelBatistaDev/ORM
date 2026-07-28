using BlogManager.Data;
using BlogManager.Models;
using BlogManager.Models.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BlogManager.Controllers;

[ApiController]
[Route("api/v1/posts")]
[Produces("application/json")]
public class PostsController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly ILogger<PostsController> _logger;

    public PostsController(AppDbContext context, ILogger<PostsController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Retrieves all blog posts.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<BlogPostResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IEnumerable<BlogPostResponseDto>>> GetAll()
    {
        var posts = await _context.Posts
            .Include(p => p.Author)
            .Include(p => p.Tags)
            .AsNoTracking()
            .OrderByDescending(p => p.PublishedDate)
            .ToListAsync();

        if (posts.Count == 0)
            return NotFound(new ErrorResponseDto
            {
                Type = "https://tools.ietf.org/html/rfc7231#section-6.5.4",
                Title = "No posts found",
                Status = StatusCodes.Status404NotFound,
                Detail = "No blog posts exist in the database."
            });

        var response = posts.Select(MapToResponseDto);
        return Ok(response);
    }

    /// <summary>
    /// Retrieves a single blog post by ID.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(BlogPostResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<BlogPostResponseDto>> GetById(Guid id)
    {
        var post = await _context.Posts
            .Include(p => p.Author)
            .Include(p => p.Tags)
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id);

        if (post is null)
            return NotFound(new ErrorResponseDto
            {
                Type = "https://tools.ietf.org/html/rfc7231#section-6.5.4",
                Title = "Post not found",
                Status = StatusCodes.Status404NotFound,
                Detail = $"No blog post found with ID '{id}'."
            });

        return Ok(MapToResponseDto(post));
    }

    /// <summary>
    /// Creates a new blog post.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(BlogPostResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<BlogPostResponseDto>> Create([FromBody] CreateBlogPostDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var author = await _context.Authors.FindAsync(dto.AuthorId);
        if (author is null)
            return NotFound(new ErrorResponseDto
            {
                Type = "https://tools.ietf.org/html/rfc7231#section-6.5.4",
                Title = "Author not found",
                Status = StatusCodes.Status404NotFound,
                Detail = $"No author found with ID '{dto.AuthorId}'."
            });

        var post = new BlogPost
        {
            Id = Guid.NewGuid(),
            Title = dto.Title.Trim(),
            Content = dto.Content.Trim(),
            CoverImage = dto.CoverImage?.Trim(),
            PublishedDate = DateTime.UtcNow,
            AuthorId = dto.AuthorId,
            Tags = dto.Tags
                .Where(t => !string.IsNullOrWhiteSpace(t))
                .Select(t => new Tag
                {
                    Id = Guid.NewGuid(),
                    Name = t.Trim()
                })
                .ToList()
        };

        _context.Posts.Add(post);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Post created: {PostId} - {Title}", post.Id, post.Title);

        var response = MapToResponseDto(post);
        return CreatedAtAction(nameof(GetById), new { id = post.Id }, response);
    }

    /// <summary>
    /// Updates an existing blog post.
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateBlogPostDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var post = await _context.Posts
            .Include(p => p.Tags)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (post is null)
            return NotFound(new ErrorResponseDto
            {
                Type = "https://tools.ietf.org/html/rfc7231#section-6.5.4",
                Title = "Post not found",
                Status = StatusCodes.Status404NotFound,
                Detail = $"No blog post found with ID '{id}'."
            });

        var authorExists = await _context.Authors.AnyAsync(a => a.Id == dto.AuthorId);
        if (!authorExists)
            return NotFound(new ErrorResponseDto
            {
                Type = "https://tools.ietf.org/html/rfc7231#section-6.5.4",
                Title = "Author not found",
                Status = StatusCodes.Status404NotFound,
                Detail = $"No author found with ID '{dto.AuthorId}'."
            });

        post.Title = dto.Title.Trim();
        post.Content = dto.Content.Trim();
        post.CoverImage = dto.CoverImage?.Trim();
        post.AuthorId = dto.AuthorId;
        post.UpdatedAt = DateTime.UtcNow;

        // Replace tags
        _context.Tags.RemoveRange(post.Tags);
        post.Tags = dto.Tags
            .Where(t => !string.IsNullOrWhiteSpace(t))
            .Select(t => new Tag
            {
                Id = Guid.NewGuid(),
                Name = t.Trim()
            })
            .ToList();

        await _context.SaveChangesAsync();

        _logger.LogInformation("Post updated: {PostId}", post.Id);

        return NoContent();
    }

    /// <summary>
    /// Deletes a blog post.
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var post = await _context.Posts.FindAsync(id);

        if (post is null)
            return NotFound(new ErrorResponseDto
            {
                Type = "https://tools.ietf.org/html/rfc7231#section-6.5.4",
                Title = "Post not found",
                Status = StatusCodes.Status404NotFound,
                Detail = $"No blog post found with ID '{id}'."
            });

        _context.Posts.Remove(post);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Post deleted: {PostId}", id);

        return NoContent();
    }

    private static BlogPostResponseDto MapToResponseDto(BlogPost post)
    {
        return new BlogPostResponseDto
        {
            Id = post.Id,
            Title = post.Title,
            Content = post.Content,
            Tags = post.Tags.Select(t => t.Name).ToList(),
            PublishedDate = post.PublishedDate,
            UpdatedAt = post.UpdatedAt,
            CoverImage = post.CoverImage,
            AuthorName = post.Author?.Name ?? "Unknown",
            AuthorId = post.AuthorId
        };
    }
}
