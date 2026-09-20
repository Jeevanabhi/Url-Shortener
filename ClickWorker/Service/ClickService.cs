public class ClickService
{
    private readonly UrlShorten _context;
    public ClickService(UrlShorten context)
    {
        _context = context;
    }

    public async Task SaveClickToDb(ClickEvent clickdto)
    {
        var clickdetails = new Click
        {
            UrlId = clickdto.UrlId,
            Referrer = clickdto.Referrer,
            IpAddress = clickdto.IpAddress,
            ClickedAt = clickdto.ClickedAt
        };
        await _context.Clicks.AddAsync(clickdetails);
        await _context.SaveChangesAsync();
    }
}