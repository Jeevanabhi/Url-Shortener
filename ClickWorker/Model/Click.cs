public class Click
{
    public int ClickId { get; set; }
    public int UrlId { get; set; }
    public DateTime ClickedAt { get; set; }
    public required string Referrer { get; set; }
    public string ?IpAddress { get; set; }

    // Navigation property back to the parent Url

}