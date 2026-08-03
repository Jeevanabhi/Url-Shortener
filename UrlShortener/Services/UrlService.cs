using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;

public class UrlService
{
    private const string Chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
    private readonly UrlShorten _context;
    private readonly ILogger<UrlService> _logger;
    public UrlService(UrlShorten context,ILogger<UrlService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<string> CreateShortUrlAsync(CreateUrlDTO dto)
    {


        int length = 7;
        var chars = new char[length];
        string code;
        do
        {
            for (int i = 0; i < length; i++)
            {
                chars[i] = Chars[Random.Shared.Next(Chars.Length)];
            }
            code = new string(chars);

        } while (await _context.Urls.AnyAsync(u => u.ShortCode == code));
        var newUrl = new Url
        {
            ShortCode = code,
            OriginalUrl = dto.OriginalUrl,
            CreatedAt = DateTime.UtcNow
        };

        _context.Urls.Add(newUrl);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Created a ShortCode for {OriginalUrl} and Saved to Db", dto.OriginalUrl);

        return code;
    }
    public async Task<Url?> GetByShortCodeAsync(string shortCode)
    {
        return await _context.Urls
            .FirstOrDefaultAsync(u => u.ShortCode == shortCode);
    }

    public async Task<Click> LogClicks(int urlId, string Referrer, string IpAddress)
    {
        var click = new Click
        {
            UrlId = urlId,
            Referrer = Referrer,
            IpAddress = IpAddress,
            ClickedAt = DateTime.UtcNow
        };

        _context.Clicks.Add(click);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Clicked Url with Id:{urlID}", urlId);
        return click;

    }
    public async Task<UrlStatsDto?> GetStatsAsync(string shortCode)
    {
        var url = await GetByShortCodeAsync(shortCode);
        if (url == null) return null;

        var totalClicks = await _context.Clicks.CountAsync(c => c.UrlId == url.UrlId);

        var clicksPerDay = await _context.Clicks
            .Where(c => c.UrlId == url.UrlId)
            .GroupBy(c => c.ClickedAt.Date)
            .Select(g => new ClicksByDayDto { Date = g.Key, Count = g.Count() })
            .OrderBy(g => g.Date)
            .ToListAsync();

        return new UrlStatsDto
        {
            ShortCode = url.ShortCode,
            OriginalUrl = url.OriginalUrl,
            TotalClicks = totalClicks,
            ClicksPerDay = clicksPerDay
        };
    }

}

