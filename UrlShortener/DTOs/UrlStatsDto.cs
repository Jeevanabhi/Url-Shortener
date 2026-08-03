public class UrlStatsDto
{
    public string ShortCode { get; set; }
    public string OriginalUrl { get; set; }
    public int TotalClicks { get; set; }
    public List<ClicksByDayDto> ClicksPerDay { get; set; }
}

public class ClicksByDayDto
{
    public DateTime Date { get; set; }
    public int Count { get; set; }
}