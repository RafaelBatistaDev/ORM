namespace BlogManager.Models.DTOs;

public class ErrorResponseDto
{
    public string Type { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public int Status { get; set; }
    public string Detail { get; set; } = string.Empty;
    public string? TraceId { get; set; }
    public IReadOnlyDictionary<string, string[]>? Errors { get; set; }
}
