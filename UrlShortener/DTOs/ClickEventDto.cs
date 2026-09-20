public class ClickEvent
{
    public int UrlId { get; set; }
    public DateTime ClickedAt { get; set; }
    public required string Referrer { get; set; }
    public string ?IpAddress { get; set; }

}