using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;
using System.Text.Json;
using StackExchange.Redis;
using System.Text;

public class UrlService
{
    private const string Chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
    private readonly UrlShorten _context;
    private readonly ILogger<UrlService> _logger;
    private readonly IDatabase _cache;
    public UrlService(UrlShorten context,ILogger<UrlService> logger,IConnectionMultiplexer redis)
    {
        _context = context;
        _logger = logger;
        _cache = redis.GetDatabase();
    }

    public async Task<string> CreateShortUrlAsync(CreateUrlDTO dto)
    {


        
        var newUrl = new Url
        {
            OriginalUrl = dto.OriginalUrl,
            CreatedAt = DateTime.UtcNow
        };

        _context.Urls.Add(newUrl);
        await _context.SaveChangesAsync();
        var code = Base62Encoder.Encode(newUrl.UrlId);
        newUrl.ShortCode = code;
        await _context.SaveChangesAsync();
        _logger.LogInformation("Created a ShortCode for {OriginalUrl} and Saved to Db", dto.OriginalUrl);

        return code;
    }
    public async Task<Url?> GetByShortCodeAsync(string shortCode)
    {
        // var cachedUrlJson = await _cache.StringGetAsync(shortCode);
        // if (cachedUrlJson.HasValue)
        // {
        //     _logger.LogInformation("Cache Hit for {shortCode}", shortCode);
        //     var cachedUrl = JsonSerializer.Deserialize<CachedUrlDto>(cachedUrlJson);
        //     if (cachedUrl is not null)
        //     {
        //         return new Url
        //         {
        //             UrlId = cachedUrl.UrlId,
        //             ShortCode = shortCode,
        //             OriginalUrl = cachedUrl.OriginalUrl
        //         };
        //     }
        // }
         _logger.LogInformation("Cache Miss for {shortCode}", shortCode);
        var url = await _context.Urls.FirstOrDefaultAsync(u => u.ShortCode == shortCode);
        if (url is not null)
        {
            CachedUrlDto dto = new CachedUrlDto
            {
                UrlId = url.UrlId,
                OriginalUrl = url.OriginalUrl
            };
            var jsondto = JsonSerializer.Serialize<CachedUrlDto>(dto);
            await _cache.StringSetAsync(shortCode,jsondto, TimeSpan.FromHours(10));
        }
        return url;
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

