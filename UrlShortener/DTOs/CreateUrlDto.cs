using System.ComponentModel.DataAnnotations;

public class CreateUrlDTO
{
    [Required]
    [Url]
    public required string OriginalUrl { get; set; }
    public DateTime? ExpiresAt { get; set; }
}