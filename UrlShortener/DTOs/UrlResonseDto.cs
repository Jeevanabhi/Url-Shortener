public class UrlResponseDto
{
    public string ShortCode { get; set; }
    public string ShortUrl { get; set; }
    public string OriginalUrl { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
}