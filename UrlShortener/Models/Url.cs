public class Url
{
    public int UrlId { get; set; }
    public  string ? ShortCode { get; set; }
    public required string OriginalUrl { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }

    // Navigation property — lets EF Core load related clicks if you ask for them
    public ICollection<Click> ?Clicks { get; set; }
}